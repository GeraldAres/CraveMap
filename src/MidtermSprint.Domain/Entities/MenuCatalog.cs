using MidtermSprint.Domain.Enums;

namespace MidtermSprint.Domain.Entities;

public sealed class Cuisine
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
}

public sealed class FoodNiche
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
}

public sealed class MenuItemNiche
{
    public int MenuItemId { get; set; }
    public int FoodNicheId { get; set; }
    public MenuItem? MenuItem { get; set; }
    public FoodNiche? FoodNiche { get; set; }
}

public sealed class Characteristic
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public CharacteristicKind Kind { get; set; }
}

public sealed class MenuItemCharacteristic
{
    public int MenuItemId { get; set; }
    public int CharacteristicId { get; set; }
    public MenuItem? MenuItem { get; set; }
    public Characteristic? Characteristic { get; set; }
}

public sealed class DietaryPreference
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
}

public sealed class MenuItemDietaryAttribute
{
    public int MenuItemId { get; set; }
    public int DietaryPreferenceId { get; set; }
    public bool IsVerified { get; set; }
    public MenuItem? MenuItem { get; set; }
    public DietaryPreference? DietaryPreference { get; set; }
}

public sealed class Allergen
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
}

public sealed class MenuItemAllergen
{
    public int MenuItemId { get; set; }
    public int AllergenId { get; set; }
    public AllergenEvidenceStatus EvidenceStatus { get; set; }
    public string? Source { get; set; }
    public DateOnly? VerifiedOn { get; set; }
    public string? CrossContactNote { get; set; }
    public string? Notes { get; set; }
    public MenuItem? MenuItem { get; set; }
    public Allergen? Allergen { get; set; }
}
