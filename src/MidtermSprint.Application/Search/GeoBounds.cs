namespace MidtermSprint.Application.Search;

public readonly record struct GeoBounds(
    double MinLatitude,
    double MaxLatitude,
    double WestLongitude,
    double EastLongitude,
    bool CrossesAntimeridian,
    bool IncludesAllLongitudes)
{
    public bool Contains(double latitude, double longitude) =>
        latitude >= MinLatitude &&
        latitude <= MaxLatitude &&
        (IncludesAllLongitudes ||
         (CrossesAntimeridian
             ? longitude >= WestLongitude || longitude <= EastLongitude
             : longitude >= WestLongitude && longitude <= EastLongitude));
}
