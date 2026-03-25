namespace DotNetTask.Domain.Enums;

/// <summary>
/// Represents the status of a task.
/// </summary>
public enum StatusTask
{
    /// <summary>
    /// Task has not been started yet.
    /// </summary>
    NotStarted = 0,

    /// <summary>
    /// Task is currently in progress.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Task has been completed.
    /// </summary>
    Done = 2,
}
