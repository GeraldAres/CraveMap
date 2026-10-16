using MidtermSprint.Domain.Entities;

namespace MidtermSprint.Application.Search;

/// <summary>
/// Scores an eligible menu item using configured, renormalized relevance weights.
/// </summary>
public sealed class SearchScorer
{
    private readonly SearchScoringWeights weights;

    public SearchScorer(SearchScoringWeights weights)
    {
        ArgumentNullException.ThrowIfNull(weights);
        var configuredWeights = new[]
        {
            weights.Flavor,
            weights.CuisineOrNiche,
            weights.Price,
            weights.Distance,
            weights.Rating
        };
        if (configuredWeights.Any(weight => !double.IsFinite(weight) || weight < 0) ||
            configuredWeights.All(weight => weight == 0))
        {
            throw new ArgumentException("Scoring weights must be finite, non-negative, and not all zero.", nameof(weights));
        }

        this.weights = weights;
    }

    public double Score(
        MenuItem item,
        Restaurant restaurant,
        SearchSpecification specification,
        double distanceKm)
    {
        var flavorAndTexture = specification.Flavors
            .Concat(specification.Textures)
            .Concat(specification.OtherCharacteristics)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var matchedCharacteristics = item.Characteristics
            .Where(link => link.Characteristic is not null)
            .Select(link => link.Characteristic!.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var cuisineAndNiche = specification.Cuisines
            .Concat(specification.Niches)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var itemCuisinesAndNiches = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (item.Cuisine is not null)
        {
            itemCuisinesAndNiches.Add(item.Cuisine.Name);
        }

        foreach (var niche in item.Niches.Where(link => link.FoodNiche is not null))
        {
            itemCuisinesAndNiches.Add(niche.FoodNiche!.Name);
        }

        double? flavorScore = flavorAndTexture.Count == 0
            ? null
            : (double)flavorAndTexture.Intersect(matchedCharacteristics, StringComparer.OrdinalIgnoreCase).Count()
              / flavorAndTexture.Count;
        double? cuisineScore = cuisineAndNiche.Count == 0
            ? null
            : (double)cuisineAndNiche.Intersect(itemCuisinesAndNiches, StringComparer.OrdinalIgnoreCase).Count()
              / cuisineAndNiche.Count;
        double? priceScore = specification.MaxPrice is not { } budget
            ? null
            : item.Price <= budget
                ? 1
                : specification.PriceIsStrict || budget <= 0
                    ? 0
                    : Math.Clamp((double)(budget / item.Price), 0, 1);
        var distanceScore = specification.RadiusKm <= 0
            ? 0
            : 1 - Math.Clamp(distanceKm / specification.RadiusKm, 0, 1);
        var ratingScore = (double)Math.Clamp(restaurant.Rating.GetValueOrDefault(2.5m) / 5m, 0, 1);

        var weighted = new List<(double? Value, double Weight)>
        {
            (flavorScore, weights.Flavor),
            (cuisineScore, weights.CuisineOrNiche),
            (priceScore, weights.Price),
            (distanceScore, weights.Distance),
            (ratingScore, weights.Rating)
        };
        var active = weighted.Where(component => component.Value.HasValue && component.Weight > 0).ToArray();
        var totalWeight = active.Sum(component => component.Weight);
        return totalWeight == 0
            ? 0
            : active.Sum(component => component.Value!.Value * component.Weight) / totalWeight;
    }
}
