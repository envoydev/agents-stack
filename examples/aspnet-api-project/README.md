# aspnet-api-project

A .NET 10 **Task API** - the backend for the `../angular-project` Task Playground and a worked example of
the house .NET conventions. Minimal API + vertical slice, EF Core + SQLite. Part of the `examples/`
testbed for the agent stack (see the design spec under `docs/superpowers/specs/`).

## Stack

- **.NET 10**, Minimal API, C# with nullable + file-scoped namespaces
- **Vertical slice** feature folders under `src/TaskApi/Features/Tasks/*`
- **EF Core + SQLite** (`app.db`), migrations via the pinned `dotnet-ef` tool
- **FluentValidation** endpoint filter, **RFC 9457 ProblemDetails** errors, built-in **OpenAPI** + Scalar UI
- Central Package Management (`Directory.Packages.props`), shared build props (`Directory.Build.props`)
- Tests: **xUnit + NSubstitute + FluentAssertions 7.x** (unit) and `WebApplicationFactory` (integration)

## Layout

```
src/TaskApi/
  Program.cs                     composition root (services + each slice's Map)
  Features/Tasks/
    Model/                       TaskItem entity, TaskState/TaskPriority enums, TaskDto, mapping
    ListTasks/ GetTask/ CreateTask/ UpdateTask/ DeleteTask/ GetTaskStats/
  Infrastructure/
    Persistence/                 AppDbContext, TaskConfiguration, ITaskStore/TaskStore, seeder, Migrations
    Errors/ Validation/          GlobalExceptionHandler, ValidationFilter
tests/TaskApi.Tests/  Unit/ Integration/
```

## Endpoints

| Verb + route | Purpose |
| --- | --- |
| `GET /api/tasks` | list all tasks (newest first) |
| `GET /api/tasks/{id}` | one task, or 404 |
| `POST /api/tasks` | create (201 + Location) |
| `PUT /api/tasks/{id}` | replace editable fields, or 404 |
| `DELETE /api/tasks/{id}` | delete (204), or 404 |
| `GET /api/tasks/stats` | dashboard aggregate |
| `GET /health` | liveness |

The JSON matches the Angular `Task` model exactly: enums serialize camelCase (`todo`, `critical`),
`id` is a GUID string, `dueDate` is an ISO date (`yyyy-MM-dd`) or null.

## Run

```bash
dotnet tool restore                       # first clone only - restores dotnet-ef
dotnet run --project src/TaskApi          # http://localhost:5080  (dev: migrates + seeds app.db)
```

OpenAPI at `/openapi/v1.json`, Scalar docs at `/scalar` (Development only).

Run with the frontend (from `../angular-project`, `npm start`): the Angular dev server proxies `/api`
to `:5080` (see its `proxy.conf.json`), so the SPA calls relative `/api/tasks` with no CORS.

## Test

```bash
dotnet test                               # 9 tests: domain + handler units, WebApplicationFactory integration
```

## Known issues (deliberately not silenced)

- Two transitive **NU1903** advisories ship inside MS's own fresh .NET 10 packages
  (`Microsoft.OpenApi 2.0.0`, `SQLitePCLRaw.lib.e_sqlite3 2.1.11`). The only patched `e_sqlite3` is a
  3.x major that mismatches EF Core 10's SQLitePCLRaw set, so they are left as build warnings pending a
  Microsoft patch rather than force-pinned.
- The dev **CORS** policy is intentionally permissive and dev-only; there is no house CORS convention,
  and the proxy makes it moot in normal dev use.
