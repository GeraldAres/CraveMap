using MidtermSprint.Application.Search;

namespace MidtermSprint.Tests.Search;

public sealed class GeoMathTests
{
    [Fact]
    public void DistanceKm_UsesHaversineDistance()
    {
        var distance = GeoMath.DistanceKm(0, 0, 0, 1);

        Assert.InRange(distance, 111.19, 111.20);
    }

    [Fact]
    public void CreateBounds_ContainsCenterAndNearbyCoordinates()
    {
        var bounds = GeoMath.CreateBounds(14.5547, 121.0244, 2);

        Assert.True(bounds.Contains(14.5547, 121.0244));
        Assert.True(bounds.Contains(14.56, 121.03));
        Assert.False(bounds.Contains(14.58, 121.0244));
    }

    [Fact]
    public void CreateBounds_HandlesTheAntimeridian()
    {
        var bounds = GeoMath.CreateBounds(0, 179.9, 50);

        Assert.True(bounds.CrossesAntimeridian);
        Assert.True(bounds.Contains(0, -179.9));
        Assert.False(bounds.Contains(0, 0));
    }

    [Theory]
    [InlineData(91, 0, 1)]
    [InlineData(0, 181, 1)]
    [InlineData(0, 0, 0)]
    public void CreateBounds_RejectsInvalidInputs(double latitude, double longitude, double radiusKm)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            GeoMath.CreateBounds(latitude, longitude, radiusKm));
    }
}
