namespace TaskApi.Features.Tasks.Model;

/// <summary>
/// Projects the persisted <see cref="TaskItem"/> entity onto its wire <see cref="TaskDto"/>.
/// </summary>
internal static class TaskMapping
{
    /// <summary>
    /// Maps a single task entity to the DTO returned to clients, snapshotting its tags so the caller
    /// cannot mutate the entity's backing collection through the DTO.
    /// </summary>
    /// <param name="task">The entity to project.</param>
    /// <returns>A <see cref="TaskDto"/> carrying the entity's current values.</returns>
    public static TaskDto ToDto(this TaskItem task) => new(
        task.Id,
        task.Title,
        task.Description,
        task.Status,
        task.Priority,
        task.DueDate,
        task.CreatedAt,
        task.UpdatedAt,
        [.. task.Tags]);
}
