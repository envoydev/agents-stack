using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaskApi.Infrastructure.Persistence;

namespace TaskApi.Tests.Integration;

/// <summary>
/// Boots the API in-process against a private in-memory SQLite database, so the endpoint tests exercise
/// real routing, validation, EF Core, and JSON serialization without touching the developer database.
/// </summary>
public sealed class TaskApiFactory : WebApplicationFactory<Program>
{
    // A single open connection keeps the in-memory database alive for the fixture's lifetime.
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
        });
    }

    /// <summary>
    /// Builds the schema on the shared connection. Call once before issuing the first request; the
    /// app's own startup migration/seed does not run under the Testing environment.
    /// </summary>
    public void CreateSchema()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
