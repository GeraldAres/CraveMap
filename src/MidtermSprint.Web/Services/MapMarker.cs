namespace MidtermSprint.Web.Services;

public sealed record MapMarker(
    int RestaurantId,
    string RestaurantName,
    string MenuItemName,
    decimal Price,
    double Latitude,
    double Longitude);

public sealed record MapLocation(double Latitude, double Longitude);

public sealed record GeolocationResult(
    bool IsSuccess,
    double? Latitude,
    double? Longitude,
    string? Error);
