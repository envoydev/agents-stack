namespace TaskApi.Features.Tasks.GetTaskStats;

/// <summary>
/// Aggregate figures for the dashboard. Field names and shape match the Angular client's <c>TaskStats</c>;
/// the <c>byStatus</c> and <c>byPriority</c> maps are keyed by the same lowercase strings the client uses.
/// </summary>
/// <param name="Total">Total number of tasks.</param>
/// <param name="ByStatus">Task counts keyed by status ('todo', 'active', 'blocked', 'done').</param>
/// <param name="ByPriority">Task counts keyed by priority ('low', 'medium', 'high', 'critical').</param>
/// <param name="Overdue">Number of tasks past their due date and not yet done.</param>
/// <param name="CompletionRate">Fraction 0..1 of tasks that are done.</param>
public sealed record TaskStatsDto(
    int Total,
    IReadOnlyDictionary<string, int> ByStatus,
    IReadOnlyDictionary<string, int> ByPriority,
    int Overdue,
    double CompletionRate);
