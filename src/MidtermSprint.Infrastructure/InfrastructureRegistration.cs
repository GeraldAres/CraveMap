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
    /// Registers PostgreSQL persistence and the repository implementations.
    /// </summary>
    public static IServiceCollection AddCraveMapInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Supabase");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:Supabase is required.");
        }

        services.AddDbContextFactory<CraveMapDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IRestaurantRepository, RestaurantRepository>();
        services.AddSingleton<DatabaseSeeder>();
        return services;
    }
}
