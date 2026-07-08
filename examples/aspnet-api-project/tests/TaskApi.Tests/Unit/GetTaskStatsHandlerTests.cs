using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using TaskApi.Features.Tasks.GetTaskStats;
using TaskApi.Features.Tasks.Model;
using TaskApi.Infrastructure.Persistence.Stores;

namespace TaskApi.Tests.Unit;

public sealed class GetTaskStatsHandlerTests
{
    private readonly ITaskStore _store = Substitute.For<ITaskStore>();
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 7, 8, 0, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task Computes_totals_completion_rate_and_overdue()
    {
        var today = new DateOnly(2026, 7, 8);
        IReadOnlyList<TaskItem> tasks =
        [
            Item(TaskState.Done, TaskPriority.Low, dueDate: null),
            Item(TaskState.Done, TaskPriority.High, dueDate: null),
            Item(TaskState.Todo, TaskPriority.High, dueDate: today.AddDays(-1)),   // past + not done -> overdue
            Item(TaskState.Active, TaskPriority.Critical, dueDate: today.AddDays(3)),
        ];
        _store.ListAsync(Arg.Any<CancellationToken>()).Returns(tasks);

        var result = await GetTaskStatsEndpoint.Handle(_store, _clock, CancellationToken.None);
        var stats = result.Value!;

        stats.Total.Should().Be(4);
        stats.CompletionRate.Should().Be(0.5);
        stats.Overdue.Should().Be(1);
        stats.ByStatus["done"].Should().Be(2);
        stats.ByStatus["todo"].Should().Be(1);
        stats.ByPriority["high"].Should().Be(2);
        stats.ByStatus.Should().ContainKeys("todo", "active", "blocked", "done");
    }

    private static TaskItem Item(TaskState status, TaskPriority priority, DateOnly? dueDate) =>
        TaskItem.Create("t", "d", status, priority, dueDate, [], Guid.NewGuid(),
            new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero));
}
