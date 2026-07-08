using TaskApi.Features.Tasks.Model;

namespace TaskApi.Features.Tasks.CreateTask;

/// <summary>
/// The payload accepted when creating a task. Mirrors the Angular client's <c>NewTask</c>: title and
/// priority are required; description, due date, tags, and initial status are optional and defaulted by
/// the handler.
/// </summary>
/// <param name="Title">Short human-readable name; required.</param>
/// <param name="Description">Longer description; defaults to empty when omitted.</param>
/// <param name="Priority">Urgency; required.</param>
/// <param name="DueDate">Optional deadline as an ISO date, or null.</param>
/// <param name="Tags">Optional labels; defaults to an empty list.</param>
/// <param name="Status">Optional initial state; defaults to <see cref="TaskState.Todo"/>.</param>
public sealed record CreateTaskRequest(
    string Title,
    string? Description,
    TaskPriority Priority,
    DateOnly? DueDate,
    IReadOnlyList<string>? Tags,
    TaskState? Status);
