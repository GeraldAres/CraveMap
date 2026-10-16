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
            AllergensToAvoid = new HashSet<string>(["Tree nuts"], StringComparer.OrdinalIgnoreCase),
            RadiusKm = 5,
            MaxPrice = 300,
            PriceIsStrict = true,
            OpenNow = true
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
        Assert.Contains("allergens=Tree%20nuts", uri);
        Assert.Contains("radius=5", uri);
        Assert.Contains("maxPrice=300", uri);
        Assert.Contains("strict=true", uri);
        Assert.Contains("openNow=true", uri);
        Assert.Contains("lat=14.5547", uri);
        Assert.Contains("lng=121.0244", uri);
    }

    [Fact]
    public void BuildExploreUri_WithNoQueryOrFilters_ReturnsExploreRoot()
    {
        var uri = SearchUrlBuilder.BuildExploreUri(string.Empty, new ManualSearchFilters(), null);

        Assert.Equal("/explore", uri);
    }
}
