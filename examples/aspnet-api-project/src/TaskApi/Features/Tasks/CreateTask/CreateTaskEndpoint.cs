using Microsoft.AspNetCore.Http.HttpResults;
using TaskApi.Features.Tasks.Model;
using TaskApi.Infrastructure.Persistence.Stores;
using TaskApi.Infrastructure.Validation;

namespace TaskApi.Features.Tasks.CreateTask;

/// <summary>Registers and handles <c>POST /api/tasks</c>.</summary>
internal static class CreateTaskEndpoint
{
    /// <summary>Maps the create endpoint onto the tasks route group.</summary>
    /// <param name="app">The tasks route group.</param>
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/", Handle)
            .WithName("CreateTask")
            .WithSummary("Create a task")
            .AddEndpointFilter<ValidationFilter<CreateTaskRequest>>()
            .Produces<TaskDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

    internal static async Task<Created<TaskDto>> Handle(
        CreateTaskRequest request,
        ITaskStore store,
        TimeProvider clock,
        CancellationToken ct)
    {
        var task = TaskItem.Create(
            request.Title,
            request.Description ?? string.Empty,
            request.Status ?? TaskState.Todo,
            request.Priority,
            request.DueDate,
            request.Tags ?? [],
            Guid.NewGuid(),
            clock.GetUtcNow());

        await store.AddAsync(task, ct);
        return TypedResults.Created($"/api/tasks/{task.Id}", task.ToDto());
    }
}
