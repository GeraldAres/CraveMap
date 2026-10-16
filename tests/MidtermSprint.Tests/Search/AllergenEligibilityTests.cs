using MidtermSprint.Application.Search;
using MidtermSprint.Domain.Entities;
using MidtermSprint.Domain.Enums;

namespace MidtermSprint.Tests.Search;

public sealed class AllergenEligibilityTests
{
    [Fact]
    public void Evaluate_ContainsIsExcludedEvenWhenStrictModeIsOff()
    {
        var result = AllergenEligibility.Evaluate(
            ItemWithEvidence(AllergenEvidenceStatus.Contains, "Peanuts"),
            Set("Peanuts"),
            strictMode: false);

        Assert.Equal(AllergenMatchTier.Excluded, result.Tier);
    }

    [Theory]
    [InlineData(AllergenEvidenceStatus.MayContainCrossContact)]
    [InlineData(AllergenEvidenceStatus.Unknown)]
    public void Evaluate_UnverifiedEvidenceIsNeverStrictlyEligible(AllergenEvidenceStatus status)
    {
        var result = AllergenEligibility.Evaluate(
            ItemWithEvidence(status, "Peanuts"),
            Set("Peanuts"),
            strictMode: true);

        Assert.Equal(AllergenMatchTier.Excluded, result.Tier);
    }

    [Fact]
    public void Evaluate_MissingEvidenceIsUnknown()
    {
        var result = AllergenEligibility.Evaluate(
            new MenuItem { Name = "Demo", Description = "Demo", Price = 100 },
            Set("Peanuts"),
            strictMode: false);

        Assert.Equal(AllergenMatchTier.Unverified, result.Tier);
        Assert.Equal(AllergenEvidenceStatus.Unknown, Assert.Single(result.UnverifiedEvidence).EvidenceStatus);
    }

    [Fact]
    public void Evaluate_VerifiedAbsenceRequiresSourceAndDate()
    {
        var item = ItemWithEvidence(AllergenEvidenceStatus.VerifiedAbsent, "Peanuts");
        var evidence = item.Allergens.Single();
        evidence.Source = "Demo supplier record";
        evidence.VerifiedOn = new DateOnly(2026, 10, 1);

        Assert.Equal(AllergenMatchTier.Verified,
            AllergenEligibility.Evaluate(item, Set("Peanuts"), strictMode: true).Tier);

        evidence.Source = null;
        Assert.Equal(AllergenMatchTier.Excluded,
            AllergenEligibility.Evaluate(item, Set("Peanuts"), strictMode: true).Tier);
    }

    [Fact]
    public void Evaluate_CrossContactIsReturnedOnlyInTheUnverifiedTierWhenNonStrict()
    {
        var result = AllergenEligibility.Evaluate(
            ItemWithEvidence(AllergenEvidenceStatus.MayContainCrossContact, "Peanuts"),
            Set("Peanuts"),
            strictMode: false);

        Assert.Equal(AllergenMatchTier.Unverified, result.Tier);
        Assert.Equal(AllergenEvidenceStatus.MayContainCrossContact,
            Assert.Single(result.UnverifiedEvidence).EvidenceStatus);
    }

    private static MenuItem ItemWithEvidence(AllergenEvidenceStatus status, string allergenName)
    {
        var allergen = new Allergen { Name = allergenName, Slug = allergenName.ToLowerInvariant() };
        var item = new MenuItem { Name = "Demo", Description = "Demo", Price = 100 };
        item.Allergens.Add(new MenuItemAllergen
        {
            Allergen = allergen,
            EvidenceStatus = status
        });
        return item;
    }

    private static IReadOnlySet<string> Set(string value) =>
        new HashSet<string>([value], StringComparer.OrdinalIgnoreCase);
}
