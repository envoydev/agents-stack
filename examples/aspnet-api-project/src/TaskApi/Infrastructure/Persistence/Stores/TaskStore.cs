using Microsoft.EntityFrameworkCore;
using TaskApi.Features.Tasks.Model;

namespace TaskApi.Infrastructure.Persistence.Stores;

/// <summary>
/// EF Core-backed <see cref="ITaskStore"/>. Reads use the context's no-tracking default and are bounded
/// by <see cref="MaxResults"/>; mutations load the tracked entity, apply the change, and persist it.
/// </summary>
internal sealed class TaskStore(AppDbContext db) : ITaskStore
{
    // A hard ceiling so a read can never return an unbounded result set.
    private const int MaxResults = 500;

    /// <inheritdoc />
    public async Task<IReadOnlyList<TaskItem>> ListAsync(CancellationToken ct)
    {
        // SQLite cannot ORDER BY a DateTimeOffset column, so bound the read in SQL and order newest-first
        // in memory. The dataset is small and the client re-sorts anyway, so this stays cheap.
        var tasks = await db.Tasks.Take(MaxResults).ToListAsync(ct);
        return tasks.OrderByDescending(t => t.CreatedAt).ToArray();
    }

    /// <inheritdoc />
    public async Task<TaskItem?> FindAsync(Guid id, CancellationToken ct) =>
        await db.Tasks.FirstOrDefaultAsync(t => t.Id == id, ct);

    /// <inheritdoc />
    public async Task AddAsync(TaskItem task, CancellationToken ct)
    {
        db.Tasks.Add(task);
        await db.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(Guid id, Action<TaskItem> apply, CancellationToken ct)
    {
        var task = await db.Tasks.AsTracking().FirstOrDefaultAsync(t => t.Id == id, ct);
        if (task is null)
        {
            return false;
        }

        apply(task);
        await db.SaveChangesAsync(ct);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> RemoveAsync(Guid id, CancellationToken ct)
    {
        var affected = await db.Tasks.Where(t => t.Id == id).ExecuteDeleteAsync(ct);
        return affected > 0;
    }
}
