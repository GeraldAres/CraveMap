using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MidtermSprint.Infrastructure.Data;

namespace MidtermSprint.Web;

public sealed class DesignTimeCraveMapDbContextFactory
    : IDesignTimeDbContextFactory<CraveMapDbContext>
{
    public CraveMapDbContext CreateDbContext(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Configuration.AddUserSecrets(
            typeof(DesignTimeCraveMapDbContextFactory).Assembly,
            optional: true);

        var connectionString = builder.Configuration.GetConnectionString("Supabase");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:Supabase is required.");
        }
        var options = new DbContextOptionsBuilder<CraveMapDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new CraveMapDbContext(options);
    }
}
