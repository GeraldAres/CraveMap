namespace MidtermSprint.Domain.Entities;

public sealed class Restaurant
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public required string Description { get; set; }
    public required string Address { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int PriceRange { get; set; }
    public decimal? Rating { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public bool SupportsDineIn { get; set; }
    public bool SupportsTakeout { get; set; }
    public string? PhotoUrl { get; set; }
    public bool IsPublished { get; set; }
    public ICollection<RestaurantHour> Hours { get; set; } = new List<RestaurantHour>();
    public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
}

public sealed class RestaurantHour
{
    public int RestaurantId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly? OpensAt { get; set; }
    public TimeOnly? ClosesAt { get; set; }
    public bool IsClosed { get; set; }
    public Restaurant? Restaurant { get; set; }
}
