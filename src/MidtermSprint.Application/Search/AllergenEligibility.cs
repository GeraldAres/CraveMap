using MidtermSprint.Domain.Entities;
using MidtermSprint.Domain.Enums;

namespace MidtermSprint.Application.Search;

public enum AllergenMatchTier
{
    Verified,
    Unverified,
    Excluded
}

public sealed record AllergenEvaluation(
    AllergenMatchTier Tier,
    IReadOnlyList<MenuItemAllergen> UnverifiedEvidence);

public static class AllergenEligibility
{
    public static AllergenEvaluation Evaluate(
        MenuItem item,
        IReadOnlySet<string> allergensToAvoid,
        bool strictMode)
    {
        if (allergensToAvoid.Count == 0)
        {
            return new AllergenEvaluation(AllergenMatchTier.Verified, Array.Empty<MenuItemAllergen>());
        }

        var unverified = new List<MenuItemAllergen>();
        foreach (var allergenName in allergensToAvoid)
        {
            var evidence = item.Allergens.FirstOrDefault(link =>
                string.Equals(link.Allergen?.Name, allergenName, StringComparison.OrdinalIgnoreCase));

            if (evidence?.EvidenceStatus == AllergenEvidenceStatus.Contains)
            {
                return new AllergenEvaluation(AllergenMatchTier.Excluded, Array.Empty<MenuItemAllergen>());
            }

            if (evidence?.EvidenceStatus == AllergenEvidenceStatus.VerifiedAbsent &&
                !string.IsNullOrWhiteSpace(evidence.Source) &&
                evidence.VerifiedOn.HasValue)
            {
                continue;
            }

            unverified.Add(evidence ?? new MenuItemAllergen
            {
                Allergen = new Allergen { Name = allergenName, Slug = string.Empty },
                EvidenceStatus = AllergenEvidenceStatus.Unknown
            });
        }

        if (unverified.Count == 0)
        {
            return new AllergenEvaluation(AllergenMatchTier.Verified, Array.Empty<MenuItemAllergen>());
        }

        return new AllergenEvaluation(
            strictMode ? AllergenMatchTier.Excluded : AllergenMatchTier.Unverified,
            unverified);
    }
}
