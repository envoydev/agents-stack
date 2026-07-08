using Microsoft.AspNetCore.Http.HttpResults;
using TaskApi.Features.Tasks.Model;
using TaskApi.Infrastructure.Persistence.Stores;

namespace TaskApi.Features.Tasks.ListTasks;

/// <summary>Registers and handles <c>GET /api/tasks</c>, returning all tasks newest-first.</summary>
internal static class ListTasksEndpoint
{
    /// <summary>Maps the list endpoint onto the tasks route group.</summary>
    /// <param name="app">The tasks route group.</param>
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/", Handle)
            .WithName("ListTasks")
            .WithSummary("List tasks")
            .WithDescription("Returns all tasks, most recently created first.")
            .Produces<IReadOnlyList<TaskDto>>();

    private static async Task<Ok<IReadOnlyList<TaskDto>>> Handle(ITaskStore store, CancellationToken ct)
    {
        var tasks = await store.ListAsync(ct);
        IReadOnlyList<TaskDto> dtos = tasks.Select(t => t.ToDto()).ToArray();
        return TypedResults.Ok(dtos);
    }
}
