using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using MidtermSprint.Domain.Enums;
using MidtermSprint.Infrastructure.Data;
using MidtermSprint.Infrastructure.Seeding;

namespace MidtermSprint.Tests.Infrastructure;

public sealed class DatabaseSeederTests
{
    [Fact]
    public async Task InitializeAsync_MigratesSeedsAndIsIdempotent()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"cravemap-{Guid.NewGuid():N}.db");
        var settings = new Dictionary<string, string?>
        {
            ["ConnectionStrings:CraveMap"] = $"Data Source={databasePath}",
            ["Seed:CenterLatitude"] = "14.5547",
            ["Seed:CenterLongitude"] = "121.0244",
            ["Seed:AreaName"] = "Test Area"
        };

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var options = new DbContextOptionsBuilder<CraveMapDbContext>()
            .UseSqlite(configuration["ConnectionStrings:CraveMap"])
            .Options;
        var contextFactory = new PooledDbContextFactory<CraveMapDbContext>(options);

        try
        {
            var seeder = new DatabaseSeeder(contextFactory, configuration);
            await seeder.InitializeAsync();
            await seeder.InitializeAsync();

            var restaurants = await new RestaurantRepository(contextFactory).GetPublishedAsync();
            var items = restaurants.SelectMany(restaurant => restaurant.MenuItems).ToArray();
            var peanutEvidence = items.SelectMany(item => item.Allergens)
                .Where(evidence => evidence.Allergen?.Name == "Peanuts")
                .Select(evidence => evidence.EvidenceStatus)
                .ToHashSet();

            Assert.Equal(33, restaurants.Count);
            Assert.Equal(128, items.Length);
            Assert.Contains(restaurants, restaurant => restaurant.MenuItems.Count == 0);
            Assert.Contains(restaurants, restaurant => restaurant.Rating is null);
            Assert.Contains(peanutEvidence, status => status == AllergenEvidenceStatus.Contains);
            Assert.Contains(peanutEvidence, status => status == AllergenEvidenceStatus.MayContainCrossContact);
            Assert.Contains(peanutEvidence, status => status == AllergenEvidenceStatus.VerifiedAbsent);
            Assert.Contains(peanutEvidence, status => status == AllergenEvidenceStatus.Unknown);
            Assert.All(restaurants, restaurant => Assert.Equal(7, restaurant.Hours.Count));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
    }
}
