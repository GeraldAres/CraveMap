using MidtermSprint.Application.Abstractions;
using MidtermSprint.Application.Search;
using MidtermSprint.Domain.Entities;
using MidtermSprint.Domain.Enums;

namespace MidtermSprint.Tests.Search;

public sealed class SearchServiceTests
{
    [Fact]
    public async Task SearchAsync_CombinesQueryAndManualFiltersAndHonorsStrictPrice()
    {
        var restaurant = CreateRestaurant(0, 0);
        var matchingItem = restaurant.MenuItems.Single();
        matchingItem.Name = "Spicy Crispy Tofu Bowl";
        matchingItem.Price = 190;
        matchingItem.Cuisine = Cuisine("Korean");
        matchingItem.Niches.Add(new MenuItemNiche
        {
            FoodNiche = new FoodNiche { Name = "Korean Street Food", Slug = "korean-street-food" }
        });
        matchingItem.DietaryAttributes.Add(new MenuItemDietaryAttribute
        {
            DietaryPreference = new DietaryPreference { Name = "Vegan", Slug = "vegan" },
            IsVerified = true
        });
        matchingItem.Characteristics.Add(new MenuItemCharacteristic
        {
            Characteristic = new Characteristic { Name = "Spicy", Kind = CharacteristicKind.Flavor }
        });
        matchingItem.Characteristics.Add(new MenuItemCharacteristic
        {
            Characteristic = new Characteristic { Name = "Crispy", Kind = CharacteristicKind.Texture }
        });
        matchingItem.Allergens.Add(VerifiedAbsent("Peanuts"));

        var overBudgetItem = CreateItem("Spicy Crispy Tofu Bowl Large", 260);
        overBudgetItem.Cuisine = Cuisine("Korean");
        overBudgetItem.DietaryAttributes.Add(new MenuItemDietaryAttribute
        {
            DietaryPreference = new DietaryPreference { Name = "Vegan", Slug = "vegan" },
            IsVerified = true
        });
        overBudgetItem.Allergens.Add(VerifiedAbsent("Peanuts"));
        restaurant.MenuItems.Add(overBudgetItem);

        var options = Options();
        var service = CreateService([restaurant], options);
        var result = await service.SearchAsync(new SearchRequest(
            "spicy Korean food under ₱250 within 3 km without peanuts",
            new ManualSearchFilters
            {
                DietaryPreferences = Set("Vegan"),
                MaxPrice = 240,
                PriceIsStrict = true,
                RadiusKm = 3,
                AllergensToAvoid = Set("Peanuts"),
                StrictAllergenMode = true
            }));

        var match = Assert.Single(result.Verified);
        Assert.Equal("Spicy Crispy Tofu Bowl", match.BestMatchingMenuItem.Name);
        Assert.Single(match.MatchedMenuItems);
        Assert.Equal(240, result.AppliedSpec.MaxPrice);
        Assert.Contains("Korean", result.AppliedSpec.Cuisines);
        Assert.Contains("Spicy", result.AppliedSpec.Flavors);
        Assert.Contains("Vegan", result.AppliedSpec.DietaryPreferences);
        Assert.Contains("Peanuts", result.AppliedSpec.AllergensToAvoid);
    }

    [Fact]
    public async Task SearchAsync_SoftBudgetRanksUnderBudgetItemsFirstWithoutExcludingOverBudgetItems()
    {
        var restaurant = CreateRestaurant(0, 0);
        restaurant.MenuItems.Single().Name = "Budget meal";
        restaurant.MenuItems.Single().Price = 190;
        restaurant.MenuItems.Add(CreateItem("Higher priced meal", 250));
        var result = await CreateService([restaurant], Options()).SearchAsync(
            new SearchRequest("preferably under 200"));

        var match = Assert.Single(result.Verified);
        Assert.Equal(2, match.MatchedMenuItems.Count);
        Assert.Equal(190, match.BestMatchingMenuItem.Price);
        Assert.False(result.AppliedSpec.PriceIsStrict);
    }

    [Fact]
    public async Task SearchAsync_StrictBudgetIncludesThePriceBoundaryButExcludesMoreExpensiveItems()
    {
        var restaurant = CreateRestaurant(0, 0);
        restaurant.MenuItems.Single().Price = 180;
        restaurant.MenuItems.Add(CreateItem("At budget", 200));
        restaurant.MenuItems.Add(CreateItem("Over budget", 201));

        var result = await CreateService([restaurant], Options()).SearchAsync(
            new SearchRequest("under ₱200"));

        Assert.Equal(2, Assert.Single(result.Verified).MatchedMenuItems.Count);
    }

    [Fact]
    public async Task SearchAsync_ReturnsVerifiedAndUnverifiedAllergenTiersSeparately()
    {
        var restaurants = new[]
        {
            RestaurantWithEvidence(0, AllergenEvidenceStatus.Contains),
            RestaurantWithEvidence(1, AllergenEvidenceStatus.MayContainCrossContact),
            RestaurantWithEvidence(2, AllergenEvidenceStatus.VerifiedAbsent),
            RestaurantWithEvidence(3, AllergenEvidenceStatus.Unknown),
            CreateRestaurantWithNoAllergenRow(4)
        };
        var service = CreateService(restaurants, Options());

        var strictResult = await service.SearchAsync(new SearchRequest("without peanuts"));
        Assert.Single(strictResult.Verified);
        Assert.Empty(strictResult.Unverified);
        Assert.Equal("Evidence 2", strictResult.Verified[0].Restaurant.Name);

        var relaxedResult = await service.SearchAsync(new SearchRequest(
            "without peanuts",
            new ManualSearchFilters { StrictAllergenMode = false }));
        Assert.Single(relaxedResult.Verified);
        Assert.Equal(3, relaxedResult.Unverified.Count);
        Assert.DoesNotContain(
            relaxedResult.Verified.Concat(relaxedResult.Unverified),
            match => match.Restaurant.Name == "Evidence 0");
    }

    [Fact]
    public async Task SearchAsync_PrioritizesMatchingCuisine()
    {
        var japanese = CreateRestaurant(0, 0, name: "Japanese sample");
        japanese.MenuItems.Single().Cuisine = Cuisine("Japanese");
        var filipino = CreateRestaurant(0, 0, name: "Filipino sample");
        filipino.MenuItems.Single().Cuisine = Cuisine("Filipino");

        var result = await CreateService([filipino, japanese], Options()).SearchAsync(
            new SearchRequest("Japanese food"));

        Assert.Equal("Japanese sample", result.Verified[0].Restaurant.Name);
    }

    [Fact]
    public async Task SearchAsync_UsesBoundingBoxAndExactRadius()
    {
        var nearby = CreateRestaurant(0, 0, name: "Nearby");
        var boundaryLatitude = 180 / Math.PI / 6371.0088;
        var boundary = CreateRestaurant(boundaryLatitude, 0, name: "Boundary");
        var distant = CreateRestaurant(0.02, 0, name: "Distant");
        var result = await CreateService([nearby, boundary, distant], Options()).SearchAsync(
            new SearchRequest(string.Empty, new ManualSearchFilters { RadiusKm = 1 }));

        Assert.Contains(result.Verified, match => match.Restaurant.Name == "Nearby");
        Assert.Contains(result.Verified, match => match.Restaurant.Name == "Boundary");
        Assert.DoesNotContain(result.Verified, match => match.Restaurant.Name == "Distant");
    }

    [Fact]
    public async Task SearchAsync_OffersSafeRelaxationsWithoutRemovingAllergens()
    {
        var restaurant = CreateRestaurant(0, 0);
        restaurant.MenuItems.Single().Price = 100;
        var result = await CreateService([restaurant], Options()).SearchAsync(
            new SearchRequest("vegan seafood under ₱50 within 500 m"));

        Assert.Empty(result.Verified);
        Assert.Contains(result.Suggestions, suggestion => suggestion.Action == RelaxationAction.ExpandRadius);
        Assert.Contains(result.Suggestions, suggestion => suggestion.Action == RelaxationAction.RaiseBudget);
        Assert.DoesNotContain(result.Suggestions, suggestion => suggestion.Action.ToString().Contains("Allergen"));
    }

    [Fact]
    public async Task SearchAsync_UsesInjectedTimeProviderForOvernightOpenNow()
    {
        var restaurant = CreateRestaurant(0, 0);
        restaurant.Hours.Add(new RestaurantHour
        {
            DayOfWeek = DayOfWeek.Friday,
            OpensAt = new TimeOnly(22, 0),
            ClosesAt = new TimeOnly(2, 0)
        });
        var earlyMorning = CreateService(
            [restaurant],
            Options(),
            new FixedTimeProvider(new DateTimeOffset(2026, 10, 16, 17, 0, 0, TimeSpan.Zero)));
        var afterClose = CreateService(
            [restaurant],
            Options(),
            new FixedTimeProvider(new DateTimeOffset(2026, 10, 16, 20, 0, 0, TimeSpan.Zero)));

        Assert.Single((await earlyMorning.SearchAsync(new SearchRequest(
            string.Empty,
            new ManualSearchFilters { OpenNow = true }))).Verified);
        Assert.Empty((await afterClose.SearchAsync(new SearchRequest(
            string.Empty,
            new ManualSearchFilters { OpenNow = true }))).Verified);
    }

    [Fact]
    public async Task SearchAsync_ExplanationDoesNotClaimUnmatchedCharacteristics()
    {
        var restaurant = CreateRestaurant(0, 0);
        var result = await CreateService([restaurant], Options()).SearchAsync(
            new SearchRequest("crispy"));

        var explanation = Assert.Single(result.Verified).Explanation;
        Assert.DoesNotContain("crispy", explanation, StringComparison.OrdinalIgnoreCase);
    }

    internal static Restaurant CreateRestaurant(
        double latitude,
        double longitude,
        string name = "Sample",
        decimal? rating = 4)
    {
        var item = CreateItem("Sample meal", 150);
        item.Cuisine = Cuisine("Filipino");
        return new Restaurant
        {
            Id = (int)(Math.Abs(latitude * 100000) + Math.Abs(longitude * 100000)) + 1,
            Name = name,
            Slug = name.ToLowerInvariant().Replace(' ', '-'),
            Description = "Fictional test restaurant.",
            Address = "Fictional test location.",
            Latitude = latitude,
            Longitude = longitude,
            Rating = rating,
            PriceRange = 1,
            SupportsDineIn = true,
            SupportsTakeout = true,
            IsPublished = true,
            MenuItems = [item]
        };
    }

    private static SearchService CreateService(
        IReadOnlyList<Restaurant> restaurants,
        SearchOptions options,
        TimeProvider? timeProvider = null) =>
        new(
            new RuleBasedQueryInterpreter(options),
            new InMemoryRestaurantRepository(restaurants),
            options,
            timeProvider ?? new FixedTimeProvider(new DateTimeOffset(2026, 10, 16, 12, 0, 0, TimeSpan.Zero)));

    private static SearchOptions Options() => new()
    {
        DefaultLatitude = 0,
        DefaultLongitude = 0,
        DefaultRadiusKm = 5,
        MaximumResults = 100
    };

    private static Restaurant RestaurantWithEvidence(int index, AllergenEvidenceStatus status)
    {
        var restaurant = CreateRestaurant(0, 0, $"Evidence {index}");
        restaurant.MenuItems.Single().Allergens.Add(status == AllergenEvidenceStatus.VerifiedAbsent
            ? VerifiedAbsent("Peanuts")
            : new MenuItemAllergen
            {
                Allergen = new Allergen { Name = "Peanuts", Slug = "peanuts" },
                EvidenceStatus = status,
                CrossContactNote = status == AllergenEvidenceStatus.MayContainCrossContact
                    ? "Documented demo warning."
                    : null
            });
        return restaurant;
    }

    private static Restaurant CreateRestaurantWithNoAllergenRow(int index) =>
        CreateRestaurant(0, 0, $"Evidence {index}");

    private static MenuItemAllergen VerifiedAbsent(string name) =>
        new()
        {
            Allergen = new Allergen { Name = name, Slug = name.ToLowerInvariant() },
            EvidenceStatus = AllergenEvidenceStatus.VerifiedAbsent,
            Source = "Demo record",
            VerifiedOn = new DateOnly(2026, 10, 1)
        };

    private static MenuItem CreateItem(string name, decimal price) =>
        new()
        {
            Name = name,
            Description = "Fictional test menu item.",
            Price = price,
            IsAvailable = true
        };

    private static Cuisine Cuisine(string name) =>
        new() { Name = name, Slug = name.ToLowerInvariant() };

    private static IReadOnlySet<string> Set(string value) =>
        new HashSet<string>([value], StringComparer.OrdinalIgnoreCase);

    private sealed class InMemoryRestaurantRepository(IReadOnlyList<Restaurant> restaurants)
        : IRestaurantRepository
    {
        public Task<IReadOnlyList<Restaurant>> GetPublishedAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(restaurants);

        public Task<Restaurant?> GetPublishedBySlugAsync(
            string slug,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(restaurants.SingleOrDefault(restaurant => restaurant.Slug == slug));

        public Task<IReadOnlyList<Restaurant>> GetPublishedByIdsAsync(
            IReadOnlyCollection<int> ids,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Restaurant>>(
                restaurants.Where(restaurant => ids.Contains(restaurant.Id)).ToArray());

        public Task<IReadOnlyList<Restaurant>> GetPublishedWithinBoundsAsync(
            GeoBounds bounds,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Restaurant>>(
                restaurants.Where(restaurant => bounds.Contains(
                    restaurant.Latitude,
                    restaurant.Longitude)).ToArray());
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
