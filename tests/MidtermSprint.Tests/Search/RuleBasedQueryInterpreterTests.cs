using MidtermSprint.Application.Search;

namespace MidtermSprint.Tests.Search;

public sealed class RuleBasedQueryInterpreterTests
{
    private readonly RuleBasedQueryInterpreter interpreter = new();

    [Fact]
    public void Interpret_RecognizesCharacteristicsCuisineBudgetAndDistanceTogether()
    {
        var parsed = interpreter.Interpret("Spicy Korean food under ₱250 within 2 km");

        Assert.Contains("Spicy", parsed.Specification.Flavors);
        Assert.Contains("Korean", parsed.Specification.Cuisines);
        Assert.Equal(250m, parsed.Specification.MaxPrice);
        Assert.True(parsed.Specification.PriceIsStrict);
        Assert.Equal(2, parsed.Specification.RadiusKm);
    }

    [Fact]
    public void Interpret_RecognizesSynonymsAndKeepsUnrecognizedWords()
    {
        var parsed = interpreter.Interpret("malinamnam and crunchy-fried chicken near the station");

        Assert.Contains("Savory", parsed.Specification.Flavors);
        Assert.Contains("Crispy", parsed.Specification.Textures);
        Assert.Contains("Fried Chicken", parsed.Specification.Niches);
        Assert.Contains("station", parsed.UnrecognizedText, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("under ₱200", true)]
    [InlineData("below 200 pesos", true)]
    [InlineData("P250 max", true)]
    [InlineData("preferably under 200", false)]
    [InlineData("ideally around ₱200", false)]
    public void Interpret_DistinguishesStrictAndSoftBudgets(string query, bool isStrict)
    {
        var parsed = interpreter.Interpret(query);

        Assert.NotNull(parsed.Specification.MaxPrice);
        Assert.Equal(isStrict, parsed.Specification.PriceIsStrict);
    }

    [Theory]
    [InlineData("within 2km", 2)]
    [InlineData("within 500 m", 0.5)]
    [InlineData("walking distance", 1)]
    public void Interpret_ConvertsDistanceUnits(string query, double radiusKm)
    {
        Assert.Equal(radiusKm, interpreter.Interpret(query).Specification.RadiusKm);
    }

    [Fact]
    public void Interpret_RequiresNegativeLanguageForAllergenFilters()
    {
        var parsed = interpreter.Interpret("without peanuts and avoid shellfish");

        Assert.Contains("Peanuts", parsed.Specification.AllergensToAvoid);
        Assert.Contains("Crustaceans", parsed.Specification.AllergensToAvoid);
        Assert.Contains("Molluscs", parsed.Specification.AllergensToAvoid);
        Assert.True(parsed.Specification.StrictAllergenMode);
        Assert.Contains(parsed.DetectedChips, chip =>
            chip.Kind == SearchChipKind.Allergen && chip.IsStrict);
    }

    [Fact]
    public void Interpret_DoesNotTreatHotMealsAsSpicy()
    {
        var parsed = interpreter.Interpret("hot meals and cold dishes");

        Assert.Contains("Hot meals", parsed.Specification.OtherCharacteristics);
        Assert.Contains("Cold dishes", parsed.Specification.OtherCharacteristics);
        Assert.DoesNotContain("Spicy", parsed.Specification.Flavors);
    }

    [Fact]
    public void Merge_UnionsFiltersAndPreservesManualAllergensAndScalarOverrides()
    {
        var parsed = interpreter.Interpret("creamy pasta under 300");
        var merged = SearchSpecificationMerger.Merge(
            parsed.Specification,
            new ManualSearchFilters
            {
                Cuisines = new HashSet<string>(["Italian"]),
                DietaryPreferences = new HashSet<string>(["Vegan"]),
                AllergensToAvoid = new HashSet<string>(["Peanuts"]),
                MaxPrice = 250,
                PriceIsStrict = true,
                RadiusKm = 3
            },
            new SearchOptions());

        Assert.Contains("Creamy", merged.Textures);
        Assert.Contains("Pasta", merged.Niches);
        Assert.Contains("Italian", merged.Cuisines);
        Assert.Contains("Vegan", merged.DietaryPreferences);
        Assert.Contains("Peanuts", merged.AllergensToAvoid);
        Assert.True(merged.StrictAllergenMode);
        Assert.Equal(250, merged.MaxPrice);
        Assert.True(merged.PriceIsStrict);
        Assert.Equal(3, merged.RadiusKm);
    }
}
