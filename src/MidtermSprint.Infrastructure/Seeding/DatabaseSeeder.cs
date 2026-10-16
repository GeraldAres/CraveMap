using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using MidtermSprint.Domain.Entities;
using MidtermSprint.Domain.Enums;
using MidtermSprint.Infrastructure.Data;

namespace MidtermSprint.Infrastructure.Seeding;

/// <summary>
/// Creates the initial SQLite schema and inserts the fictional discovery dataset once.
/// </summary>
public sealed class DatabaseSeeder(
    IDbContextFactory<CraveMapDbContext> contextFactory,
    IConfiguration configuration)
{
    private static readonly string[] CuisineNames =
    [
        "Filipino", "Japanese", "Korean", "Chinese", "Italian", "American", "Cafe", "Fusion"
    ];

    private static readonly string[] NicheNames =
    [
        "Silog Meals", "Korean Street Food", "Ramen", "Specialty Coffee", "Matcha",
        "Fried Chicken", "Milk Tea", "Artisanal Desserts", "Filipino Comfort Food",
        "Affordable Eats", "Japanese Comfort Food", "Pasta"
    ];

    private static readonly (CharacteristicKind Kind, string Name)[] CharacteristicNames =
    [
        (CharacteristicKind.Flavor, "Savory"), (CharacteristicKind.Flavor, "Sweet"),
        (CharacteristicKind.Flavor, "Sour"), (CharacteristicKind.Flavor, "Bitter"),
        (CharacteristicKind.Flavor, "Umami"), (CharacteristicKind.Flavor, "Spicy"),
        (CharacteristicKind.Texture, "Crispy"), (CharacteristicKind.Texture, "Crunchy"),
        (CharacteristicKind.Texture, "Creamy"), (CharacteristicKind.Texture, "Chewy"),
        (CharacteristicKind.Texture, "Soft"), (CharacteristicKind.Other, "Hot meals"),
        (CharacteristicKind.Other, "Cold dishes"), (CharacteristicKind.Other, "Refreshing drinks"),
        (CharacteristicKind.Other, "Comfort food")
    ];

    private static readonly string[] DietaryNames =
    [
        "Vegetarian", "Vegan", "Halal", "Pescatarian", "High-protein", "Low-carb"
    ];

    private static readonly string[] AllergenNames =
    [
        "Peanuts", "Tree nuts", "Milk", "Eggs", "Soy", "Wheat", "Fish",
        "Crustaceans", "Molluscs", "Sesame"
    ];

    private static readonly RestaurantSeed[] Restaurants =
    [
        new("Timpla Terrace", "Filipino", "Silog Meals",
            ["Crispy Chicken Longsilog", "Garlic Beef Tapa Rice", "Spicy Pork Sisig Bowl", "Ube Rice Pudding"]),
        new("Kanto Kubo", "Filipino", "Filipino Comfort Food",
            ["Crispy Chicken Adobo", "Sinigang Rice Bowl", "Savory Pork Barbecue", "Sweet Banana Turon"]),
        new("Sarsa & Sizzle", "Filipino", "Affordable Eats",
            ["Crispy Fish Rice Plate", "Chicken Inasal Meal", "Spicy Bicol Express Bowl", "Mango Sago Cup"]),
        new("Seoul Spoon", "Korean", "Korean Street Food",
            ["Spicy Korean Fried Chicken", "Savory Bulgogi Rice Bowl", "Crispy Mandu Dumplings", "Sweet Hotteok Pancake"]),
        new("Bap & Blaze", "Korean", "Korean Street Food",
            ["Spicy Gochujang Tofu Bowl", "Crispy Chicken Kimbap", "Savory Beef Bibimbap", "Chewy Tteokbokki"]),
        new("Hangul Bowl", "Korean", "Affordable Eats",
            ["Crispy Korean Chicken Cup", "Spicy Kimchi Fried Rice", "Savory Mushroom Bibimbap", "Sweet Yakgwa Bites"]),
        new("Miso Moon", "Japanese", "Ramen",
            ["Creamy Tonkotsu Ramen", "Savory Miso Ramen", "Crispy Chicken Karaage", "Chewy Matcha Mochi"]),
        new("Katsu Kite", "Japanese", "Japanese Comfort Food",
            ["Crispy Chicken Katsu", "Soft Curry Rice", "Savory Pork Gyoza", "Sweet Castella Cake"]),
        new("Nori Nest", "Japanese", "Affordable Eats",
            ["Crispy Tempura Rice Bowl", "Savory Salmon Onigiri", "Soft Udon Soup", "Sweet Hojicha Pudding"]),
        new("Wok Mallow", "Chinese", "Affordable Eats",
            ["Crispy Spring Rolls", "Savory Ginger Chicken Rice", "Spicy Mapo Tofu Bowl", "Soft Sesame Dumplings"]),
        new("Lantern Wok", "Chinese", "Filipino Comfort Food",
            ["Crispy Salted Chicken", "Savory Beef Noodle Soup", "Spicy Chili Wontons", "Sweet Mango Sago"]),
        new("Dumpling Orbit", "Chinese", "Affordable Eats",
            ["Crispy Pork Dumplings", "Soft Vegetable Bao", "Savory Scallion Noodles", "Chewy Sesame Balls"]),
        new("Nonna Nook", "Italian", "Pasta",
            ["Creamy Mushroom Pasta", "Savory Tomato Rigatoni", "Crispy Chicken Parmigiana", "Sweet Ricotta Tart"]),
        new("Pasta Post", "Italian", "Pasta",
            ["Creamy Alfredo Pasta", "Spicy Arrabbiata Penne", "Savory Pesto Gnocchi", "Soft Tiramisu Cup"]),
        new("Basil & Brick", "Italian", "Artisanal Desserts",
            ["Crispy Margherita Flatbread", "Savory Roasted Tomato Pasta", "Creamy Four Cheese Gnocchi", "Sweet Lemon Cake"]),
        new("Burger Comet", "American", "Fried Chicken",
            ["Crispy Chicken Burger", "Savory Mushroom Melt", "Crunchy Potato Fries", "Sweet Chocolate Brownie"]),
        new("Fryday Forge", "American", "Fried Chicken",
            ["Crispy Buttermilk Chicken", "Spicy Buffalo Chicken Bites", "Savory Beef Slider", "Creamy Vanilla Shake"]),
        new("Poblano Picnic", "Fusion", "Affordable Eats",
            ["Crispy Chicken Taco Rice", "Spicy Tofu Burrito Bowl", "Savory Corn Fritter Plate", "Sweet Churro Bites"]),
        new("Sip & Steam", "Cafe", "Specialty Coffee",
            ["Bitter Cold Brew Coffee", "Creamy Oat Latte", "Sweet Cinnamon Roll", "Chewy Chocolate Cookie"]),
        new("Bean Beacon", "Cafe", "Specialty Coffee",
            ["Bitter Espresso Tonic", "Creamy Caramel Latte", "Savory Cheese Toast", "Sweet Banana Bread"]),
        new("Matcha Meadow", "Cafe", "Matcha",
            ["Creamy Matcha Latte", "Sweet Matcha Mochi", "Chewy Matcha Waffle", "Cold Matcha Float"]),
        new("Tea Trolley", "Cafe", "Milk Tea",
            ["Chewy Brown Sugar Milk Tea", "Sweet Wintermelon Milk Tea", "Refreshing Calamansi Tea", "Creamy Taro Milk Tea"]),
        new("Sugar Compass", "Cafe", "Artisanal Desserts",
            ["Creamy Ube Cheesecake", "Sweet Mango Tart", "Chewy Brownie Bites", "Soft Vanilla Pudding"]),
        new("Cloudcrumb Atelier", "Cafe", "Artisanal Desserts",
            ["Sweet Strawberry Shortcake", "Creamy Chocolate Mousse", "Soft Lemon Madeleines", "Chewy Pistachio Cookie"]),
        new("Sinigang Stories", "Filipino", "Filipino Comfort Food",
            ["Sour Tamarind Vegetable Soup", "Savory Chicken Tinola Bowl", "Crispy Pork Kare-Kare", "Sweet Buko Pandan Cup"]),
        new("Rice Rocket", "Filipino", "Affordable Eats",
            ["Crispy Chicken Rice Box", "Savory Tofu Rice Plate", "Spicy Beef Pepper Bowl", "Soft Champorado Cup"]),
        new("Silog Station", "Filipino", "Silog Meals",
            ["Crispy Longganisa Silog", "Savory Beef Tapa Silog", "Spicy Chicken Tocino Rice", "Sweet Cassava Cake"]),
        new("Curry Cartograph", "Fusion", "Filipino Comfort Food",
            ["Creamy Coconut Curry Bowl", "Spicy Chickpea Rice Plate", "Savory Chicken Curry", "Soft Cardamom Pudding"]),
        new("Ramen Current", "Japanese", "Ramen",
            ["Creamy Shoyu Ramen", "Spicy Miso Ramen", "Soft Vegetable Udon", "Chewy Black Sesame Mochi"]),
        new("Milk Tea Meteor", "Cafe", "Milk Tea",
            ["Chewy Taro Milk Tea", "Sweet Brown Sugar Boba", "Refreshing Lychee Tea", "Creamy Matcha Milk Tea"]),
        new("Tofu Lantern", "Fusion", "Affordable Eats",
            ["Savory Tofu Garden Bowl", "Crispy Vegetable Spring Rolls", "Spicy Mushroom Rice", "Cold Citrus Fruit Cup"]),
        new("Noodle Nook", "Chinese", "Affordable Eats",
            ["Savory Mushroom Noodles", "Spicy Tofu Noodle Bowl", "Crispy Vegetable Wontons", "Soft Red Bean Bun"]),
        new("Empty Plate Cafe", "Cafe", "Specialty Coffee", [])
    ];

    /// <summary>
    /// Applies migrations and seeds the demonstration dataset when the database is empty.
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        await context.Database.MigrateAsync(cancellationToken);

        if (await context.Restaurants.AnyAsync(cancellationToken))
        {
            return;
        }

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var cuisines = CuisineNames.Select(name => new Cuisine { Name = name, Slug = Slugify(name) }).ToArray();
        var niches = NicheNames.Select(name => new FoodNiche { Name = name, Slug = Slugify(name) }).ToArray();
        var characteristics = CharacteristicNames
            .Select(item => new Characteristic { Name = item.Name, Kind = item.Kind })
            .ToArray();
        var diets = DietaryNames.Select(name => new DietaryPreference { Name = name, Slug = Slugify(name) }).ToArray();
        var allergens = AllergenNames.Select(name => new Allergen { Name = name, Slug = Slugify(name) }).ToArray();

        context.AddRange(cuisines);
        context.AddRange(niches);
        context.AddRange(characteristics);
        context.AddRange(diets);
        context.AddRange(allergens);
        await context.SaveChangesAsync(cancellationToken);

        var centerLatitude = double.Parse(
            configuration["Seed:CenterLatitude"] ?? throw new InvalidOperationException("Seed:CenterLatitude is required."),
            CultureInfo.InvariantCulture);
        var centerLongitude = double.Parse(
            configuration["Seed:CenterLongitude"] ?? throw new InvalidOperationException("Seed:CenterLongitude is required."),
            CultureInfo.InvariantCulture);
        var areaName = configuration["Seed:AreaName"] ?? "Makati, Metro Manila";
        var cuisineIds = cuisines.ToDictionary(item => item.Name, item => item.Id);
        var nicheIds = niches.ToDictionary(item => item.Name, item => item.Id);
        var characteristicIds = characteristics.ToDictionary(item => (item.Kind, item.Name), item => item.Id);
        var dietIds = diets.ToDictionary(item => item.Name, item => item.Id);
        var allergenIds = allergens.ToDictionary(item => item.Name, item => item.Id);

        var restaurants = Restaurants.Select((seed, index) =>
        {
            var radiusKm = 0.25 + (index % 8) * 0.55;
            var angle = index * 2.399963229728653;
            var latitude = centerLatitude + radiusKm / 111.045 * Math.Cos(angle);
            var longitude = centerLongitude + radiusKm / (111.045 * Math.Cos(centerLatitude * Math.PI / 180)) * Math.Sin(angle);
            return new Restaurant
            {
                Name = seed.Name,
                Slug = Slugify(seed.Name),
                Description = $"Fictional sample restaurant in the {areaName} demonstration area.",
                Address = $"Sample location {index + 1}, {areaName} (fictional)",
                Latitude = latitude,
                Longitude = longitude,
                PriceRange = index % 3 + 1,
                Rating = index == 9 ? null : 3.5m + index % 15 * 0.1m,
                SupportsDineIn = index % 9 != 0,
                SupportsTakeout = true,
                IsPublished = true
            };
        }).ToArray();

        context.Restaurants.AddRange(restaurants);
        await context.SaveChangesAsync(cancellationToken);

        var menuItems = new List<MenuItem>();
        var itemIndex = 0;
        for (var restaurantIndex = 0; restaurantIndex < Restaurants.Length; restaurantIndex++)
        {
            var seed = Restaurants[restaurantIndex];
            var restaurant = restaurants[restaurantIndex];

            foreach (var (name, menuIndex) in seed.MenuItems.Select((name, menuIndex) => (name, menuIndex)))
            {
                var item = new MenuItem
                {
                    RestaurantId = restaurant.Id,
                    Name = name,
                    Description = "Fictional sample menu item; preparation and ingredient details are not verified.",
                    MenuSection = menuIndex < 3 ? "Meals" : "Drinks and desserts",
                    Price = 75m + (itemIndex * 37 % 376),
                    CuisineId = cuisineIds[seed.Cuisine],
                    IsAvailable = true
                };

                AddCharacteristics(item, name, characteristicIds);
                item.Niches.Add(new MenuItemNiche { FoodNicheId = nicheIds[seed.Niche] });
                AddDietaryAttributes(item, name, dietIds);
                AddAllergenEvidence(item, itemIndex, allergenIds);
                menuItems.Add(item);
                itemIndex++;
            }
        }

        context.MenuItems.AddRange(menuItems);
        AddHours(context, restaurants);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private static void AddHours(CraveMapDbContext context, IReadOnlyList<Restaurant> restaurants)
    {
        foreach (var (restaurant, restaurantIndex) in restaurants.Select((restaurant, index) => (restaurant, index)))
        {
            var closedDay = (DayOfWeek)(restaurantIndex % 7);
            foreach (var day in Enum.GetValues<DayOfWeek>())
            {
                var isClosed = day == closedDay;
                var isLateNight = restaurantIndex % 6 == 0;
                context.RestaurantHours.Add(new RestaurantHour
                {
                    RestaurantId = restaurant.Id,
                    DayOfWeek = day,
                    IsClosed = isClosed,
                    OpensAt = isClosed ? null : isLateNight ? new TimeOnly(22, 0) : new TimeOnly(10, 0),
                    ClosesAt = isClosed ? null : isLateNight ? new TimeOnly(2, 0) : new TimeOnly(22, 0)
                });
            }
        }
    }

    private static void AddCharacteristics(
        MenuItem item,
        string name,
        IReadOnlyDictionary<(CharacteristicKind Kind, string Name), int> characteristicIds)
    {
        var text = name.ToLowerInvariant();
        var flavors = new HashSet<string>(StringComparer.Ordinal);
        var textures = new HashSet<string>(StringComparer.Ordinal);
        var other = new HashSet<string>(StringComparer.Ordinal);

        if (text.Contains("sweet") || text.Contains("dessert") || text.Contains("cake") ||
            text.Contains("pudding") || text.Contains("mochi") || text.Contains("tart") ||
            text.Contains("boba") || text.Contains("milk tea") || text.Contains("turon"))
        {
            flavors.Add("Sweet");
        }

        if (text.Contains("spicy") || text.Contains("chili") || text.Contains("gochujang"))
        {
            flavors.Add("Spicy");
        }

        if (text.Contains("sour") || text.Contains("tamarind") || text.Contains("calamansi"))
        {
            flavors.Add("Sour");
        }

        if (text.Contains("bitter") || text.Contains("coffee") || text.Contains("espresso"))
        {
            flavors.Add("Bitter");
        }

        if (text.Contains("ramen") || text.Contains("miso") || text.Contains("mushroom") ||
            text.Contains("beef") || text.Contains("savory") || text.Contains("tofu"))
        {
            flavors.Add("Umami");
        }

        if (!flavors.Contains("Sweet") && !flavors.Contains("Bitter") && !flavors.Contains("Sour"))
        {
            flavors.Add("Savory");
        }

        if (text.Contains("crispy") || text.Contains("crunchy") || text.Contains("fried") ||
            text.Contains("katsu") || text.Contains("karaage") || text.Contains("fritter"))
        {
            textures.Add("Crispy");
        }

        if (text.Contains("crunchy") || text.Contains("spring roll") || text.Contains("fries"))
        {
            textures.Add("Crunchy");
        }

        if (text.Contains("creamy") || text.Contains("latte") || text.Contains("mousse") ||
            text.Contains("pudding") || text.Contains("shake"))
        {
            textures.Add("Creamy");
        }

        if (text.Contains("chewy") || text.Contains("mochi") || text.Contains("boba") ||
            text.Contains("tteokbokki"))
        {
            textures.Add("Chewy");
        }

        if (text.Contains("soft") || text.Contains("soup") || text.Contains("ramen") ||
            text.Contains("curry") || text.Contains("stew") || text.Contains("tinola"))
        {
            textures.Add("Soft");
        }

        if (text.Contains("tea") || text.Contains("coffee") || text.Contains("cold") ||
            text.Contains("float") || text.Contains("tonic"))
        {
            other.Add("Refreshing drinks");
        }
        else
        {
            other.Add("Hot meals");
            other.Add("Comfort food");
        }

        foreach (var nameValue in flavors.Select(value => (CharacteristicKind.Flavor, value))
                     .Concat(textures.Select(value => (CharacteristicKind.Texture, value)))
                     .Concat(other.Select(value => (CharacteristicKind.Other, value))))
        {
            item.Characteristics.Add(new MenuItemCharacteristic
            {
                CharacteristicId = characteristicIds[nameValue]
            });
        }
    }

    private static void AddDietaryAttributes(
        MenuItem item,
        string name,
        IReadOnlyDictionary<string, int> dietIds)
    {
        var text = name.ToLowerInvariant();
        if (text.Contains("tofu") || text.Contains("vegetable") || text.Contains("chickpea"))
        {
            item.DietaryAttributes.Add(new MenuItemDietaryAttribute
            {
                DietaryPreferenceId = dietIds["Vegetarian"],
                IsVerified = true
            });
        }

        if (text.Contains("tofu garden") || text.Contains("vegetable spring rolls") || text.Contains("chickpea"))
        {
            item.DietaryAttributes.Add(new MenuItemDietaryAttribute
            {
                DietaryPreferenceId = dietIds["Vegan"],
                IsVerified = true
            });
        }
    }

    private static void AddAllergenEvidence(
        MenuItem item,
        int itemIndex,
        IReadOnlyDictionary<string, int> allergenIds)
    {
        var peanutStatus = (itemIndex % 4) switch
        {
            0 => AllergenEvidenceStatus.Contains,
            1 => AllergenEvidenceStatus.MayContainCrossContact,
            2 => AllergenEvidenceStatus.VerifiedAbsent,
            _ => AllergenEvidenceStatus.Unknown
        };

        item.Allergens.Add(new MenuItemAllergen
        {
            AllergenId = allergenIds["Peanuts"],
            EvidenceStatus = peanutStatus,
            Source = peanutStatus == AllergenEvidenceStatus.VerifiedAbsent ? "Fictional demo seed record" : null,
            VerifiedOn = peanutStatus == AllergenEvidenceStatus.VerifiedAbsent ? new DateOnly(2026, 10, 1) : null,
            CrossContactNote = peanutStatus == AllergenEvidenceStatus.MayContainCrossContact
                ? "Shared sample preparation equipment; fictional demo note."
                : null
        });

        if (itemIndex % 5 == 0)
        {
            item.Allergens.Add(new MenuItemAllergen
            {
                AllergenId = allergenIds["Eggs"],
                EvidenceStatus = AllergenEvidenceStatus.Contains,
                Notes = "Fictional demo seed evidence."
            });
        }
    }

    private static string Slugify(string value)
    {
        var characters = value.ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray();
        return string.Join('-', new string(characters).Split('-', StringSplitOptions.RemoveEmptyEntries));
    }

    private sealed record RestaurantSeed(
        string Name,
        string Cuisine,
        string Niche,
        string[] MenuItems);
}
