using MidtermSprint.Domain.Entities;
using MidtermSprint.Application.Search;

namespace MidtermSprint.Application.Abstractions;

/// <summary>
/// Provides published restaurant and menu data without coupling callers to the persistence provider.
/// </summary>
public interface IRestaurantRepository
{
    Task<IReadOnlyList<Restaurant>> GetPublishedAsync(CancellationToken cancellationToken = default);

    Task<Restaurant?> GetPublishedBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Restaurant>> GetPublishedBySlugsAsync(
        IReadOnlyCollection<string> slugs,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Restaurant>> GetPublishedByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Restaurant>> GetPublishedWithinBoundsAsync(
        GeoBounds bounds,
        CancellationToken cancellationToken = default);
}
