namespace MidtermSprint.Web.Services;

public sealed class DiscoveryOptions
{
    public List<CravingSuggestion> SuggestedCravings { get; set; } = [];
    public List<FoodNicheCard> FoodNiches { get; set; } = [];
    public List<SupportedLocation> SupportedLocations { get; set; } = [];
}

public sealed class CravingSuggestion
{
    public string Label { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
}

public sealed class FoodNicheCard
{
    public string Name { get; set; } = string.Empty;
    public string Emoji { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;
}

public sealed class SupportedLocation
{
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public sealed record NearbyDiscovery(
    string Name,
    string Slug,
    string Cuisine,
    string? MenuItemName,
    decimal? Price,
    int PriceRange,
    double DistanceKm);
