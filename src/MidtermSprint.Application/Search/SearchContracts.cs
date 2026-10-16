using MidtermSprint.Domain.Entities;
using MidtermSprint.Domain.Enums;

namespace MidtermSprint.Application.Search;

public enum SearchSort
{
    BestMatch,
    Nearest,
    LowestPrice,
    HighestRating
}

public enum ServiceType
{
    Any,
    DineIn,
    Takeout
}

public enum SearchChipKind
{
    Flavor,
    Texture,
    OtherCharacteristic,
    Cuisine,
    Niche,
    DietaryPreference,
    Allergen,
    Budget,
    Distance
}

public enum MatchReasonKind
{
    Characteristic,
    CuisineOrNiche,
    Price,
    Distance,
    Rating
}

public enum RelaxationAction
{
    ExpandRadius,
    RaiseBudget,
    RemoveCuisine,
    TrySimilarFlavor
}

public sealed record DetectedChip(
    SearchChipKind Kind,
    string Label,
    string SourceText,
    bool IsStrict);

public sealed record SearchSpecification
{
    public IReadOnlySet<string> Flavors { get; init; } = EmptySet();
    public IReadOnlySet<string> Textures { get; init; } = EmptySet();
    public IReadOnlySet<string> OtherCharacteristics { get; init; } = EmptySet();
    public IReadOnlySet<string> Cuisines { get; init; } = EmptySet();
    public IReadOnlySet<string> Niches { get; init; } = EmptySet();
    public IReadOnlySet<string> DietaryPreferences { get; init; } = EmptySet();
    public IReadOnlySet<string> AllergensToAvoid { get; init; } = EmptySet();
    public decimal? MaxPrice { get; init; }
    public bool PriceIsStrict { get; init; }
    public double RadiusKm { get; init; } = 5;
    public double? OriginLatitude { get; init; }
    public double? OriginLongitude { get; init; }
    public decimal? MinRating { get; init; }
    public bool OpenNow { get; init; }
    public ServiceType ServiceType { get; init; } = ServiceType.Any;
    public bool StrictAllergenMode { get; init; }
    public SearchSort Sort { get; init; } = SearchSort.BestMatch;

    internal static IReadOnlySet<string> EmptySet() =>
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);
}

public sealed record ManualSearchFilters
{
    public IReadOnlySet<string> Flavors { get; init; } = SearchSpecification.EmptySet();
    public IReadOnlySet<string> Textures { get; init; } = SearchSpecification.EmptySet();
    public IReadOnlySet<string> OtherCharacteristics { get; init; } = SearchSpecification.EmptySet();
    public IReadOnlySet<string> Cuisines { get; init; } = SearchSpecification.EmptySet();
    public IReadOnlySet<string> Niches { get; init; } = SearchSpecification.EmptySet();
    public IReadOnlySet<string> DietaryPreferences { get; init; } = SearchSpecification.EmptySet();
    public IReadOnlySet<string> AllergensToAvoid { get; init; } = SearchSpecification.EmptySet();
    public decimal? MaxPrice { get; init; }
    public bool? PriceIsStrict { get; init; }
    public double? RadiusKm { get; init; }
    public double? OriginLatitude { get; init; }
    public double? OriginLongitude { get; init; }
    public decimal? MinRating { get; init; }
    public bool? OpenNow { get; init; }
    public ServiceType? ServiceType { get; init; }
    public bool? StrictAllergenMode { get; init; }
    public SearchSort? Sort { get; init; }
}

public sealed record ParsedCraving(
    SearchSpecification Specification,
    IReadOnlyList<DetectedChip> DetectedChips,
    string UnrecognizedText);

public sealed record SearchRequest(string Query, ManualSearchFilters? Filters = null);

public sealed record MatchReason(
    MatchReasonKind Kind,
    string Text,
    IReadOnlyList<string> MatchedMenuItemNames);

public sealed record RestaurantMatch(
    Restaurant Restaurant,
    IReadOnlyList<MenuItem> MatchedMenuItems,
    MenuItem BestMatchingMenuItem,
    double DistanceKm,
    double Score,
    IReadOnlyList<MatchReason> Reasons,
    string Explanation);

public sealed record RelaxationSuggestion(
    RelaxationAction Action,
    string Label,
    double? RadiusKm = null,
    decimal? MaxPrice = null,
    string? Cuisine = null);

public sealed record SearchResult(
    IReadOnlyList<RestaurantMatch> Verified,
    IReadOnlyList<RestaurantMatch> Unverified,
    int TotalCount,
    SearchSpecification AppliedSpec,
    IReadOnlyList<RelaxationSuggestion> Suggestions);

public sealed class SearchOptions
{
    public double DefaultRadiusKm { get; set; } = 5;
    public double DefaultLatitude { get; set; } = 14.5547;
    public double DefaultLongitude { get; set; } = 121.0244;
    public int MaximumResults { get; set; } = 100;
    public SearchScoringWeights ScoringWeights { get; set; } = new();
}

public sealed class SearchScoringWeights
{
    public double Flavor { get; set; } = 0.35;
    public double CuisineOrNiche { get; set; } = 0.25;
    public double Price { get; set; } = 0.20;
    public double Distance { get; set; } = 0.10;
    public double Rating { get; set; } = 0.10;
}

public interface IQueryInterpreter
{
    ParsedCraving Interpret(string query);
}

public interface ISearchService
{
    Task<SearchResult> SearchAsync(
        SearchRequest request,
        CancellationToken cancellationToken = default);
}

public static class SearchSpecificationMerger
{
    public static SearchSpecification Merge(
        SearchSpecification parsed,
        ManualSearchFilters manual,
        SearchOptions options)
    {
        ArgumentNullException.ThrowIfNull(parsed);
        ArgumentNullException.ThrowIfNull(manual);
        ArgumentNullException.ThrowIfNull(options);

        var allergens = Union(parsed.AllergensToAvoid, manual.AllergensToAvoid);
        return new SearchSpecification
        {
            Flavors = Union(parsed.Flavors, manual.Flavors),
            Textures = Union(parsed.Textures, manual.Textures),
            OtherCharacteristics = Union(parsed.OtherCharacteristics, manual.OtherCharacteristics),
            Cuisines = Union(parsed.Cuisines, manual.Cuisines),
            Niches = Union(parsed.Niches, manual.Niches),
            DietaryPreferences = Union(parsed.DietaryPreferences, manual.DietaryPreferences),
            AllergensToAvoid = allergens,
            MaxPrice = manual.MaxPrice ?? parsed.MaxPrice,
            PriceIsStrict = manual.MaxPrice.HasValue
                ? manual.PriceIsStrict ?? true
                : parsed.PriceIsStrict,
            RadiusKm = manual.RadiusKm ?? parsed.RadiusKm,
            OriginLatitude = manual.OriginLatitude ?? parsed.OriginLatitude ?? options.DefaultLatitude,
            OriginLongitude = manual.OriginLongitude ?? parsed.OriginLongitude ?? options.DefaultLongitude,
            MinRating = manual.MinRating ?? parsed.MinRating,
            OpenNow = manual.OpenNow ?? parsed.OpenNow,
            ServiceType = manual.ServiceType ?? parsed.ServiceType,
            StrictAllergenMode = manual.StrictAllergenMode ?? (allergens.Count > 0),
            Sort = manual.Sort ?? parsed.Sort
        };
    }

    private static IReadOnlySet<string> Union(
        IEnumerable<string> first,
        IEnumerable<string> second) =>
        new HashSet<string>(first.Concat(second), StringComparer.OrdinalIgnoreCase);
}
