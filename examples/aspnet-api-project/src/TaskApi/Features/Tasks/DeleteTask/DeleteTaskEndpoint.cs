using Microsoft.AspNetCore.Http.HttpResults;
using TaskApi.Infrastructure.Persistence.Stores;

namespace TaskApi.Features.Tasks.DeleteTask;

/// <summary>Registers and handles <c>DELETE /api/tasks/{id}</c>.</summary>
internal static class DeleteTaskEndpoint
{
    /// <summary>Maps the delete endpoint onto the tasks route group.</summary>
    /// <param name="app">The tasks route group.</param>
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapDelete("/{id:guid}", Handle)
            .WithName("DeleteTask")
            .WithSummary("Delete a task")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<Results<NoContent, NotFound>> Handle(Guid id, ITaskStore store, CancellationToken ct)
    {
        var removed = await store.RemoveAsync(id, ct);
        return removed ? TypedResults.NoContent() : TypedResults.NotFound();
    }
}
