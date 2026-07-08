using TaskApi.Features.Tasks.Model;

namespace TaskApi.Infrastructure.Persistence.Stores;

/// <summary>
/// The data-access surface for tasks. Purpose-built rather than a generic repository: each method is one
/// intentful query or mutation, reads stay bounded and no-tracking, and the request's cancellation token
/// flows through to EF Core. Injected into every task feature slice.
/// </summary>
internal interface ITaskStore
{
    /// <summary>Returns the most recent tasks, newest first, bounded to a fixed maximum.</summary>
    /// <param name="ct">Cancellation token for the request.</param>
    /// <returns>A read-only, no-tracking list of tasks.</returns>
    Task<IReadOnlyList<TaskItem>> ListAsync(CancellationToken ct);

    /// <summary>Finds a single task by identifier.</summary>
    /// <param name="id">The task identifier.</param>
    /// <param name="ct">Cancellation token for the request.</param>
    /// <returns>The task, or <see langword="null"/> when no task has that identifier.</returns>
    Task<TaskItem?> FindAsync(Guid id, CancellationToken ct);

    /// <summary>Persists a new task.</summary>
    /// <param name="task">The task to add.</param>
    /// <param name="ct">Cancellation token for the request.</param>
    Task AddAsync(TaskItem task, CancellationToken ct);

    /// <summary>
    /// Loads the tracked task, applies <paramref name="apply"/> to it, and persists the change.
    /// </summary>
    /// <param name="id">The task identifier.</param>
    /// <param name="apply">The mutation to run against the loaded entity.</param>
    /// <param name="ct">Cancellation token for the request.</param>
    /// <returns><see langword="true"/> when the task existed and was updated; otherwise <see langword="false"/>.</returns>
    Task<bool> UpdateAsync(Guid id, Action<TaskItem> apply, CancellationToken ct);

    /// <summary>Deletes a task by identifier.</summary>
    /// <param name="id">The task identifier.</param>
    /// <param name="ct">Cancellation token for the request.</param>
    /// <returns><see langword="true"/> when a task was deleted; otherwise <see langword="false"/>.</returns>
    Task<bool> RemoveAsync(Guid id, CancellationToken ct);
}
