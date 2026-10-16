using Microsoft.EntityFrameworkCore;
using MidtermSprint.Application.Abstractions;
using MidtermSprint.Application.Search;
using MidtermSprint.Domain.Entities;

namespace MidtermSprint.Infrastructure.Data;

public sealed class RestaurantRepository(IDbContextFactory<CraveMapDbContext> contextFactory)
    : IRestaurantRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<Restaurant>> GetPublishedAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await PublishedQuery(context).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Restaurant?> GetPublishedBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await PublishedQuery(context)
            .SingleOrDefaultAsync(restaurant => restaurant.Slug == slug, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Restaurant>> GetPublishedByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return Array.Empty<Restaurant>();
        }

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await PublishedQuery(context)
            .Where(restaurant => ids.Contains(restaurant.Id))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Restaurant>> GetPublishedWithinBoundsAsync(
        GeoBounds bounds,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var query = PublishedQuery(context)
            .Where(restaurant =>
                restaurant.Latitude >= bounds.MinLatitude &&
                restaurant.Latitude <= bounds.MaxLatitude);

        if (!bounds.IncludesAllLongitudes)
        {
            query = bounds.CrossesAntimeridian
                ? query.Where(restaurant =>
                    restaurant.Longitude >= bounds.WestLongitude ||
                    restaurant.Longitude <= bounds.EastLongitude)
                : query.Where(restaurant =>
                    restaurant.Longitude >= bounds.WestLongitude &&
                    restaurant.Longitude <= bounds.EastLongitude);
        }

        return await query.ToListAsync(cancellationToken);
    }

    private static IQueryable<Restaurant> PublishedQuery(CraveMapDbContext context) =>
        context.Restaurants
            .AsNoTracking()
            .AsSplitQuery()
            .Where(restaurant => restaurant.IsPublished)
            .Include(restaurant => restaurant.Hours)
            .Include(restaurant => restaurant.MenuItems)
                .ThenInclude(item => item.Cuisine)
            .Include(restaurant => restaurant.MenuItems)
                .ThenInclude(item => item.Niches)
                    .ThenInclude(link => link.FoodNiche)
            .Include(restaurant => restaurant.MenuItems)
                .ThenInclude(item => item.Characteristics)
                    .ThenInclude(link => link.Characteristic)
            .Include(restaurant => restaurant.MenuItems)
                .ThenInclude(item => item.DietaryAttributes)
                    .ThenInclude(link => link.DietaryPreference)
            .Include(restaurant => restaurant.MenuItems)
                .ThenInclude(item => item.Allergens)
                    .ThenInclude(link => link.Allergen);
}
