using System.Globalization;
using MidtermSprint.Application.Abstractions;
using MidtermSprint.Domain.Entities;

namespace MidtermSprint.Application.Search;

/// <summary>
/// Runs craving interpretation, candidate filtering, scoring, allergen tiering, and explanations.
/// </summary>
public sealed class SearchService(
    IQueryInterpreter interpreter,
    IRestaurantRepository restaurantRepository,
    SearchOptions options,
    TimeProvider timeProvider)
    : ISearchService
{
    private const double RadiusBoundaryToleranceKm = 1e-9;
    private readonly SearchScorer scorer = new(options.ScoringWeights);
    private readonly OpenNowEvaluator openNowEvaluator = new();
    private readonly MatchExplanationBuilder explanationBuilder = new();

    public async Task<SearchResult> SearchAsync(
        SearchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var parsed = interpreter.Interpret(request.Query);
        var specification = SearchSpecificationMerger.Merge(
            parsed.Specification,
            request.Filters ?? new ManualSearchFilters(),
            options);
        ValidateSpecification(specification);

        var originLatitude = specification.OriginLatitude ?? options.DefaultLatitude;
        var originLongitude = specification.OriginLongitude ?? options.DefaultLongitude;
        var bounds = GeoMath.CreateBounds(originLatitude, originLongitude, specification.RadiusKm);
        var candidates = await restaurantRepository.GetPublishedWithinBoundsAsync(bounds, cancellationToken);
        var verified = new List<RestaurantMatch>();
        var unverified = new List<RestaurantMatch>();
        var now = timeProvider.GetUtcNow();

        foreach (var restaurant in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var distance = GeoMath.DistanceKm(
                originLatitude,
                originLongitude,
                restaurant.Latitude,
                restaurant.Longitude);
            if (distance > specification.RadiusKm + RadiusBoundaryToleranceKm ||
                !MatchesRestaurantConstraints(restaurant, specification, now))
            {
                continue;
            }

            var eligibleItems = new List<(MenuItem Item, AllergenMatchTier Tier)>();
            foreach (var item in restaurant.MenuItems.Where(item => item.IsAvailable))
            {
                if (!MatchesItemConstraints(item, specification))
                {
                    continue;
                }

                var allergenResult = AllergenEligibility.Evaluate(
                    item,
                    specification.AllergensToAvoid,
                    specification.StrictAllergenMode);
                if (allergenResult.Tier != AllergenMatchTier.Excluded)
                {
                    eligibleItems.Add((item, allergenResult.Tier));
                }
            }

            if (eligibleItems.Count == 0)
            {
                continue;
            }

            var match = BuildRestaurantMatch(restaurant, eligibleItems, specification, distance);
            var hasVerifiedItem = eligibleItems.Any(item => item.Tier == AllergenMatchTier.Verified);
            if (hasVerifiedItem)
            {
                var verifiedItems = eligibleItems
                    .Where(item => item.Tier == AllergenMatchTier.Verified)
                    .Select(item => item.Item)
                    .ToArray();
                verified.Add(BuildRestaurantMatch(
                    restaurant,
                    eligibleItems.Where(item => item.Tier == AllergenMatchTier.Verified).ToArray(),
                    specification,
                    distance,
                    verifiedItems));
            }
            else
            {
                unverified.Add(match);
            }
        }

        var sortedVerified = Sort(verified, specification.Sort).Take(options.MaximumResults).ToArray();
        var remainingCapacity = Math.Max(0, options.MaximumResults - sortedVerified.Length);
        var sortedUnverified = Sort(unverified, specification.Sort).Take(remainingCapacity).ToArray();
        var totalCount = sortedVerified.Length + sortedUnverified.Length;
        return new SearchResult(
            sortedVerified,
            sortedUnverified,
            totalCount,
            specification,
            totalCount == 0 ? BuildSuggestions(specification) : Array.Empty<RelaxationSuggestion>());
    }

    private RestaurantMatch BuildRestaurantMatch(
        Restaurant restaurant,
        IReadOnlyList<(MenuItem Item, AllergenMatchTier Tier)> eligibleItems,
        SearchSpecification specification,
        double distanceKm,
        IReadOnlyList<MenuItem>? itemsOverride = null)
    {
        var items = itemsOverride ?? eligibleItems.Select(pair => pair.Item).ToArray();
        var ranked = items
            .Select(item => (Item: item, Score: scorer.Score(item, restaurant, specification, distanceKm)))
            .OrderByDescending(pair => pair.Score)
            .ThenBy(pair => pair.Item.Price)
            .ToArray();
        var best = ranked[0];
        var (reasons, summary) = explanationBuilder.Build(
            items,
            best.Item,
            restaurant,
            specification,
            distanceKm);
        return new RestaurantMatch(
            restaurant,
            items,
            best.Item,
            distanceKm,
            best.Score,
            reasons,
            summary);
    }

    private bool MatchesRestaurantConstraints(
        Restaurant restaurant,
        SearchSpecification specification,
        DateTimeOffset now)
    {
        if (specification.ServiceType == ServiceType.DineIn && !restaurant.SupportsDineIn ||
            specification.ServiceType == ServiceType.Takeout && !restaurant.SupportsTakeout)
        {
            return false;
        }

        if (specification.OpenNow && !openNowEvaluator.IsOpenNow(restaurant, now))
        {
            return false;
        }

        return !specification.MinRating.HasValue ||
               restaurant.Rating.HasValue && restaurant.Rating.Value >= specification.MinRating.Value;
    }

    private static bool MatchesItemConstraints(MenuItem item, SearchSpecification specification)
    {
        if (specification.PriceIsStrict && specification.MaxPrice is { } budget && item.Price > budget)
        {
            return false;
        }

        foreach (var diet in specification.DietaryPreferences)
        {
            if (!item.DietaryAttributes.Any(attribute =>
                    attribute.IsVerified &&
                    string.Equals(
                        attribute.DietaryPreference?.Name,
                        diet,
                        StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
        }

        return true;
    }

    private static IEnumerable<RestaurantMatch> Sort(
        IEnumerable<RestaurantMatch> matches,
        SearchSort sort) =>
        sort switch
        {
            SearchSort.Nearest => matches.OrderBy(match => match.DistanceKm)
                .ThenByDescending(match => match.Score),
            SearchSort.LowestPrice => matches.OrderBy(match => match.BestMatchingMenuItem.Price)
                .ThenByDescending(match => match.Score),
            SearchSort.HighestRating => matches
                .OrderByDescending(match => match.Restaurant.Rating ?? 0)
                .ThenByDescending(match => match.Score),
            _ => matches.OrderByDescending(match => match.Score)
                .ThenBy(match => match.DistanceKm)
        };

    private static IReadOnlyList<RelaxationSuggestion> BuildSuggestions(SearchSpecification specification)
    {
        var suggestions = new List<RelaxationSuggestion>();
        if (specification.RadiusKm < 50)
        {
            var expandedRadius = Math.Min(specification.RadiusKm * 2, 50);
            suggestions.Add(new RelaxationSuggestion(
                RelaxationAction.ExpandRadius,
                $"Expand the search radius to {expandedRadius:0.##} km.",
                RadiusKm: expandedRadius));
        }

        if (specification.MaxPrice.HasValue)
        {
            var raisedBudget = Math.Ceiling(specification.MaxPrice.Value * 1.25m);
            suggestions.Add(new RelaxationSuggestion(
                RelaxationAction.RaiseBudget,
                $"Raise the menu-item budget to ₱{raisedBudget:0.##}.",
                MaxPrice: raisedBudget));
        }

        var optionalCuisine = specification.Cuisines.FirstOrDefault();
        if (optionalCuisine is not null)
        {
            suggestions.Add(new RelaxationSuggestion(
                RelaxationAction.RemoveCuisine,
                $"Try removing the {optionalCuisine} cuisine preference.",
                Cuisine: optionalCuisine));
        }

        var selectedFlavor = specification.Flavors.FirstOrDefault();
        if (selectedFlavor is not null)
        {
            suggestions.Add(new RelaxationSuggestion(
                RelaxationAction.TrySimilarFlavor,
                $"Try searching for other savory options alongside {selectedFlavor.ToLowerInvariant()}."));
        }

        return suggestions;
    }

    private void ValidateSpecification(SearchSpecification specification)
    {
        if (!double.IsFinite(specification.RadiusKm) || specification.RadiusKm <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(specification.RadiusKm), "Search radius must be positive.");
        }

        if (specification.MaxPrice is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(specification.MaxPrice), "Maximum price must be positive.");
        }

        if (specification.MinRating is < 0 or > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(specification.MinRating), "Minimum rating must be between 0 and 5.");
        }

        if (options.MaximumResults <= 0)
        {
            throw new InvalidOperationException("Search:MaximumResults must be greater than zero.");
        }
    }
}
