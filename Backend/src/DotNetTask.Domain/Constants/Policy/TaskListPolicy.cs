using DotNetTask.Domain.ValueObjects;

namespace DotNetTask.Domain.Constants;

/// <summary>
/// Provides constants for defining tag policy requirements.
/// </summary>
public static class TaskListPolicy
{
    /// <summary>
    /// The error message used when a unique task list identifier is missing from a request.
    /// </summary>
    public const string IdRequiredMessage = "Task list ID is required.";

    /// <summary>
    /// Error message for null or empty task list title.
    /// </summary>
    public const string EmptyMessage = "Title is required.";

    /// <summary>
    /// The error message when the task list not found or deleted during a general search in a task.
    /// </summary>
    public const string NotFoundMessage = "Task list not found. It may have been deleted.";

    /// <summary>
    /// Error message when the task list title exceeds the maximum allowed length.
    /// </summary>
    public static readonly string TooLongMessage = $"Title cannot exceed {TaskListTitle.MaxLength} characters.";
}
