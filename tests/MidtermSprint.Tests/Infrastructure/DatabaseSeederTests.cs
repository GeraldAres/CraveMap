using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using MidtermSprint.Domain.Entities;
using MidtermSprint.Domain.Enums;
using MidtermSprint.Infrastructure.Data;
using MidtermSprint.Infrastructure.Seeding;

namespace MidtermSprint.Tests.Infrastructure;

public sealed class DatabaseSeederTests
{
    [Fact]
    public async Task InitializeAsync_MigratesSeedsAndIsIdempotent()
    {
        var settings = new Dictionary<string, string?>
        {
            ["Seed:CenterLatitude"] = "14.5547",
            ["Seed:CenterLongitude"] = "121.0244",
            ["Seed:AreaName"] = "Test Area"
        };

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var options = new DbContextOptionsBuilder<CraveMapDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var contextFactory = new PooledDbContextFactory<CraveMapDbContext>(options);

        var seeder = new DatabaseSeeder(contextFactory, configuration);
        await seeder.InitializeAsync();
        await seeder.InitializeAsync();

        var restaurants = await new RestaurantRepository(contextFactory).GetPublishedAsync();
        var savedSlugs = restaurants.Take(2).Select(restaurant => restaurant.Slug).ToArray();
        var savedRestaurants = await new RestaurantRepository(contextFactory)
            .GetPublishedBySlugsAsync(savedSlugs);
        var items = restaurants.SelectMany(restaurant => restaurant.MenuItems).ToArray();
        var peanutEvidence = items.SelectMany(item => item.Allergens)
            .Where(evidence => evidence.Allergen?.Name == "Peanuts")
            .Select(evidence => evidence.EvidenceStatus)
            .ToHashSet();

        Assert.Equal(33, restaurants.Count);
        Assert.Equal(128, items.Length);
        Assert.Equal(savedSlugs.Order(), savedRestaurants.Select(restaurant => restaurant.Slug).Order());
        Assert.Empty(await new RestaurantRepository(contextFactory).GetPublishedBySlugsAsync([]));
        Assert.Contains(restaurants, restaurant => restaurant.MenuItems.Count == 0);
        Assert.Contains(restaurants, restaurant => restaurant.Rating is null);
        Assert.Contains(peanutEvidence, status => status == AllergenEvidenceStatus.Contains);
        Assert.Contains(peanutEvidence, status => status == AllergenEvidenceStatus.MayContainCrossContact);
        Assert.Contains(peanutEvidence, status => status == AllergenEvidenceStatus.VerifiedAbsent);
        Assert.Contains(peanutEvidence, status => status == AllergenEvidenceStatus.Unknown);
        Assert.All(restaurants, restaurant => Assert.Equal(7, restaurant.Hours.Count));
    }

    [Fact]
    public async Task InitializeAsync_DoesNotSeedWhenAnyTableAlreadyHasRows()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Seed:CenterLatitude"] = "14.5547",
                ["Seed:CenterLongitude"] = "121.0244"
            })
            .Build();
        var options = new DbContextOptionsBuilder<CraveMapDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var contextFactory = new PooledDbContextFactory<CraveMapDbContext>(options);
        await using (var context = await contextFactory.CreateDbContextAsync())
        {
            context.Allergens.Add(new Allergen { Name = "Existing", Slug = "existing" });
            await context.SaveChangesAsync();
        }

        await new DatabaseSeeder(contextFactory, configuration).InitializeAsync();

        await using var verificationContext = await contextFactory.CreateDbContextAsync();
        Assert.Empty(await verificationContext.Restaurants.ToListAsync());
        Assert.Single(await verificationContext.Allergens.ToListAsync());
    }
}
