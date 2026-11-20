# CraveMap

CraveMap is a fictional, map-oriented restaurant discovery demo that matches cravings against menu items.

## Prerequisites

- .NET 10 SDK
- Node.js LTS and npm

## Run

From the solution root, run:

```powershell
dotnet run --project src/MidtermSprint.Web
```

The Web project restores its npm dependencies and builds Tailwind and Leaflet assets as part of `dotnet build`/`dotnet run`. The app uses the Supabase PostgreSQL database configured as `ConnectionStrings:Supabase`.

Set the connection string in user-secrets from the solution root (use the connection details from your Supabase dashboard; do not commit them):

```powershell
dotnet user-secrets set "ConnectionStrings:Supabase" "Host=<SUPABASE_HOST>;Port=5432;Database=postgres;Username=<SUPABASE_USERNAME>;Password=<SUPABASE_PASSWORD>;SSL Mode=Require;Trust Server Certificate=true" --project src/MidtermSprint.Web
```

Apply the PostgreSQL migrations from the solution root:

```powershell
dotnet ef database update --project src/MidtermSprint.Infrastructure --startup-project src/MidtermSprint.Web
```

Then run the app with `dotnet run --project src/MidtermSprint.Web`. On startup, migrations are checked and the fictional sample dataset is inserted only when all application tables are empty. A partially populated database is left unchanged.

## Demo features

- Explore fictional sample menus with craving search, filters, list/map views, and restaurant detail pages.
- Save restaurants in this browser’s local storage; no account or server-side saved list is used.
- Read about the demo and its data limitations at `/about`.

## Architecture

- `MidtermSprint.Domain`: persistence-independent entities and enums.
- `MidtermSprint.Application`: application contracts and search logic.
- `MidtermSprint.Infrastructure`: PostgreSQL persistence, migrations, repository, and fictional sample data.
- `MidtermSprint.Web`: Blazor Web App and composition root.
- `MidtermSprint.Tests`: automated tests.

The default sample area is Makati, Metro Manila. Change `Seed:CenterLatitude`, `Seed:CenterLongitude`, and `Seed:AreaName` in `src/MidtermSprint.Web/appsettings.json` to move the sample cluster. The seeded restaurant/menu/allergen records are fictional and must not be treated as real-world information. Seeding does not overwrite or append to any database with existing rows.
