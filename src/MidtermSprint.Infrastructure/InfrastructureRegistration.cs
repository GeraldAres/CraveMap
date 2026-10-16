using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MidtermSprint.Application.Abstractions;
using MidtermSprint.Infrastructure.Data;
using MidtermSprint.Infrastructure.Seeding;

namespace MidtermSprint.Infrastructure;

public static class InfrastructureRegistration
{
    /// <summary>
    /// Registers SQLite persistence and the repository implementations.
    /// </summary>
    public static IServiceCollection AddCraveMapInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CraveMap")
            ?? throw new InvalidOperationException("ConnectionStrings:CraveMap is required.");

        services.AddDbContextFactory<CraveMapDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IRestaurantRepository, RestaurantRepository>();
        services.AddSingleton<DatabaseSeeder>();
        return services;
    }
}
