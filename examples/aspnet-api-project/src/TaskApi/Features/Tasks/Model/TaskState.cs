namespace TaskApi.Features.Tasks.Model;

/// <summary>
/// The lifecycle state of a task. The member names map to the exact lowercase strings the Angular
/// client sends and renders ('todo', 'active', 'blocked', 'done') - the API's JSON options serialize
/// the enum with a camelCase naming policy, and the EF configuration persists it as that same string.
/// </summary>
/// <remarks>
/// Named <c>TaskState</c> rather than <c>TaskStatus</c> on purpose: <c>System.Threading.Tasks.TaskStatus</c>
/// is imported globally, so an unqualified <c>TaskStatus</c> in a feature slice would be ambiguous.
/// </remarks>
public enum TaskState
{
    /// <summary>Not started yet.</summary>
    Todo,

    /// <summary>Currently being worked on.</summary>
    Active,

    /// <summary>Cannot progress until an external blocker is cleared.</summary>
    Blocked,

    /// <summary>Finished.</summary>
    Done,
}
