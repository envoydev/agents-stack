using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using TaskApi.Features.Tasks.CreateTask;
using TaskApi.Features.Tasks.Model;
using TaskApi.Infrastructure.Persistence.Stores;

namespace TaskApi.Tests.Unit;

public sealed class CreateTaskHandlerTests
{
    private readonly ITaskStore _store = Substitute.For<ITaskStore>();
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 7, 8, 9, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task Creates_task_with_clock_timestamp_and_defaults_when_optionals_omitted()
    {
        var request = new CreateTaskRequest("Ship it", Description: null, TaskPriority.High,
            DueDate: null, Tags: null, Status: null);

        var result = await CreateTaskEndpoint.Handle(request, _store, _clock, CancellationToken.None);

        result.Value.Should().NotBeNull();
        result.Value!.Title.Should().Be("Ship it");
        result.Value.Description.Should().BeEmpty();
        result.Value.Status.Should().Be(TaskState.Todo);
        result.Value.Priority.Should().Be(TaskPriority.High);
        result.Value.Tags.Should().BeEmpty();
        result.Value.CreatedAt.Should().Be(_clock.GetUtcNow());
        result.Location.Should().Be($"/api/tasks/{result.Value.Id}");

        // Loose stance: only the persistence boundary is verified; the store's other members stay implicit.
        await _store.Received(1).AddAsync(
            Arg.Is<TaskItem>(t => t.Title == "Ship it" && t.Status == TaskState.Todo),
            Arg.Any<CancellationToken>());
    }
}
