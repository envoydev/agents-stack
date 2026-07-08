using Microsoft.AspNetCore.Http.HttpResults;
using TaskApi.Features.Tasks.Model;
using TaskApi.Infrastructure.Persistence.Stores;

namespace TaskApi.Features.Tasks.GetTask;

/// <summary>Registers and handles <c>GET /api/tasks/{id}</c>.</summary>
internal static class GetTaskEndpoint
{
    /// <summary>Maps the get-by-id endpoint onto the tasks route group.</summary>
    /// <param name="app">The tasks route group.</param>
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/{id:guid}", Handle)
            .WithName("GetTask")
            .WithSummary("Get a task by id")
            .Produces<TaskDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<Results<Ok<TaskDto>, NotFound>> Handle(Guid id, ITaskStore store, CancellationToken ct)
    {
        var task = await store.FindAsync(id, ct);
        return task is null ? TypedResults.NotFound() : TypedResults.Ok(task.ToDto());
    }
}
