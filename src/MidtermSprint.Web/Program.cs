using MidtermSprint.Web.Components;
using MidtermSprint.Infrastructure;
using MidtermSprint.Infrastructure.Seeding;
using MidtermSprint.Application.Search;
using Microsoft.Extensions.Options;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddCraveMapInfrastructure(builder.Configuration);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.Configure<SearchOptions>(options =>
{
    builder.Configuration.GetSection("Search").Bind(options);
    options.DefaultLatitude = double.Parse(
        builder.Configuration["Seed:CenterLatitude"] ?? throw new InvalidOperationException("Seed:CenterLatitude is required."),
        CultureInfo.InvariantCulture);
    options.DefaultLongitude = double.Parse(
        builder.Configuration["Seed:CenterLongitude"] ?? throw new InvalidOperationException("Seed:CenterLongitude is required."),
        CultureInfo.InvariantCulture);
});
builder.Services.AddSingleton(serviceProvider =>
    serviceProvider.GetRequiredService<IOptions<SearchOptions>>().Value);
builder.Services.AddSingleton<IQueryInterpreter, RuleBasedQueryInterpreter>();
builder.Services.AddScoped<ISearchService, SearchService>();

var app = builder.Build();
Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "App_Data"));

await using (var scope = app.Services.CreateAsyncScope())
{
    await scope.ServiceProvider.GetRequiredService<DatabaseSeeder>().InitializeAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
