namespace TaskApi.Features.Tasks.Model;

/// <summary>
/// The wire representation of a task returned by the API. The field names and value encodings match the
/// Angular client's <c>Task</c> model exactly, so the client consumes responses without remapping.
/// </summary>
/// <param name="Id">Server-assigned identifier, serialized as a string.</param>
/// <param name="Title">Short human-readable name of the task.</param>
/// <param name="Description">Longer free-form description; may be empty.</param>
/// <param name="Status">Lifecycle state, serialized as 'todo' | 'active' | 'blocked' | 'done'.</param>
/// <param name="Priority">Urgency, serialized as 'low' | 'medium' | 'high' | 'critical'.</param>
/// <param name="DueDate">Optional deadline as an ISO date ('yyyy-MM-dd'), or null when unset.</param>
/// <param name="CreatedAt">Creation instant as an ISO-8601 timestamp.</param>
/// <param name="UpdatedAt">Last-modified instant as an ISO-8601 timestamp.</param>
/// <param name="Tags">Free-form labels attached to the task.</param>
public sealed record TaskDto(
    Guid Id,
    string Title,
    string Description,
    TaskState Status,
    TaskPriority Priority,
    DateOnly? DueDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<string> Tags);
