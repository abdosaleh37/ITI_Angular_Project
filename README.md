# ITI Angular Project

This repository contains two main projects: a .NET backend API and an Angular frontend application.

Top-level structure

- `Backend/ITI_Project` - ASP.NET Core solution (API, BLL, DAL)
- `Frontend` - Angular application

Quick start (Windows / PowerShell)

1. Backend

   - Open the `Backend/ITI_Project` folder in Visual Studio or run from the command line:

     # From PowerShell
     dotnet build Backend/ITI_Project/ITI_Project.sln
     dotnet run --project Backend/ITI_Project/ITI_Project.API

   - Configuration files:
     - `Backend/ITI_Project/ITI_Project.API/appsettings.json` and `appsettings.Development.json` contain connection strings and other settings. Update the `ConnectionStrings` section before running.

   - Database migrations are in `Backend/ITI_Project/ITI_Project.DAL/Migrations`. To apply migrations:

     dotnet ef database update --project Backend/ITI_Project/ITI_Project.DAL --startup-project Backend/ITI_Project/ITI_Project.API

2. Frontend

   - From `Frontend` run the usual Angular commands:

     # From PowerShell
     cd Frontend
     npm install
     npm run start

   - The Angular app serves from `Frontend/src`. Main entry point is `src/main.ts`. Routes and components are under `src/app`.

Notes

- When running the frontend and backend locally, ensure the API base URL in the frontend configuration (`src/app/app.config.ts` or environment files) points to the running backend (e.g. `https://localhost:5001` / `http://localhost:5000`).
- The backend solution uses EF Core and has Identity-related entities. Review `Backend/ITI_Project/ITI_Project.DAL/Data/AppDbContext.cs` for DB context and entity sets.

If you want, I can add run/debug tasks, or expand any README with more details (env variables, CI, Dockerfile, sample requests).
