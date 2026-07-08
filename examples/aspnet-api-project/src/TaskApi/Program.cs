using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TaskApi.Features.Tasks.CreateTask;
using TaskApi.Features.Tasks.DeleteTask;
using TaskApi.Features.Tasks.GetTask;
using TaskApi.Features.Tasks.GetTaskStats;
using TaskApi.Features.Tasks.ListTasks;
using TaskApi.Features.Tasks.UpdateTask;
using TaskApi.Infrastructure.Errors;
using TaskApi.Infrastructure.Persistence;
using TaskApi.Infrastructure.Persistence.Seeding;
using TaskApi.Infrastructure.Persistence.Stores;

var builder = WebApplication.CreateBuilder(args);

const string spaCorsPolicy = "spa";

// --- Services ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<ITaskStore, TaskStore>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskValidator>(includeInternalTypes: true);

// Serialize enums as their camelCase names ('todo', 'critical', ...) to match the Angular contract.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();

// Dev-only CORS for the Angular dev server. NOTE: the house skills define no CORS convention; in dev the
// SPA proxies /api same-origin, so this only matters for a direct cross-origin call from :4200.
builder.Services.AddCors(options =>
    options.AddPolicy(spaCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// --- Pipeline ---
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseCors(spaCorsPolicy);
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/health", () => TypedResults.Ok(new { status = "healthy" }))
    .WithName("Health")
    .ExcludeFromDescription();

var tasks = app.MapGroup("/api/tasks").WithTags("Tasks");
ListTasksEndpoint.Map(tasks);
GetTaskEndpoint.Map(tasks);
CreateTaskEndpoint.Map(tasks);
UpdateTaskEndpoint.Map(tasks);
DeleteTaskEndpoint.Map(tasks);
GetTaskStatsEndpoint.Map(tasks);

// Dev convenience: create/upgrade the local SQLite database and seed the starter tasks on startup.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await TaskSeeder.SeedAsync(db);
}

app.Run();

/// <summary>Entry-point marker so the integration tests can reference the running host via WebApplicationFactory.</summary>
public partial class Program;
