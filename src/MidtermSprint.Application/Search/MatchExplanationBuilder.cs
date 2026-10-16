using System.Globalization;
using MidtermSprint.Domain.Entities;

namespace MidtermSprint.Application.Search;

public sealed class MatchExplanationBuilder
{
    public (IReadOnlyList<MatchReason> Reasons, string Summary) Build(
        IReadOnlyList<MenuItem> matchedItems,
        MenuItem bestItem,
        Restaurant restaurant,
        SearchSpecification specification,
        double distanceKm)
    {
        var reasons = new List<MatchReason>();
        var itemNames = matchedItems.Select(item => item.Name).ToArray();
        var bestCharacteristics = bestItem.Characteristics
            .Where(link => link.Characteristic is not null)
            .Select(link => link.Characteristic!.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var requestedCharacteristics = specification.Flavors
            .Concat(specification.Textures)
            .Concat(specification.OtherCharacteristics)
            .Where(bestCharacteristics.Contains)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (requestedCharacteristics.Length > 0)
        {
            reasons.Add(new MatchReason(
                MatchReasonKind.Characteristic,
                $"Offers {string.Join(", ", requestedCharacteristics)} options, including {bestItem.Name}.",
                [bestItem.Name]));
        }

        var matchedCuisineAndNiche = new List<string>();
        if (bestItem.Cuisine is not null && specification.Cuisines.Contains(bestItem.Cuisine.Name))
        {
            matchedCuisineAndNiche.Add(bestItem.Cuisine.Name);
        }

        matchedCuisineAndNiche.AddRange(bestItem.Niches
            .Where(link => link.FoodNiche is not null && specification.Niches.Contains(link.FoodNiche.Name))
            .Select(link => link.FoodNiche!.Name));

        if (matchedCuisineAndNiche.Count > 0)
        {
            reasons.Add(new MatchReason(
                MatchReasonKind.CuisineOrNiche,
                $"Matches your {string.Join(" and ", matchedCuisineAndNiche.Distinct(StringComparer.OrdinalIgnoreCase))} preference.",
                [bestItem.Name]));
        }

        if (specification.MaxPrice is { } budget && bestItem.Price <= budget)
        {
            var countUnderBudget = matchedItems.Count(item => item.Price <= budget);
            reasons.Add(new MatchReason(
                MatchReasonKind.Price,
                $"Has {countUnderBudget} matched menu item{(countUnderBudget == 1 ? string.Empty : "s")} at or under ₱{budget.ToString("0.##", CultureInfo.InvariantCulture)}.",
                matchedItems.Where(item => item.Price <= budget).Select(item => item.Name).ToArray()));
        }

        if (distanceKm <= specification.RadiusKm)
        {
            reasons.Add(new MatchReason(
                MatchReasonKind.Distance,
                $"Located {distanceKm.ToString("0.0", CultureInfo.InvariantCulture)} km away within your {specification.RadiusKm.ToString("0.##", CultureInfo.InvariantCulture)} km search radius.",
                itemNames));
        }

        if (restaurant.Rating.HasValue && specification.MinRating.HasValue &&
            restaurant.Rating.Value >= specification.MinRating.Value)
        {
            reasons.Add(new MatchReason(
                MatchReasonKind.Rating,
                $"Rated {restaurant.Rating.Value.ToString("0.0", CultureInfo.InvariantCulture)} out of 5.",
                itemNames));
        }

        var summary = reasons.Count == 0
            ? $"A nearby option with {bestItem.Name} on its sample menu."
            : string.Join(" ", reasons.Select(reason => reason.Text));
        return (reasons, summary);
    }
}
