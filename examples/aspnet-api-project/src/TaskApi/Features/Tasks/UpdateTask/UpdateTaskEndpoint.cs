using Microsoft.AspNetCore.Http.HttpResults;
using TaskApi.Features.Tasks.Model;
using TaskApi.Infrastructure.Persistence.Stores;
using TaskApi.Infrastructure.Validation;

namespace TaskApi.Features.Tasks.UpdateTask;

/// <summary>Registers and handles <c>PUT /api/tasks/{id}</c>.</summary>
internal static class UpdateTaskEndpoint
{
    /// <summary>Maps the update endpoint onto the tasks route group.</summary>
    /// <param name="app">The tasks route group.</param>
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/{id:guid}", Handle)
            .WithName("UpdateTask")
            .WithSummary("Update a task")
            .AddEndpointFilter<ValidationFilter<UpdateTaskRequest>>()
            .Produces<TaskDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<Results<Ok<TaskDto>, NotFound>> Handle(
        Guid id,
        UpdateTaskRequest request,
        ITaskStore store,
        TimeProvider clock,
        CancellationToken ct)
    {
        var updated = await store.UpdateAsync(
            id,
            task => task.Update(
                request.Title,
                request.Description ?? string.Empty,
                request.Status,
                request.Priority,
                request.DueDate,
                request.Tags ?? [],
                clock.GetUtcNow()),
            ct);

        if (!updated)
        {
            return TypedResults.NotFound();
        }

        var task = await store.FindAsync(id, ct);
        return TypedResults.Ok(task!.ToDto());
    }
}
