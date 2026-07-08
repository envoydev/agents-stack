using FluentAssertions;
using TaskApi.Features.Tasks.Model;

namespace TaskApi.Tests.Unit;

public sealed class TaskItemTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 8, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_sets_fields_and_starts_updatedAt_equal_to_createdAt()
    {
        var id = Guid.NewGuid();

        var task = TaskItem.Create("Write tests", "unit", TaskState.Todo, TaskPriority.High,
            new DateOnly(2026, 8, 1), ["a", "b"], id, Now);

        task.Id.Should().Be(id);
        task.Title.Should().Be("Write tests");
        task.Status.Should().Be(TaskState.Todo);
        task.Priority.Should().Be(TaskPriority.High);
        task.DueDate.Should().Be(new DateOnly(2026, 8, 1));
        task.Tags.Should().Equal("a", "b");
        task.CreatedAt.Should().Be(Now);
        task.UpdatedAt.Should().Be(Now);
    }

    [Fact]
    public void Update_overwrites_fields_and_advances_updatedAt_but_keeps_id_and_createdAt()
    {
        var id = Guid.NewGuid();
        var task = TaskItem.Create("Old", "old", TaskState.Todo, TaskPriority.Low, null, [], id, Now);
        var later = Now.AddHours(3);

        task.Update("New", "new", TaskState.Done, TaskPriority.Critical, new DateOnly(2026, 9, 1), ["x"], later);

        task.Id.Should().Be(id);
        task.CreatedAt.Should().Be(Now);
        task.Title.Should().Be("New");
        task.Status.Should().Be(TaskState.Done);
        task.Priority.Should().Be(TaskPriority.Critical);
        task.DueDate.Should().Be(new DateOnly(2026, 9, 1));
        task.Tags.Should().Equal("x");
        task.UpdatedAt.Should().Be(later);
    }
}
