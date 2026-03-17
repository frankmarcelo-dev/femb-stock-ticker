# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ASP.NET Core 8.0 Web API for a stock ticker application. Currently in early development with a placeholder WeatherForecast endpoint. Uses Auth0 for JWT-based authentication with scope-based authorization policies (`read:stocks`, `write:stocks`).

## Build and Run Commands

```bash
# Build
dotnet build --configuration Release

# Run (launches at http://localhost:7236, Swagger UI at /swagger)
dotnet run --project FembStockTicker

# Publish
dotnet publish -c Release -o <output-directory>
```

There is no test project configured yet.

## Architecture

**Solution:** `femb-stock-ticker.sln` containing a single project `FembStockTicker/`.

**Layered pattern:**
- `Controllers/` — API endpoints, route prefix: `/api/{controller}`
- `Services/` — Business logic via interface-based DI (e.g., `IWeatherForecastService` → `WeatherForecastService`)
- `Models/` — Data models
- `Config/` — Strongly-typed settings classes bound from `appsettings.json` (root: `AppSettings`)
- `Auth0/` — Auth0 authentication/authorization setup as extension methods (`Auth0ServiceExtensions`)
- `Middleware/` — `ExceptionHandlerMiddleware` (global error handling → JSON 500), `CorrelationIdHeaderMiddleware` (request tracing via GUID)
- `Swagger/` — Swagger/OpenAPI configuration with JWT Bearer security definition

**DI and pipeline setup** is in `Program.cs`. Middleware order: CorrelationId → Serilog request logging → exception handler → HTTPS redirect → auth → authorization → response caching → Swagger (dev only).

## Key Configuration

- `appsettings.json` — Auth0 domain/audience/scopes, API version, application host, logging (Serilog)
- Auth0 domain: configured via `appsettings.json` / user secrets (not committed)
- Authorization policies defined in `Auth0ServiceExtensions.cs` check for scope claims

## CI/CD

GitHub Actions workflow (`.github/workflows/azure-webapps-dotnet-core.yml`) deploys to Azure Web App on push to master. Targets .NET 8, app name `femb-stock-ticker`.
