## Fintrack

Fintrack is a learning-focused, Malaysia-ready e-invoice and expense management system. The workspace now boots with a `.NET 10` controller-based backend, a React + Vite frontend shell, and a repeatable local PostgreSQL setup.

## Workspace layout

```text
backend/
  Fintrack.slnx
  Fintrack.Api/
  tests/

frontend/
  src/
  .env.example

scripts/
  dev-up.ps1
  dev-down.ps1
  set-dev-secrets.ps1

compose.yaml
```

## Current stack

- Backend: ASP.NET Core Web API, single-project controller-based host, OpenAPI, ProblemDetails, health checks
- Frontend: React, Vite, TypeScript, React Router, Axios
- Database: PostgreSQL via Docker Compose
- Secrets path: .NET user secrets for backend-sensitive development values

## Prerequisites

- .NET SDK 10
- Node.js 22+ and npm
- Docker Desktop or Docker Engine with Compose

## Quick start

Run these commands from the repository root:

```powershell
dotnet restore backend/Fintrack.slnx
npm install --prefix frontend
.\scripts\dev-up.ps1
.\scripts\set-dev-secrets.ps1
```

After that, use two terminals:

Terminal 1, start the backend:

```powershell
dotnet run --project backend/Fintrack.Api
```

Terminal 2, start the frontend:

```powershell
npm run dev --prefix frontend
```

Then open `http://localhost:5173`.

## Local startup details

1. Start PostgreSQL:

```powershell
.\scripts\dev-up.ps1
```

2. Set backend development secrets:

```powershell
.\scripts\set-dev-secrets.ps1
```

3. Run the API:

```powershell
dotnet run --project backend/Fintrack.Api
```

4. Run the frontend:

```powershell
npm run dev --prefix frontend
```

## What gets initialized automatically

- PostgreSQL runs in Docker on `localhost:5432`
- The backend applies EF Core migrations automatically on startup in development
- The backend seeds roles automatically: `admin`, `accountant`, `staff`
- The backend creates the bootstrap admin user automatically if it does not already exist

## Seeded sign-in

Use these development credentials after the backend starts:

- Email: `admin@fintrack.local`
- Password: `ChangeMe123!`

## Development URLs

- Frontend: `http://localhost:5173`
- Backend HTTP: `http://localhost:5232`
- Backend HTTPS: `https://localhost:7093`
- OpenAPI: `http://localhost:5232/openapi/v1.json`
- Health check: `http://localhost:5232/health`

## Secret and environment configuration

Backend non-secret defaults live in the single API project:

- `backend/Fintrack.Api/appsettings.json`
- `backend/Fintrack.Api/appsettings.Development.json`

Backend secret examples live in [backend/.env.example](backend/.env.example), but the intended local path is user secrets:

```powershell
dotnet user-secrets set --project backend/Fintrack.Api/Fintrack.Api.csproj "ConnectionStrings:Postgres" "Host=localhost;Port=5432;Database=fintrack;Username=fintrack;Password=fintrack"
dotnet user-secrets set --project backend/Fintrack.Api/Fintrack.Api.csproj "Jwt:SigningKey" "replace-with-a-32-character-development-secret"
dotnet user-secrets set --project backend/Fintrack.Api/Fintrack.Api.csproj "BootstrapAdmin:Email" "admin@fintrack.local"
dotnet user-secrets set --project backend/Fintrack.Api/Fintrack.Api.csproj "BootstrapAdmin:Password" "ChangeMe123!"
dotnet user-secrets set --project backend/Fintrack.Api/Fintrack.Api.csproj "BootstrapAdmin:CompanyName" "Fintrack Demo Sdn. Bhd."
```

Frontend environment examples live in [frontend/.env.example](frontend/.env.example):

```text
VITE_API_BASE_URL=http://localhost:5232
VITE_PROXY_TARGET=http://localhost:5232
```

## Seeded admin bootstrap path

The workspace now includes a repeatable configuration path for the initial development administrator via `.\scripts\set-dev-secrets.ps1`. The actual Identity-backed role and admin seeding flow will be wired in the next task group, but the credentials and company bootstrap values are already standardized and documented inside the single backend project setup.

## Stop the stack

Stop the backend and frontend with `Ctrl+C`, then stop PostgreSQL with:

```powershell
.\scripts\dev-down.ps1
```

## Useful commands

```powershell
dotnet restore backend/Fintrack.slnx
dotnet build backend/Fintrack.slnx
dotnet test backend/Fintrack.slnx
npm run typecheck --prefix frontend
npm run build --prefix frontend
.\scripts\dev-down.ps1
```

---
