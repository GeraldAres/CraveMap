using System.Globalization;
using MidtermSprint.Application.Search;

namespace MidtermSprint.Web.Services;

public static class SearchUrlBuilder
{
    public static string BuildExploreUri(
        string query,
        ManualSearchFilters filters,
        SupportedLocation? location,
        string? niche = null)
    {
        var values = new List<KeyValuePair<string, string>>();
        Add(values, "q", query);
        AddSet(values, "flavors", filters.Flavors);
        AddSet(values, "textures", filters.Textures);
        AddSet(values, "other", filters.OtherCharacteristics);
        AddSet(values, "cuisines", filters.Cuisines);
        AddSet(values, "niches", filters.Niches);
        AddSet(values, "diets", filters.DietaryPreferences);
        AddSet(values, "allergens", filters.AllergensToAvoid);
        Add(values, "radius", filters.RadiusKm?.ToString(CultureInfo.InvariantCulture));
        Add(values, "lat", (filters.OriginLatitude ?? location?.Latitude)?.ToString(CultureInfo.InvariantCulture));
        Add(values, "lng", (filters.OriginLongitude ?? location?.Longitude)?.ToString(CultureInfo.InvariantCulture));
        Add(values, "maxPrice", filters.MaxPrice?.ToString(CultureInfo.InvariantCulture));
        Add(values, "strict", filters.PriceIsStrict?.ToString().ToLowerInvariant());
        Add(values, "strictAllergen", filters.StrictAllergenMode?.ToString().ToLowerInvariant());
        Add(values, "openNow", filters.OpenNow == true ? "true" : null);
        Add(values, "minRating", filters.MinRating?.ToString(CultureInfo.InvariantCulture));
        Add(values, "service", filters.ServiceType?.ToString());
        Add(values, "sort", filters.Sort?.ToString());
        Add(values, "niche", niche);

        return "/explore" + (values.Count == 0
            ? string.Empty
            : "?" + string.Join("&", values.Select(pair =>
                $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}")));
    }

    private static void Add(
        ICollection<KeyValuePair<string, string>> values,
        string key,
        string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            values.Add(new KeyValuePair<string, string>(key, value));
        }
    }

    private static void AddSet(
        ICollection<KeyValuePair<string, string>> values,
        string key,
        IEnumerable<string> value) =>
        Add(values, key, string.Join(",", value.OrderBy(item => item, StringComparer.OrdinalIgnoreCase)));
}
