using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using TaskApi.Features.Tasks.Model;
using TaskApi.Infrastructure.Persistence.Stores;

namespace TaskApi.Features.Tasks.GetTaskStats;

/// <summary>Registers and handles <c>GET /api/tasks/stats</c>, the dashboard aggregate.</summary>
internal static class GetTaskStatsEndpoint
{
    /// <summary>Maps the stats endpoint onto the tasks route group.</summary>
    /// <param name="app">The tasks route group.</param>
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/stats", Handle)
            .WithName("GetTaskStats")
            .WithSummary("Task statistics")
            .Produces<TaskStatsDto>();

    internal static async Task<Ok<TaskStatsDto>> Handle(ITaskStore store, TimeProvider clock, CancellationToken ct)
    {
        var tasks = await store.ListAsync(ct);
        var today = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);

        var byStatus = Enum.GetValues<TaskState>()
            .ToDictionary(s => CamelCaseName(s), s => tasks.Count(t => t.Status == s));
        var byPriority = Enum.GetValues<TaskPriority>()
            .ToDictionary(p => CamelCaseName(p), p => tasks.Count(t => t.Priority == p));

        var total = tasks.Count;
        var done = tasks.Count(t => t.Status == TaskState.Done);
        var overdue = tasks.Count(t => t.DueDate is { } due && due < today && t.Status != TaskState.Done);
        var completionRate = total == 0 ? 0d : (double)done / total;

        var stats = new TaskStatsDto(total, byStatus, byPriority, overdue, completionRate);
        return TypedResults.Ok(stats);
    }

    private static string CamelCaseName<TEnum>(TEnum value)
        where TEnum : struct, Enum =>
        JsonNamingPolicy.CamelCase.ConvertName(value.ToString());
}
