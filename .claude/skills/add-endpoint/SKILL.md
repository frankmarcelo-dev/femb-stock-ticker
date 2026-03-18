---
name: add-endpoint
description: Scaffold a new API endpoint for this project following the layered architecture (Controller → Service interface → Service implementation → Model). Use when adding a new resource or feature to the stock ticker API.
argument-hint: [resource-name]
disable-model-invocation: false
allowed-tools: Read, Glob, Grep, Edit, Write
---

Scaffold a new `$ARGUMENTS` API endpoint for this ASP.NET Core 8 project.

Follow the established layered architecture exactly as used in `WeatherForecast`:

## Steps

1. **Model** — Create `FembStockTicker/Models/$ARGUMENTS.cs`
   - Plain C# record or class with nullable-enabled properties

2. **Service interface** — Create `FembStockTicker/Services/I$ARGUMENTSService.cs`
   - Single interface with methods needed by the controller

3. **Service implementation** — Create `FembStockTicker/Services/$ARGUMENTSService.cs`
   - Implement the interface; register as scoped in `Program.cs`

4. **Controller** — Create `FembStockTicker/Controllers/$ARGUMENTSController.cs`
   - Route: `[Route("api/[controller]")]`
   - Inject `I$ARGUMENTSService` via constructor
   - Protect endpoints with `[Authorize(Policy = "read:stocks")]` or `write:stocks` as appropriate
   - Return `IActionResult` / `ActionResult<T>`

5. **Register the service** — In `Program.cs`, add:
   ```csharp
   builder.Services.AddScoped<I$ARGUMENTSService, $ARGUMENTSService>();
   ```

## Conventions to follow

- Use `ILogger<T>` injected via constructor for logging
- Namespace: `FembStockTicker.Controllers`, `FembStockTicker.Services`, `FembStockTicker.Models`
- Keep controllers thin — business logic belongs in the service
- Match the existing file and class naming style (PascalCase, no abbreviations)
- Do not add XML doc comments unless the user asks

## Reference files

Read these before generating any code:
- `FembStockTicker/Controllers/WeatherForecastController.cs`
- `FembStockTicker/Services/WeatherForecastService.cs`
- `FembStockTicker/Services/IWeatherForecastService.cs`
- `FembStockTicker/Auth0/Auth0ServiceExtensions.cs` (for available policy names)
- `FembStockTicker/Program.cs` (to find the right place to register the new service)
