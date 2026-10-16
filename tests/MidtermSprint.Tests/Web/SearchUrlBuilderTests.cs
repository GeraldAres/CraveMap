using MidtermSprint.Application.Search;
using MidtermSprint.Web.Services;

namespace MidtermSprint.Tests.Web;

public sealed class SearchUrlBuilderTests
{
    [Fact]
    public void BuildExploreUri_EncodesQueryAndIncludesSelectedFiltersAndLocation()
    {
        var filters = new ManualSearchFilters
        {
            Flavors = new HashSet<string>(["Savory"], StringComparer.OrdinalIgnoreCase),
            OtherCharacteristics = new HashSet<string>(["Fresh"], StringComparer.OrdinalIgnoreCase),
            Cuisines = new HashSet<string>(["Japanese"], StringComparer.OrdinalIgnoreCase),
            Niches = new HashSet<string>(["Ramen"], StringComparer.OrdinalIgnoreCase),
            AllergensToAvoid = new HashSet<string>(["Tree nuts"], StringComparer.OrdinalIgnoreCase),
            RadiusKm = 5,
            MaxPrice = 300,
            PriceIsStrict = true,
            OpenNow = false,
            Sort = SearchSort.Nearest
        };
        var location = new SupportedLocation
        {
            Name = "Makati demo center",
            Latitude = 14.5547,
            Longitude = 121.0244
        };

        var uri = SearchUrlBuilder.BuildExploreUri("crispy chicken", filters, location);

        Assert.StartsWith("/explore?", uri);
        Assert.Contains("q=crispy%20chicken", uri);
        Assert.Contains("flavors=Savory", uri);
        Assert.Contains("other=Fresh", uri);
        Assert.Contains("cuisines=Japanese", uri);
        Assert.Contains("niches=Ramen", uri);
        Assert.Contains("allergens=Tree%20nuts", uri);
        Assert.Contains("radius=5", uri);
        Assert.Contains("maxPrice=300", uri);
        Assert.Contains("strict=true", uri);
        Assert.Contains("openNow=false", uri);
        Assert.Contains("sort=Nearest", uri);
        Assert.Contains("lat=14.5547", uri);
        Assert.Contains("lng=121.0244", uri);
    }

    [Fact]
    public void BuildExploreUri_WithNoQueryOrFilters_ReturnsExploreRoot()
    {
        var uri = SearchUrlBuilder.BuildExploreUri(string.Empty, new ManualSearchFilters(), null);

        Assert.Equal("/explore", uri);
    }

    [Fact]
    public void Parse_RestoresSearchQueryAndAllSupportedFilters()
    {
        var parsed = SearchUrlParser.Parse(
            "https://cravemap.test/explore?q=crispy%20chicken"
            + "&flavors=Savory%2CSpicy&textures=Crispy&other=Fresh"
            + "&cuisines=Japanese&niches=Ramen&diets=Vegan"
            + "&allergens=Tree%20nuts&radius=7.5&lat=14.5547&lng=121.0244"
            + "&maxPrice=300&strict=false&strictAllergen=true&openNow=false"
            + "&minRating=4.5&service=Takeout&sort=Nearest");

        Assert.Equal("crispy chicken", parsed.Query);
        Assert.Equal(2, parsed.Filters.Flavors.Count);
        Assert.Contains("Savory", parsed.Filters.Flavors);
        Assert.Contains("Spicy", parsed.Filters.Flavors);
        Assert.Contains("Fresh", parsed.Filters.OtherCharacteristics);
        Assert.Contains("Ramen", parsed.Filters.Niches);
        Assert.Contains("Tree nuts", parsed.Filters.AllergensToAvoid);
        Assert.Equal(7.5, parsed.Filters.RadiusKm);
        Assert.Equal(14.5547, parsed.Filters.OriginLatitude);
        Assert.Equal(121.0244, parsed.Filters.OriginLongitude);
        Assert.Equal(300, parsed.Filters.MaxPrice);
        Assert.False(parsed.Filters.PriceIsStrict);
        Assert.True(parsed.Filters.StrictAllergenMode);
        Assert.False(parsed.Filters.OpenNow);
        Assert.Equal(4.5m, parsed.Filters.MinRating);
        Assert.Equal(ServiceType.Takeout, parsed.Filters.ServiceType);
        Assert.Equal(SearchSort.Nearest, parsed.Filters.Sort);
    }

    [Fact]
    public void Parse_IgnoresInvalidNumericBooleanAndEnumValues()
    {
        var parsed = SearchUrlParser.Parse(
            "https://cravemap.test/explore?radius=Infinity&lat=91&lng=-181"
            + "&maxPrice=-10&strict=maybe&minRating=9&service=Unknown&sort=Nope");

        Assert.Null(parsed.Filters.RadiusKm);
        Assert.Null(parsed.Filters.OriginLatitude);
        Assert.Null(parsed.Filters.OriginLongitude);
        Assert.Null(parsed.Filters.MaxPrice);
        Assert.Null(parsed.Filters.PriceIsStrict);
        Assert.Null(parsed.Filters.MinRating);
        Assert.Null(parsed.Filters.ServiceType);
        Assert.Null(parsed.Filters.Sort);
    }
}
