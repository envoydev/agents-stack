using Microsoft.EntityFrameworkCore;
using TaskApi.Features.Tasks.Model;

namespace TaskApi.Infrastructure.Persistence;

/// <summary>
/// The EF Core context for the task store. Reads are no-tracking by default (set in the constructor);
/// a query that intends to mutate opts back in with <c>AsTracking()</c>.
/// </summary>
public sealed class AppDbContext : DbContext
{
    /// <summary>
    /// Creates the context with the supplied options and pins the query-tracking behaviour to
    /// no-tracking, so read queries never accumulate change-tracker state.
    /// </summary>
    /// <param name="options">The context options configured by dependency injection.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    /// <summary>The set of tasks.</summary>
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
