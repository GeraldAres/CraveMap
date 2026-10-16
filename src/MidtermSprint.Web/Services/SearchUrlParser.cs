using System.Globalization;
using Microsoft.AspNetCore.WebUtilities;
using MidtermSprint.Application.Search;

namespace MidtermSprint.Web.Services;

public sealed record ParsedSearchUrl(string Query, ManualSearchFilters Filters);

public static class SearchUrlParser
{
    private const int MaximumQueryLength = 240;

    public static ParsedSearchUrl Parse(string uri)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uri);
        if (!Uri.TryCreate(uri, UriKind.Absolute, out var absoluteUri))
        {
            throw new ArgumentException("Search URI must be absolute.", nameof(uri));
        }

        var values = QueryHelpers.ParseQuery(absoluteUri.Query);
        var query = Read(values, "q");
        if (query.Length > MaximumQueryLength)
        {
            query = query[..MaximumQueryLength];
        }

        var latitude = ParseDouble(Read(values, "lat"), -90, 90);
        var longitude = ParseDouble(Read(values, "lng"), -180, 180);
        if (!latitude.HasValue || !longitude.HasValue)
        {
            latitude = null;
            longitude = null;
        }

        var niches = ReadSet(values, "niches");
        niches.UnionWith(ReadSet(values, "niche"));

        return new ParsedSearchUrl(query, new ManualSearchFilters
        {
            Flavors = ReadSet(values, "flavors"),
            Textures = ReadSet(values, "textures"),
            OtherCharacteristics = ReadSet(values, "other"),
            Cuisines = ReadSet(values, "cuisines"),
            Niches = niches,
            DietaryPreferences = ReadSet(values, "diets"),
            AllergensToAvoid = ReadSet(values, "allergens"),
            RadiusKm = ParseDouble(Read(values, "radius"), 0.1, 50),
            OriginLatitude = latitude,
            OriginLongitude = longitude,
            MaxPrice = ParseDecimal(Read(values, "maxPrice"), 0.01m, 100_000m),
            PriceIsStrict = ParseBoolean(Read(values, "strict")),
            StrictAllergenMode = ParseBoolean(Read(values, "strictAllergen")),
            OpenNow = ParseBoolean(Read(values, "openNow")),
            MinRating = ParseDecimal(Read(values, "minRating"), 0, 5),
            ServiceType = ParseEnum<ServiceType>(Read(values, "service")),
            Sort = ParseEnum<SearchSort>(Read(values, "sort"))
        });
    }

    private static string Read(
        IDictionary<string, Microsoft.Extensions.Primitives.StringValues> values,
        string key) =>
        values.TryGetValue(key, out var value) ? value.ToString().Trim() : string.Empty;

    private static HashSet<string> ReadSet(
        IDictionary<string, Microsoft.Extensions.Primitives.StringValues> values,
        string key)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in Read(values, key).Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            if (item.Length <= 60 && result.Count < 20)
            {
                result.Add(item);
            }
        }

        return result;
    }

    private static double? ParseDouble(string value, double minimum, double maximum) =>
        double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)
        && double.IsFinite(result)
        && result >= minimum
        && result <= maximum
            ? result
            : null;

    private static decimal? ParseDecimal(string value, decimal minimum, decimal maximum) =>
        decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result)
        && result >= minimum
        && result <= maximum
            ? result
            : null;

    private static bool? ParseBoolean(string value) =>
        bool.TryParse(value, out var result) ? result : null;

    private static TEnum? ParseEnum<TEnum>(string value)
        where TEnum : struct, Enum =>
        Enum.TryParse<TEnum>(value, ignoreCase: true, out var result)
        && Enum.IsDefined(result)
            ? result
            : null;
}
