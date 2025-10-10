# Backend - ITI_Project API

This folder contains the ASP.NET Core solution for the backend API. The solution lives in `ITI_Project` and includes three projects:

- `ITI_Project.API` - Web API project (controllers, Program.cs, configuration)
- `ITI_Project.BLL` - Business logic layer (repositories, unit of work)
- `ITI_Project.DAL` - Data access layer (EF Core DbContext, entities, migrations)

Key paths

- Solution: `Backend/ITI_Project/ITI_Project.sln`
- API: `Backend/ITI_Project/ITI_Project.API`
  - `Program.cs` - host and middleware configuration
  - `appsettings.json` / `appsettings.Development.json` - configuration and connection strings
  - Controllers: `Backend/ITI_Project/ITI_Project.API/Controllers`
- DAL: `Backend/ITI_Project/ITI_Project.DAL`
  - `Data/AppDbContext.cs` - EF Core DbContext
  - `Entities` - entity classes (Product, Order, OrderItem, ApplicationUser, ...)
  - `Migrations` - EF Core migrations

Running locally (PowerShell)

1. Build the solution:

   dotnet build Backend/ITI_Project/ITI_Project.sln

2. Update connection string:

   Edit `Backend/ITI_Project/ITI_Project.API/appsettings.Development.json` (or `appsettings.json`) and set the `ConnectionStrings:DefaultConnection` to your database (SQL Server, SQLite, etc.).

3. Apply EF Core migrations (if needed):

   dotnet tool restore
   dotnet ef database update --project Backend/ITI_Project/ITI_Project.DAL --startup-project Backend/ITI_Project/ITI_Project.API

4. Run the API:

   dotnet run --project Backend/ITI_Project/ITI_Project.API

Notes

- Ports and HTTPS: By default the API may run on HTTPS (e.g., https://localhost:5001). Check `Properties/launchSettings.json` for configured profiles and ports.
- If you don't have `dotnet-ef` installed globally, use `dotnet tool restore` to restore local tools or install with `dotnet tool install --global dotnet-ef`.
- Secrets and environment specific settings should be stored securely and not committed to source control.

Common troubleshooting

- Migration errors: ensure the `DefaultConnection` is valid and the database server is reachable.
- Port conflicts: change the application URL in `launchSettings.json` or pass `--urls` to `dotnet run`.
