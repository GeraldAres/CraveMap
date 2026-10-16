using MidtermSprint.Application.Search;
using MidtermSprint.Domain.Entities;
using MidtermSprint.Domain.Enums;

namespace MidtermSprint.Tests.Search;

public sealed class SearchScorerTests
{
    [Fact]
    public void Score_RenormalizesWeightsWhenRequestedCriteriaAreAbsent()
    {
        var scorer = new SearchScorer(new SearchScoringWeights());
        var restaurant = new Restaurant
        {
            Name = "Demo",
            Slug = "demo",
            Description = "Demo",
            Address = "Demo",
            Rating = null
        };
        var item = new MenuItem { Name = "Demo", Description = "Demo", Price = 100 };
        var specification = new SearchSpecification { RadiusKm = 5 };

        var score = scorer.Score(item, restaurant, specification, distanceKm: 0);

        Assert.Equal(0.75, score, precision: 8);
    }

    [Fact]
    public void Score_UsesOnlyMatchedEvidenceForCharacteristicsAndCuisine()
    {
        var scorer = new SearchScorer(new SearchScoringWeights());
        var restaurant = SearchServiceTests.CreateRestaurant(0, 0, rating: 5);
        var item = restaurant.MenuItems.Single();
        item.Cuisine = new Cuisine { Name = "Japanese", Slug = "japanese" };
        item.Characteristics.Add(new MenuItemCharacteristic
        {
            Characteristic = new Characteristic
            {
                Name = "Crispy",
                Kind = CharacteristicKind.Texture
            }
        });
        var specification = new SearchSpecification
        {
            Flavors = new HashSet<string>(["Spicy"]),
            Textures = new HashSet<string>(["Crispy"]),
            Cuisines = new HashSet<string>(["Japanese"]),
            RadiusKm = 5
        };

        var score = scorer.Score(item, restaurant, specification, distanceKm: 0);

        Assert.True(score > 0);
        Assert.True(score < 1);
    }
}
