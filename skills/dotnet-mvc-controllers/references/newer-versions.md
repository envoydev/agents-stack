# Newer versions (optional)

SKILL.md owns the .NET 8 / C# 12 baseline; this file carries the optional .NET 9/10 deltas.

- **.NET 9+:** the built-in OpenAPI generator (`AddOpenApi()` / `MapOpenApi()`) covers controllers too and supersedes Swashbuckle - see `dotnet-openapi`.
- **.NET 10+:** the unified validation APIs move to the `Microsoft.Extensions.Validation` package and OpenAPI generation from controllers improves (form-data enum types, merged XML docs from referenced assemblies). The built-in validation remains attribute-per-property with no cross-field or async rule, so the FluentValidation-in-a-filter convention still carries those cases and stays the house default - per `dotnet-web-backend`.
- **.NET 10+:** `IActionContextAccessor` / `ActionContextAccessor` are obsoleted - where an action genuinely needs ambient request context outside its parameter list, inject `IHttpContextAccessor` instead. Preferring an explicit action parameter over either accessor remains the rule.
