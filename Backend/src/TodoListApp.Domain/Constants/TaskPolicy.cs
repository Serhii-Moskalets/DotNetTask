namespace TodoListApp.Domain.Constants;

/// <summary>
/// Provides constants for defining task policy requirements.
/// </summary>
public static class TaskPolicy
{
    /// <summary>
    /// The error message used when a unique task identifier is missing from a request.
    /// </summary>
    public const string IdRequiredMessage = "Task ID is required.";

    /// <summary>
    /// The error message when the task not found or deleted during a general search in a task.
    /// </summary>
    public const string TaskNotFoundMessage = "Task not found. It may have been deleted.";

    /// <summary>
    /// The error message when a user attempts to access or modify a task they do not own or have permission for.
    /// </summary>
    public const string AccessDeniedMessage = "You don't have access to this task.";
}