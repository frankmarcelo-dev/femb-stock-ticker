# femb-stock-ticker

A production-ready **ASP.NET Core 8.0 Web API** for a stock ticker service. Designed with microservice best practices — clean layering, interface-driven DI, Auth0 JWT security, structured logging, distributed tracing, and CI/CD via GitHub Actions to Azure.

---

## Table of Contents

- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Design Patterns](#design-patterns)
- [Project Structure](#project-structure)
- [Middleware Pipeline](#middleware-pipeline)
- [Authentication & Authorization](#authentication--authorization)
- [Configuration](#configuration)
- [Observability](#observability)
- [API Documentation](#api-documentation)
- [Getting Started](#getting-started)
- [CI/CD](#cicd)
- [Roadmap](#roadmap)

---

## Tech Stack

| Concern              | Technology                               |
|----------------------|------------------------------------------|
| Framework            | ASP.NET Core 8.0                         |
| Authentication       | Auth0 (JWT Bearer via OIDC)              |
| Logging              | Serilog (structured, configurable sinks) |
| API Docs             | Swagger / OpenAPI 3                      |
| Observability        | Correlation ID tracing, New Relic        |
| Caching              | ASP.NET Response Caching                 |
| CI/CD                | GitHub Actions → Azure Web App           |

---

## Architecture

This service follows a **layered architecture** appropriate for a microservice: thin controllers delegate to a service layer, which contains all business logic. Repositories (not yet added) would sit below services and abstract data access.

```text
┌──────────────────────────────────────────────┐
│                  HTTP Clients                │
└────────────────────┬─────────────────────────┘
                     │
        ┌────────────▼────────────┐
        │    Middleware Pipeline  │  ← Correlation ID, Logging, Exception Handler
        └────────────┬────────────┘
                     │
        ┌────────────▼────────────┐
        │      Controllers        │  ← Route handling, Auth policies, HTTP semantics
        └────────────┬────────────┘
                     │
        ┌────────────▼────────────┐
        │   Service Layer         │  ← Business logic, interface-driven
        └────────────┬────────────┘
                     │
        ┌────────────▼────────────┐
        │   Repository Layer      │  ← (planned) Data access abstraction
        └────────────┬────────────┘
                     │
        ┌────────────▼────────────┐
        │   External / Data       │  ← Database, external stock APIs
        └─────────────────────────┘
```

### Key Principles

- **Single Responsibility** — each layer has one job; controllers don't contain business logic
- **Dependency Inversion** — all dependencies are injected via interfaces, not concrete types
- **Open/Closed** — new features extend the service layer without modifying controllers
- **Fail Fast** — required configuration (`AppSettings`) throws at startup if missing, preventing silent misconfigurations

---

## Design Patterns

### Interface-Based Dependency Injection

All services are registered against interfaces. This decouples consumers from implementations and enables unit testing via mock substitution.

```csharp
// Registration (Program.cs)
builder.Services.AddSingleton<IWeatherForecastService, WeatherForecastService>();

// Consumption (Controller)
public WeatherForecastController(IWeatherForecastService forecastService) { ... }
```

### Strongly-Typed Configuration

Settings are bound from `appsettings.json` to a typed `AppSettings` root object at startup. Nested sub-configurations (`Auth0Configuration`, `CacheSettings`, etc.) are passed directly to extension methods — no raw `IConfiguration` leaking into services.

```csharp
var appSettings = configuration.Get<AppSettings>()
    ?? throw new InvalidOperationException("AppSettings configuration is missing.");
builder.Services.AddSingleton(appSettings);
```

### Extension Method Pattern for Service Registration

Cross-cutting concerns (Auth0, Swagger) are organized as extension methods in their own namespaces. This keeps `Program.cs` clean and each concern self-contained and independently testable.

```csharp
builder.Services.AddAuth0Authentication(appSettings.Auth0);
builder.Services.AddAuth0Authorization(appSettings.Auth0);
```

### Global Exception Handler (Middleware)

`ExceptionHandlerMiddleware` catches all unhandled exceptions, logs them with Serilog, and returns a consistent `500` JSON response. Consumers always receive a structured error — never a stack trace or HTML error page.

```json
{ "error": "An error occurred while processing your request." }
```

### Correlation ID Propagation

`CorrelationIdHeaderMiddleware` ensures every request carries a `correlationId` header. If the caller provides one (upstream service), it is preserved. If not, a new GUID is generated. The ID is echoed back in the response header, enabling end-to-end trace correlation across microservices.

---

## Project Structure

```text
femb-stock-ticker/
├── FembStockTicker/
│   ├── Auth0/               # Auth0 extension methods (authn + authz setup)
│   ├── Config/              # Strongly-typed settings classes
│   │   ├── AppSettings.cs   # Root configuration object
│   │   ├── Auth0Configuration.cs
│   │   ├── CacheSettings.cs
│   │   ├── HealthCheckSettings.cs
│   │   └── ...
│   ├── Controllers/         # API endpoints — thin, delegate to services
│   ├── Middleware/
│   │   ├── CorrelationIdHeaderMiddleware.cs
│   │   └── ExceptionHandlerMiddleware.cs
│   ├── Models/              # Data transfer objects and domain models
│   ├── Services/            # Business logic behind interfaces
│   │   ├── IWeatherForecastService.cs
│   │   └── WeatherForecastService.cs
│   ├── Swagger/             # OpenAPI configuration and custom attributes
│   ├── Program.cs           # Composition root — DI wiring and middleware pipeline
│   ├── appsettings.json
│   └── appsettings.Development.json
├── .github/workflows/       # GitHub Actions CI/CD
└── femb-stock-ticker.sln
```

---

## Middleware Pipeline

Order matters in ASP.NET Core middleware. The pipeline is deliberately ordered for correctness and observability:

```text
Request →
  1. CorrelationIdHeaderMiddleware   — inject/propagate correlation ID
  2. Serilog Request Logging         — structured HTTP access logs with correlation context
  3. ExceptionHandlerMiddleware      — catch-all for unhandled exceptions
  4. HTTPS Redirection               — enforce TLS
  5. Authentication                  — validate JWT (Auth0)
  6. Authorization                   — enforce scope-based policies
  7. Response Caching                — cache eligible GET responses
  8. Swagger UI (Development only)   — interactive API explorer
  9. Controllers                     — route to endpoint handlers
← Response
```

Placing the exception handler **after** logging ensures that even error responses are logged with the correlation ID. Auth runs **before** controllers so protected routes are never reached unauthenticated.

---

## Authentication & Authorization

Auth is handled by **Auth0** using OpenID Connect with JWT Bearer tokens.

### Authentication

JWT tokens are validated against:

- `Issuer` — Auth0 authority URL
- `Audience` — API identifier registered in Auth0
- `Lifetime` — token expiry enforced
- `NameClaimType` — mapped to `ClaimTypes.NameIdentifier`

### Authorization Policies

Scope-based policies enforce fine-grained access control:

| Policy         | Required Scope  | Intended Use              |
|----------------|-----------------|---------------------------|
| `read:stocks`  | `read:stocks`   | GET endpoints (read data) |
| `write:stocks` | `write:stocks`  | POST/PUT endpoints        |

Apply policies on controllers or actions:

```csharp
[Authorize(Policy = "read:stocks")]
[HttpGet]
public ActionResult<Stock[]> GetStocks() { ... }
```

### Required Auth0 Configuration

Set these in `appsettings.Development.json` or as environment variables / Azure App Service settings:

```json
{
  "Auth0": {
    "Domain": "your-tenant.auth0.com",
    "Audience": "https://your-api-identifier",
    "ClientId": "...",
    "ClientSecret": "..."
  }
}
```

---

## Configuration

All settings are strongly typed under `AppSettings`:

| Section              | Purpose                                      |
|----------------------|----------------------------------------------|
| `AppConfiguration`   | API version, identifier, host, environment   |
| `Auth0`              | Auth0 domain, audience, scopes               |
| `Cache`              | Response cache duration settings             |
| `HealthCheck`        | Health probe configuration                   |
| `Api`                | External API directory / downstream services |
| `NEW_RELIC_*`        | New Relic APM agent configuration            |

Sensitive values (`Auth0.ClientSecret`, `NEW_RELIC_LICENSE_KEY`) should **never** be committed to source. Use:

- `dotnet user-secrets` for local development
- Azure Key Vault / App Service configuration for deployed environments

---

## Observability

### Structured Logging (Serilog)

Serilog is configured via `appsettings.json` with environment-specific overrides. All log entries are structured (not plain text), making them queryable in log aggregation tools.

```json
"Serilog": {
  "MinimumLevel": { "Default": "Information" },
  "WriteTo": [{ "Name": "Console" }]
}
```

Planned sinks: Splunk (`Config/Splunk.cs`), file, Azure Application Insights.

### Distributed Tracing

Every request carries a `correlationId` header. In a microservice mesh, pass this header to downstream service calls so the full request trace can be reconstructed across service boundaries.

### New Relic APM

`NEW_RELIC_APP_NAME` and `NEW_RELIC_LICENSE_KEY` in `AppSettings` wire up New Relic agent-based APM for performance monitoring and alerting in production.

---

## API Documentation

Swagger UI is available in **Development** mode at:

```text
http://localhost:7236/swagger
```

The Swagger definition includes:

- JWT Bearer security scheme (paste a token to authorize all requests)
- Per-endpoint auth requirements
- Custom `ConsumesHeader` attribute for documenting required request headers (e.g., `correlationId`)

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- An Auth0 tenant with an API registered (for authenticated endpoints)

### Run Locally

```bash
# Restore and build
dotnet build --configuration Release

# Set Auth0 secrets (one-time)
dotnet user-secrets set "Auth0:Domain" "your-tenant.auth0.com" --project FembStockTicker
dotnet user-secrets set "Auth0:Audience" "https://your-api-identifier" --project FembStockTicker
dotnet user-secrets set "Auth0:ClientId" "your clientId" --project FembStockTicker
dotnet user-secrets set "Auth0:ClientSecret" "your clientSecret" --project FembStockTicker

# Run — API at http://localhost:7236, Swagger at http://localhost:7236/swagger
dotnet run --project FembStockTicker
```

### Publish

```bash
dotnet publish -c Release -o ./publish
```

---

## CI/CD

GitHub Actions workflow at `.github/workflows/azure-webapps-dotnet-core.yml` deploys to **Azure Web App** on push to `master`.

> **Note:** The workflow currently specifies .NET 5. Update `dotnet-version` to `8.0.x` to match the project target framework.

Recommended secrets to configure in GitHub Actions:

- `AZURE_WEBAPP_PUBLISH_PROFILE`
- `AUTH0_DOMAIN`
- `AUTH0_AUDIENCE`
- `NEW_RELIC_LICENSE_KEY`

---

## Roadmap

- [ ] Replace placeholder `WeatherForecast` with `Stock` domain (quotes, ticker search, historical data)
- [ ] Add Repository layer with EF Core or Dapper
- [ ] Add Health Check endpoints (`/health`, `/health/ready`)
- [ ] Add rate limiting (ASP.NET Core built-in rate limiter)
- [ ] Add integration tests with `WebApplicationFactory<Program>`
- [ ] Update CI/CD pipeline to target .NET 8
- [ ] Add OpenTelemetry traces alongside Serilog logs
- [ ] Configure Splunk log forwarding via Serilog sink
