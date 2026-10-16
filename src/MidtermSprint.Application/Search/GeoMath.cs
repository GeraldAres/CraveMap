namespace MidtermSprint.Application.Search;

public static class GeoMath
{
    private const double EarthRadiusKm = 6371.0088;
    private const double LatitudeKmPerDegree = 110.574;

    public static double DistanceKm(
        double latitude1,
        double longitude1,
        double latitude2,
        double longitude2)
    {
        ValidateCoordinates(latitude1, longitude1, nameof(latitude1));
        ValidateCoordinates(latitude2, longitude2, nameof(latitude2));

        var latitudeDelta = ToRadians(latitude2 - latitude1);
        var longitudeDelta = ToRadians(longitude2 - longitude1);
        var latitude1Radians = ToRadians(latitude1);
        var latitude2Radians = ToRadians(latitude2);

        var haversine = Math.Pow(Math.Sin(latitudeDelta / 2), 2) +
                        Math.Cos(latitude1Radians) * Math.Cos(latitude2Radians) *
                        Math.Pow(Math.Sin(longitudeDelta / 2), 2);
        return EarthRadiusKm * 2 * Math.Asin(Math.Sqrt(Math.Clamp(haversine, 0, 1)));
    }

    public static GeoBounds CreateBounds(double latitude, double longitude, double radiusKm)
    {
        ValidateCoordinates(latitude, longitude, nameof(latitude));
        if (!double.IsFinite(radiusKm) || radiusKm <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(radiusKm), "Radius must be a positive finite value.");
        }

        var latitudeDelta = radiusKm / LatitudeKmPerDegree;
        var minLatitude = Math.Max(-90, latitude - latitudeDelta);
        var maxLatitude = Math.Min(90, latitude + latitudeDelta);
        var cosine = Math.Cos(ToRadians(latitude));
        var includesAllLongitudes = Math.Abs(cosine) < 1e-12;
        var longitudeDelta = includesAllLongitudes ? 180 : radiusKm / (111.0 * Math.Abs(cosine));

        if (longitudeDelta >= 180)
        {
            includesAllLongitudes = true;
        }

        if (includesAllLongitudes)
        {
            return new GeoBounds(minLatitude, maxLatitude, -180, 180, false, true);
        }

        var rawWest = longitude - longitudeDelta;
        var rawEast = longitude + longitudeDelta;
        var crossesAntimeridian = rawWest <= -180 || rawEast >= 180;
        return new GeoBounds(
            minLatitude,
            maxLatitude,
            NormalizeLongitude(rawWest),
            NormalizeLongitude(rawEast),
            crossesAntimeridian,
            false);
    }

    private static void ValidateCoordinates(double latitude, double longitude, string parameterName)
    {
        if (!double.IsFinite(latitude) || latitude is < -90 or > 90)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Latitude must be between -90 and 90.");
        }

        if (!double.IsFinite(longitude) || longitude is < -180 or > 180)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Longitude must be between -180 and 180.");
        }
    }

    private static double NormalizeLongitude(double longitude)
    {
        var normalized = (longitude + 180) % 360;
        return (normalized < 0 ? normalized + 360 : normalized) - 180;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
