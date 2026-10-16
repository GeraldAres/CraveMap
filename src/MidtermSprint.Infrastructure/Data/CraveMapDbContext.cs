using Microsoft.EntityFrameworkCore;
using MidtermSprint.Domain.Entities;

namespace MidtermSprint.Infrastructure.Data;

public sealed class CraveMapDbContext(DbContextOptions<CraveMapDbContext> options) : DbContext(options)
{
    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<RestaurantHour> RestaurantHours => Set<RestaurantHour>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Cuisine> Cuisines => Set<Cuisine>();
    public DbSet<FoodNiche> FoodNiches => Set<FoodNiche>();
    public DbSet<MenuItemNiche> MenuItemNiches => Set<MenuItemNiche>();
    public DbSet<Characteristic> Characteristics => Set<Characteristic>();
    public DbSet<MenuItemCharacteristic> MenuItemCharacteristics => Set<MenuItemCharacteristic>();
    public DbSet<DietaryPreference> DietaryPreferences => Set<DietaryPreference>();
    public DbSet<MenuItemDietaryAttribute> MenuItemDietaryAttributes => Set<MenuItemDietaryAttribute>();
    public DbSet<Allergen> Allergens => Set<Allergen>();
    public DbSet<MenuItemAllergen> MenuItemAllergens => Set<MenuItemAllergen>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Restaurant>(entity =>
        {
            entity.HasKey(restaurant => restaurant.Id);
            entity.HasIndex(restaurant => restaurant.Slug).IsUnique();
            entity.HasIndex(restaurant => new { restaurant.Latitude, restaurant.Longitude });
            entity.Property(restaurant => restaurant.Name).HasMaxLength(160).IsRequired();
            entity.Property(restaurant => restaurant.Slug).HasMaxLength(180).IsRequired();
            entity.Property(restaurant => restaurant.Description).HasMaxLength(1200).IsRequired();
            entity.Property(restaurant => restaurant.Address).HasMaxLength(300).IsRequired();
            entity.Property(restaurant => restaurant.Rating).HasPrecision(2, 1);
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_Restaurants_PriceRange", "PriceRange BETWEEN 1 AND 3");
                table.HasCheckConstraint("CK_Restaurants_Latitude", "Latitude BETWEEN -90 AND 90");
                table.HasCheckConstraint("CK_Restaurants_Longitude", "Longitude BETWEEN -180 AND 180");
            });
        });

        modelBuilder.Entity<RestaurantHour>(entity =>
        {
            entity.HasKey(hour => new { hour.RestaurantId, hour.DayOfWeek });
            entity.HasOne(hour => hour.Restaurant)
                .WithMany(restaurant => restaurant.Hours)
                .HasForeignKey(hour => hour.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.HasIndex(item => new { item.RestaurantId, item.IsAvailable });
            entity.Property(item => item.Name).HasMaxLength(180).IsRequired();
            entity.Property(item => item.Description).HasMaxLength(1200).IsRequired();
            entity.Property(item => item.Price).HasPrecision(10, 2);
            entity.Property(item => item.MenuSection).HasMaxLength(100);
            entity.HasOne(item => item.Restaurant)
                .WithMany(restaurant => restaurant.MenuItems)
                .HasForeignKey(item => item.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(item => item.Cuisine)
                .WithMany()
                .HasForeignKey(item => item.CuisineId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        ConfigureNamedLookup<Cuisine>(modelBuilder);
        ConfigureNamedLookup<FoodNiche>(modelBuilder);
        ConfigureNamedLookup<DietaryPreference>(modelBuilder);
        ConfigureNamedLookup<Allergen>(modelBuilder);

        modelBuilder.Entity<Characteristic>(entity =>
        {
            entity.HasKey(characteristic => characteristic.Id);
            entity.HasIndex(characteristic => new { characteristic.Kind, characteristic.Name }).IsUnique();
            entity.Property(characteristic => characteristic.Name).HasMaxLength(80).IsRequired();
        });

        modelBuilder.Entity<MenuItemNiche>(entity =>
        {
            entity.HasKey(link => new { link.MenuItemId, link.FoodNicheId });
            entity.HasOne(link => link.MenuItem).WithMany(item => item.Niches).HasForeignKey(link => link.MenuItemId);
            entity.HasOne(link => link.FoodNiche).WithMany().HasForeignKey(link => link.FoodNicheId);
        });

        modelBuilder.Entity<MenuItemCharacteristic>(entity =>
        {
            entity.HasKey(link => new { link.MenuItemId, link.CharacteristicId });
            entity.HasOne(link => link.MenuItem).WithMany(item => item.Characteristics).HasForeignKey(link => link.MenuItemId);
            entity.HasOne(link => link.Characteristic).WithMany().HasForeignKey(link => link.CharacteristicId);
        });

        modelBuilder.Entity<MenuItemDietaryAttribute>(entity =>
        {
            entity.HasKey(link => new { link.MenuItemId, link.DietaryPreferenceId });
            entity.HasOne(link => link.MenuItem).WithMany(item => item.DietaryAttributes).HasForeignKey(link => link.MenuItemId);
            entity.HasOne(link => link.DietaryPreference).WithMany().HasForeignKey(link => link.DietaryPreferenceId);
        });

        modelBuilder.Entity<MenuItemAllergen>(entity =>
        {
            entity.HasKey(link => new { link.MenuItemId, link.AllergenId });
            entity.Property(link => link.Source).HasMaxLength(240);
            entity.Property(link => link.CrossContactNote).HasMaxLength(500);
            entity.Property(link => link.Notes).HasMaxLength(1000);
            entity.HasOne(link => link.MenuItem).WithMany(item => item.Allergens).HasForeignKey(link => link.MenuItemId);
            entity.HasOne(link => link.Allergen).WithMany().HasForeignKey(link => link.AllergenId);
        });
    }

    private static void ConfigureNamedLookup<TEntity>(
        ModelBuilder modelBuilder)
        where TEntity : class
    {
        var entity = modelBuilder.Entity<TEntity>();
        entity.HasKey("Id");
        entity.HasIndex("Name").IsUnique();
        entity.HasIndex("Slug").IsUnique();
        entity.Property<string>("Name").HasMaxLength(100).IsRequired();
        entity.Property<string>("Slug").HasMaxLength(120).IsRequired();
    }
}
