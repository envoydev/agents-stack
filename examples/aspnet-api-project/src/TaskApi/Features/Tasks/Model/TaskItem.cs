namespace TaskApi.Features.Tasks.Model;

/// <summary>
/// A task tracked by the API - the persisted entity and the single writable representation of a task.
/// It is never serialized directly; endpoints project it to <see cref="TaskDto"/> before returning it.
/// </summary>
public sealed class TaskItem
{
    // Parameterless constructor used by EF Core to materialize the entity.
    private TaskItem()
    {
    }

    private TaskItem(Guid id, string title, string description, TaskState status, TaskPriority priority,
        DateOnly? dueDate, IEnumerable<string> tags, DateTimeOffset createdAt)
    {
        Id = id;
        Title = title;
        Description = description;
        Status = status;
        Priority = priority;
        DueDate = dueDate;
        Tags = tags.ToList();
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    /// <summary>Server-assigned identifier.</summary>
    public Guid Id { get; private set; }

    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public TaskState Status { get; private set; }
    public TaskPriority Priority { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public List<string> Tags { get; private set; } = [];

    /// <summary>
    /// Creates a new task with the supplied details, stamping it with the given identifier and creation
    /// time. <see cref="UpdatedAt"/> starts equal to <paramref name="createdAt"/>.
    /// </summary>
    /// <param name="title">Short human-readable name.</param>
    /// <param name="description">Longer free-form description; may be empty.</param>
    /// <param name="status">Initial lifecycle state.</param>
    /// <param name="priority">Initial urgency.</param>
    /// <param name="dueDate">Optional deadline, or null when the task has none.</param>
    /// <param name="tags">Free-form labels to attach.</param>
    /// <param name="id">The identifier to assign.</param>
    /// <param name="createdAt">The creation instant.</param>
    /// <returns>A new, unsaved <see cref="TaskItem"/>.</returns>
    public static TaskItem Create(string title, string description, TaskState status, TaskPriority priority,
        DateOnly? dueDate, IEnumerable<string> tags, Guid id, DateTimeOffset createdAt) =>
        new(id, title, description, status, priority, dueDate, tags, createdAt);

    /// <summary>
    /// Overwrites the editable fields of the task and advances <see cref="UpdatedAt"/> to
    /// <paramref name="updatedAt"/>. The identifier and creation time are immutable.
    /// </summary>
    /// <param name="title">New title.</param>
    /// <param name="description">New description.</param>
    /// <param name="status">New lifecycle state.</param>
    /// <param name="priority">New urgency.</param>
    /// <param name="dueDate">New deadline, or null to clear it.</param>
    /// <param name="tags">The full replacement set of labels.</param>
    /// <param name="updatedAt">The instant of this edit.</param>
    public void Update(string title, string description, TaskState status, TaskPriority priority,
        DateOnly? dueDate, IEnumerable<string> tags, DateTimeOffset updatedAt)
    {
        Title = title;
        Description = description;
        Status = status;
        Priority = priority;
        DueDate = dueDate;
        Tags = tags.ToList();
        UpdatedAt = updatedAt;
    }
}
