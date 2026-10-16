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

The Web project restores its npm dependencies and builds Tailwind and Leaflet assets as part of `dotnet build`/`dotnet run`. SQLite is created and seeded automatically in `src/MidtermSprint.Web/App_Data`.

## Architecture

- `MidtermSprint.Domain`: persistence-independent entities and enums.
- `MidtermSprint.Application`: application contracts and search logic.
- `MidtermSprint.Infrastructure`: SQLite persistence, migrations, repository, and fictional sample data.
- `MidtermSprint.Web`: Blazor Web App and composition root.
- `MidtermSprint.Tests`: automated tests.

The default sample area is Makati, Metro Manila. Change `Seed:CenterLatitude`, `Seed:CenterLongitude`, and `Seed:AreaName` in `src/MidtermSprint.Web/appsettings.json` to move the sample cluster. The seeded restaurant/menu/allergen records are fictional and must not be treated as real-world information.

To regenerate the database after changing seed data, stop the app and remove `src/MidtermSprint.Web/App_Data/cravemap.db`; the next run creates and seeds it again.
