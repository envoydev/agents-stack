using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using TaskApi.Features.Tasks.Model;

namespace TaskApi.Tests.Integration;

public sealed class TasksApiTests : IClassFixture<TaskApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    private readonly HttpClient _client;

    public TasksApiTests(TaskApiFactory factory)
    {
        factory.CreateSchema();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_then_get_round_trips_the_task_in_the_angular_contract_shape()
    {
        var create = await _client.PostAsJsonAsync("/api/tasks", new
        {
            title = "Write integration tests",
            description = "cover the endpoints",
            priority = "high",
            dueDate = "2026-08-01",
            tags = new[] { "test" },
        }, Json);

        create.StatusCode.Should().Be(HttpStatusCode.Created);
        create.Headers.Location.Should().NotBeNull();

        var created = await create.Content.ReadFromJsonAsync<TaskDto>(Json);
        created.Should().NotBeNull();
        created!.Status.Should().Be(TaskState.Todo);
        created.Priority.Should().Be(TaskPriority.High);

        var get = await _client.GetAsync($"/api/tasks/{created.Id}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);

        var fetched = await get.Content.ReadFromJsonAsync<TaskDto>(Json);
        fetched!.Title.Should().Be("Write integration tests");
        fetched.DueDate.Should().Be(new DateOnly(2026, 8, 1));
        fetched.Tags.Should().Equal("test");
    }

    [Fact]
    public async Task Post_with_blank_title_returns_400_problem_details()
    {
        var response = await _client.PostAsJsonAsync("/api/tasks", new { title = "", priority = "low" }, Json);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task Get_unknown_id_returns_404()
    {
        var response = await _client.GetAsync($"/api/tasks/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_changes_fields_then_delete_removes_the_task()
    {
        var created = await (await _client.PostAsJsonAsync("/api/tasks",
            new { title = "temp", priority = "low" }, Json)).Content.ReadFromJsonAsync<TaskDto>(Json);

        var update = await _client.PutAsJsonAsync($"/api/tasks/{created!.Id}", new
        {
            title = "renamed",
            status = "done",
            priority = "critical",
            tags = new[] { "x" },
        }, Json);
        update.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await update.Content.ReadFromJsonAsync<TaskDto>(Json);
        updated!.Title.Should().Be("renamed");
        updated.Status.Should().Be(TaskState.Done);
        updated.Priority.Should().Be(TaskPriority.Critical);

        var delete = await _client.DeleteAsync($"/api/tasks/{created.Id}");
        delete.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getAfter = await _client.GetAsync($"/api/tasks/{created.Id}");
        getAfter.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Stats_returns_all_status_keys_and_a_valid_completion_rate()
    {
        await _client.PostAsJsonAsync("/api/tasks", new { title = "s1", priority = "low" }, Json);
        await _client.PostAsJsonAsync("/api/tasks", new { title = "s2", priority = "high" }, Json);

        var response = await _client.GetAsync("/api/tasks/stats");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var stats = await response.Content.ReadFromJsonAsync<StatsShape>(Json);
        stats!.Total.Should().BeGreaterThanOrEqualTo(2);
        stats.ByStatus.Should().ContainKeys("todo", "active", "blocked", "done");
        stats.CompletionRate.Should().BeInRange(0, 1);
    }

    private sealed record StatsShape(
        int Total,
        Dictionary<string, int> ByStatus,
        Dictionary<string, int> ByPriority,
        int Overdue,
        double CompletionRate);
}
