using System.Globalization;
using System.Text.RegularExpressions;

namespace MidtermSprint.Application.Search;

/// <summary>
/// Interprets supported craving, budget, distance, dietary, and allergen phrases without network calls.
/// </summary>
public sealed class RuleBasedQueryInterpreter : IQueryInterpreter
{
    private static readonly RegexOptions PhraseOptions =
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled;

    private static readonly (string Name, string[] Aliases)[] Flavors =
    [
        ("Savory", ["savory", "salty", "malinamnam"]),
        ("Sweet", ["sweet", "matamis"]),
        ("Sour", ["sour", "maasim"]),
        ("Bitter", ["bitter"]),
        ("Umami", ["umami"]),
        ("Spicy", ["spicy", "maanghang"])
    ];

    private static readonly (string Name, string[] Aliases)[] Textures =
    [
        ("Crispy", ["crispy", "crisp", "crunchy-fried", "crunchy fried"]),
        ("Crunchy", ["crunchy"]),
        ("Creamy", ["creamy"]),
        ("Chewy", ["chewy"]),
        ("Soft", ["soft"])
    ];

    private static readonly (string Name, string[] Aliases)[] OtherCharacteristics =
    [
        ("Hot meals", ["hot meal", "hot meals"]),
        ("Cold dishes", ["cold dish", "cold dishes"]),
        ("Refreshing drinks", ["refreshing drink", "refreshing drinks"]),
        ("Comfort food", ["comfort food", "comfort foods"])
    ];

    private static readonly (string Name, string[] Aliases)[] Cuisines =
    [
        ("Filipino", ["filipino", "pinoy", "pilipino"]),
        ("Japanese", ["japanese", "japan"]),
        ("Korean", ["korean", "korea"]),
        ("Chinese", ["chinese", "china"]),
        ("Italian", ["italian", "italy"]),
        ("American", ["american", "usa"]),
        ("Fusion", ["fusion"])
    ];

    private static readonly (string Name, string[] Aliases)[] Niches =
    [
        ("Silog Meals", ["silog", "silog meal", "silog meals"]),
        ("Korean Street Food", ["korean street food", "tteokbokki", "kimbap"]),
        ("Ramen", ["ramen"]),
        ("Specialty Coffee", ["specialty coffee", "coffee", "espresso"]),
        ("Matcha", ["matcha"]),
        ("Fried Chicken", ["fried chicken", "crispy chicken"]),
        ("Milk Tea", ["milk tea", "boba", "bubble tea"]),
        ("Artisanal Desserts", ["artisanal dessert", "artisanal desserts", "dessert", "desserts", "pastry", "pastries"]),
        ("Filipino Comfort Food", ["filipino comfort food", "pinoy comfort food"]),
        ("Affordable Eats", ["affordable eats", "affordable meals", "cheap eats"]),
        ("Japanese Comfort Food", ["japanese comfort food"]),
        ("Pasta", ["pasta"])
    ];

    private static readonly (string Name, string[] Aliases)[] DietaryPreferences =
    [
        ("Vegetarian", ["vegetarian", "veggie"]),
        ("Vegan", ["vegan", "plant based", "plant-based"]),
        ("Halal", ["halal"]),
        ("Pescatarian", ["pescatarian"]),
        ("High-protein", ["high protein", "high-protein"]),
        ("Low-carb", ["low carb", "low-carb"])
    ];

    private static readonly (string Name, string[] Aliases)[] Allergens =
    [
        ("Peanuts", ["peanut", "peanuts"]),
        ("Tree nuts", ["tree nut", "tree nuts"]),
        ("Milk", ["milk", "dairy"]),
        ("Eggs", ["egg", "eggs"]),
        ("Soy", ["soy", "soya"]),
        ("Wheat", ["wheat", "gluten"]),
        ("Fish", ["fish"]),
        ("Crustaceans", ["crustacean", "crustaceans", "shrimp", "prawn", "prawns", "crab", "shellfish"]),
        ("Molluscs", ["mollusc", "molluscs", "shellfish", "mussel", "oyster", "clam", "squid"]),
        ("Sesame", ["sesame"])
    ];

    private static readonly Regex SoftBudgetPattern = new(
        @"(?:preferably|ideally|around|about|i(?:'|’)d\s+prefer(?:ably)?|i\s+would\s+prefer(?:ably)?)\s*(?:(?:under|below|less\s+than)\s*)?(?:₱\s*|php\s*|p\s*)?(\d{2,4})\s*(?:php|pesos?)?",
        PhraseOptions);

    private static readonly Regex StrictBudgetPattern = new(
        @"(?:under|below|less\s+than|no\s+more\s+than|at\s+most|max(?:imum)?)\s*(?:₱\s*|php\s*|p\s*)?(\d{2,4})\s*(?:php|pesos?)?",
        PhraseOptions);

    private static readonly Regex SuffixBudgetPattern = new(
        @"(?:₱\s*|php\s*|p\s*)(\d{2,4})\s*(?:max|maximum)\b",
        PhraseOptions);

    private static readonly Regex DistancePattern = new(
        @"\bwithin\s+(\d+(?:\.\d+)?)\s*(km|kilometers?|m|meters?)\b",
        PhraseOptions);

    private static readonly Regex WalkingDistancePattern = new(
        @"\bwalking\s+distance\b",
        PhraseOptions);

    private static readonly Regex HotFlavorPattern = new(
        @"(?<![\p{L}\p{N}])hot(?!\s+meals?\b)(?![\p{L}\p{N}])",
        PhraseOptions);

    private readonly double defaultRadiusKm;

    public RuleBasedQueryInterpreter(SearchOptions? options = null)
    {
        defaultRadiusKm = options?.DefaultRadiusKm ?? 5;
    }

    public ParsedCraving Interpret(string query)
    {
        ArgumentNullException.ThrowIfNull(query);
        var consumed = new bool[query.Length];
        var chips = new List<DetectedChip>();
        var flavors = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var textures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var other = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var cuisines = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var niches = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var diets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var allergens = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        AddAliases(query, consumed, Flavors, flavors, chips, SearchChipKind.Flavor);
        AddAliases(query, consumed, Textures, textures, chips, SearchChipKind.Texture);
        AddAliases(query, consumed, OtherCharacteristics, other, chips, SearchChipKind.OtherCharacteristic);
        AddAliases(query, consumed, Cuisines, cuisines, chips, SearchChipKind.Cuisine);
        AddAliases(query, consumed, Niches, niches, chips, SearchChipKind.Niche);
        AddAliases(query, consumed, DietaryPreferences, diets, chips, SearchChipKind.DietaryPreference);
        AddHotFlavor(query, consumed, flavors, chips);
        AddAllergenMatches(query, consumed, allergens, chips);

        decimal? maxPrice = null;
        var priceIsStrict = false;
        var budget = FindUnconsumed(query, consumed, SoftBudgetPattern)
                     ?? FindUnconsumed(query, consumed, StrictBudgetPattern)
                     ?? FindUnconsumed(query, consumed, SuffixBudgetPattern);
        if (budget is not null &&
            decimal.TryParse(budget.Groups[1].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var parsedPrice))
        {
            maxPrice = parsedPrice;
            var isSoft = budget.Value.StartsWith("preferably", StringComparison.OrdinalIgnoreCase) ||
                         budget.Value.StartsWith("ideally", StringComparison.OrdinalIgnoreCase) ||
                         budget.Value.StartsWith("around", StringComparison.OrdinalIgnoreCase) ||
                         budget.Value.StartsWith("about", StringComparison.OrdinalIgnoreCase) ||
                         budget.Value.StartsWith("i'd", StringComparison.OrdinalIgnoreCase) ||
                         budget.Value.StartsWith("i’d", StringComparison.OrdinalIgnoreCase) ||
                         budget.Value.StartsWith("i would", StringComparison.OrdinalIgnoreCase);
            priceIsStrict = !isSoft;
            MarkConsumed(consumed, budget);
            chips.Add(new DetectedChip(
                SearchChipKind.Budget,
                $"Up to ₱{parsedPrice:0.##}",
                budget.Value,
                priceIsStrict));
        }

        double? radiusKm = null;
        var distance = FindUnconsumed(query, consumed, DistancePattern);
        if (distance is not null &&
            double.TryParse(distance.Groups[1].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var amount))
        {
            var unit = distance.Groups[2].Value;
            radiusKm = unit.StartsWith('m') ? amount / 1000 : amount;
            MarkConsumed(consumed, distance);
            chips.Add(new DetectedChip(
                SearchChipKind.Distance,
                $"Within {radiusKm:0.##} km",
                distance.Value,
                true));
        }
        else if ((distance = FindUnconsumed(query, consumed, WalkingDistancePattern)) is not null)
        {
            radiusKm = 1;
            MarkConsumed(consumed, distance);
            chips.Add(new DetectedChip(SearchChipKind.Distance, "Within 1 km", distance.Value, true));
        }

        var cleanText = new string(query
            .Select((character, index) => consumed[index] ? ' ' : character)
            .ToArray());
        var unrecognized = Regex.Replace(cleanText, @"[\s,;.!?]+", " ").Trim();

        return new ParsedCraving(
            new SearchSpecification
            {
                Flavors = flavors,
                Textures = textures,
                OtherCharacteristics = other,
                Cuisines = cuisines,
                Niches = niches,
                DietaryPreferences = diets,
                AllergensToAvoid = allergens,
                MaxPrice = maxPrice,
                PriceIsStrict = priceIsStrict,
                RadiusKm = radiusKm ?? defaultRadiusKm,
                StrictAllergenMode = allergens.Count > 0
            },
            chips.DistinctBy(chip => (chip.Kind, chip.Label)).ToArray(),
            unrecognized);
    }

    private static void AddAliases(
        string query,
        bool[] consumed,
        IEnumerable<(string Name, string[] Aliases)> lexicon,
        ISet<string> values,
        ICollection<DetectedChip> chips,
        SearchChipKind kind)
    {
        foreach (var entry in lexicon)
        {
            foreach (var alias in entry.Aliases.OrderByDescending(value => value.Length))
            {
                var pattern = new Regex(
                    $@"(?<![\p{{L}}\p{{N}}]){Regex.Escape(alias)}(?![\p{{L}}\p{{N}}])",
                    PhraseOptions);
                foreach (Match match in pattern.Matches(query))
                {
                    values.Add(entry.Name);
                    MarkConsumed(consumed, match);
                    chips.Add(new DetectedChip(kind, entry.Name, match.Value, false));
                }
            }
        }
    }

    private static void AddHotFlavor(
        string query,
        bool[] consumed,
        ISet<string> values,
        ICollection<DetectedChip> chips)
    {
        foreach (Match match in HotFlavorPattern.Matches(query))
        {
            if (IsConsumed(consumed, match))
            {
                continue;
            }

            values.Add("Spicy");
            MarkConsumed(consumed, match);
            chips.Add(new DetectedChip(SearchChipKind.Flavor, "Spicy", match.Value, false));
        }
    }

    private static void AddAllergenMatches(
        string query,
        bool[] consumed,
        ISet<string> values,
        ICollection<DetectedChip> chips)
    {
        foreach (var allergen in Allergens)
        {
            var aliases = string.Join("|", allergen.Aliases
                .OrderByDescending(alias => alias.Length)
                .Select(Regex.Escape));
            var pattern = new Regex(
                $@"(?<![\p{{L}}\p{{N}}])(?:(?:no|without|avoid(?:ing)?|allergic\s+to)\s+(?:the\s+)?(?:{aliases})|(?:{aliases})[- ]free|free\s+(?:of|from)\s+(?:{aliases}))(?![\p{{L}}\p{{N}}])",
                PhraseOptions);

            foreach (Match match in pattern.Matches(query))
            {
                values.Add(allergen.Name);
                MarkConsumed(consumed, match);
                chips.Add(new DetectedChip(SearchChipKind.Allergen, allergen.Name, match.Value, true));
            }
        }
    }

    private static Match? FindUnconsumed(string query, bool[] consumed, Regex pattern)
    {
        foreach (Match match in pattern.Matches(query))
        {
            if (!IsConsumed(consumed, match))
            {
                return match;
            }
        }

        return null;
    }

    private static bool IsConsumed(bool[] consumed, Match match)
    {
        for (var index = match.Index; index < match.Index + match.Length; index++)
        {
            if (consumed[index])
            {
                return true;
            }
        }

        return false;
    }

    private static void MarkConsumed(bool[] consumed, Match match)
    {
        for (var index = match.Index; index < match.Index + match.Length; index++)
        {
            consumed[index] = true;
        }
    }
}
