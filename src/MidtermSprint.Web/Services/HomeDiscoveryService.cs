using MidtermSprint.Application.Abstractions;
using MidtermSprint.Application.Search;

namespace MidtermSprint.Web.Services;

/// <summary>
/// Provides a small, distance-ranked sample of menu-backed discoveries for the home page.
/// </summary>
public sealed class HomeDiscoveryService(
    IRestaurantRepository restaurantRepository,
    SearchOptions searchOptions)
{
    public async Task<IReadOnlyList<NearbyDiscovery>> GetNearbyAsync(
        SupportedLocation location,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(location);
        var bounds = GeoMath.CreateBounds(location.Latitude, location.Longitude, searchOptions.DefaultRadiusKm);
        var restaurants = await restaurantRepository.GetPublishedWithinBoundsAsync(bounds, cancellationToken);

        return restaurants
            .Select(restaurant =>
            {
                var item = restaurant.MenuItems
                    .Where(menuItem => menuItem.IsAvailable)
                    .OrderBy(menuItem => menuItem.Price)
                    .FirstOrDefault();
                return new NearbyDiscovery(
                    restaurant.Name,
                    restaurant.Slug,
                    item?.Cuisine?.Name ?? "Menu not yet available",
                    item?.Name,
                    item?.Price,
                    restaurant.PriceRange,
                    GeoMath.DistanceKm(
                        location.Latitude,
                        location.Longitude,
                        restaurant.Latitude,
                        restaurant.Longitude));
            })
            .OrderBy(discovery => discovery.DistanceKm)
            .Take(3)
            .ToArray();
    }
}
