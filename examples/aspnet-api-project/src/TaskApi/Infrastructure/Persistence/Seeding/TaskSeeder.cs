using Microsoft.EntityFrameworkCore;
using TaskApi.Features.Tasks.Model;

namespace TaskApi.Infrastructure.Persistence.Seeding;

/// <summary>
/// Seeds the six starter tasks (mirroring the Angular client's original in-memory seed set) into an empty
/// database, so a freshly created dev database shows the same data the standalone SPA used to.
/// </summary>
internal static class TaskSeeder
{
    /// <summary>
    /// Adds the starter tasks when the <c>Tasks</c> table is empty; does nothing otherwise, so it is safe
    /// to call on every startup.
    /// </summary>
    /// <param name="db">The context to seed.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        if (await db.Tasks.AnyAsync(ct))
        {
            return;
        }

        var createdAt = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);

        TaskItem New(string title, string description, TaskState status, TaskPriority priority,
            DateOnly? dueDate, params string[] tags) =>
            TaskItem.Create(title, description, status, priority, dueDate, tags, Guid.NewGuid(), createdAt);

        db.Tasks.AddRange(
            New("Set up CI pipeline", "GitHub Actions build + test", TaskState.Active, TaskPriority.High, new DateOnly(2026, 6, 20), "devops"),
            New("Write onboarding docs", "README + first-run guide", TaskState.Todo, TaskPriority.Medium, new DateOnly(2026, 8, 1), "docs"),
            New("Fix flaky login test", "Race in the auth spec", TaskState.Blocked, TaskPriority.Critical, new DateOnly(2026, 6, 10), "bug", "auth"),
            New("Design dashboard", "Stats + charts", TaskState.Done, TaskPriority.Low, null, "ui"),
            New("Upgrade Angular", "Bump to latest, check breaking changes", TaskState.Todo, TaskPriority.High, new DateOnly(2026, 9, 15), "chore"),
            New("Add dark mode", "Theme tokens + toggle", TaskState.Active, TaskPriority.Low, null, "ui"));

        await db.SaveChangesAsync(ct);
    }
}
