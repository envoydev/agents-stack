using TaskApi.Features.Tasks.Model;

namespace TaskApi.Features.Tasks.UpdateTask;

/// <summary>
/// The payload accepted when updating a task. A PUT replaces the editable fields wholesale: title,
/// status, and priority are required; description, due date, and tags are optional.
/// </summary>
/// <param name="Title">Short human-readable name; required.</param>
/// <param name="Description">Longer description; defaults to empty when omitted.</param>
/// <param name="Status">Lifecycle state; required.</param>
/// <param name="Priority">Urgency; required.</param>
/// <param name="DueDate">Optional deadline as an ISO date, or null to clear it.</param>
/// <param name="Tags">Optional labels; defaults to an empty list.</param>
public sealed record UpdateTaskRequest(
    string Title,
    string? Description,
    TaskState Status,
    TaskPriority Priority,
    DateOnly? DueDate,
    IReadOnlyList<string>? Tags);
