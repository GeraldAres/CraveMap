namespace MidtermSprint.Domain.Entities;

public sealed class MenuItem
{
    public int Id { get; set; }
    public int RestaurantId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public string? Ingredients { get; set; }
    public string? MenuSection { get; set; }
    public decimal Price { get; set; }
    public int CuisineId { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsAvailable { get; set; }
    public Restaurant? Restaurant { get; set; }
    public Cuisine? Cuisine { get; set; }
    public ICollection<MenuItemNiche> Niches { get; set; } = new List<MenuItemNiche>();
    public ICollection<MenuItemCharacteristic> Characteristics { get; set; } = new List<MenuItemCharacteristic>();
    public ICollection<MenuItemDietaryAttribute> DietaryAttributes { get; set; } = new List<MenuItemDietaryAttribute>();
    public ICollection<MenuItemAllergen> Allergens { get; set; } = new List<MenuItemAllergen>();
}
