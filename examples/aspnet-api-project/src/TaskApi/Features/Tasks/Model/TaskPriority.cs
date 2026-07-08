namespace TaskApi.Features.Tasks.Model;

/// <summary>
/// The urgency of a task. The member names map to the exact lowercase strings the Angular client uses
/// ('low', 'medium', 'high', 'critical'); serialized as a camelCase string and persisted as that same
/// string by the EF configuration.
/// </summary>
public enum TaskPriority
{
    /// <summary>Can wait indefinitely.</summary>
    Low,

    /// <summary>Normal urgency.</summary>
    Medium,

    /// <summary>Should be handled soon.</summary>
    High,

    /// <summary>Needs immediate attention.</summary>
    Critical,
}
