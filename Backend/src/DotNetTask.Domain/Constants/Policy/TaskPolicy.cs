using DotNetTask.Domain.ValueObjects;

namespace DotNetTask.Domain.Constants;

/// <summary>
/// Provides constants for defining task policy requirements.
/// </summary>
public static class TaskPolicy
{
    /// <summary>
    /// Error message used when a unique task identifier is missing from a request.
    /// </summary>
    public const string IdRequiredMessage = "Task ID is required.";

    /// <summary>
    /// Error message for null or empty task title.
    /// </summary>
    public const string EmptyTitleMessage = "Title is required.";

    /// <summary>
    /// Error message for null or empty task description.
    /// </summary>
    public const string EmptyDescriptionMessage = "Description is required.";

    /// <summary>
    /// Error message for null or empty collection of task IDs when performing bulk operations.
    /// </summary>
    public const string EmptyTaskIdsCollectionMessage = "TaskIds collection is required.";

    /// <summary>
    /// Error message when the task not found or deleted during a general search in a task.
    /// </summary>
    public const string NotFoundMessage = "Task not found. It may have been deleted.";

    /// <summary>
    /// Error message when a user attempts to access or modify a task they do not own or have permission for.
    /// </summary>
    public const string AccessDeniedMessage = "You don't have access to this task.";

    /// <summary>
    /// Error message when the due date is set in the past.
    /// </summary>
    public const string InvalidDueDateMessage = "Due date cannot be in the past.";

    /// <summary>
    /// Error message when the start date of a range is after the end date.
    /// </summary>
    public const string InvalidDateRangeMessage = "DueAfter must be before or equal to DueBefore.";

    /// <summary>
    /// Error when emum is invalid.
    /// </summary>
    public const string InvalidTaskSatusMessage = "Invalid task status.";

    /// <summary>
    /// Error message when moving from Done back to NotStarted.
    /// </summary>
    public const string DoneToNotStartedMessage = "Cannot move from Done to NotStarted.";

    /// <summary>
    /// Error message when moving from InProgress back to NotStarted.
    /// </summary>
    public const string InProgressToNotStartedMessage = "Cannot move from InProgress to NotStarted.";

    /// <summary>
    /// Error message when completing a task that hasn't been started.
    /// </summary>
    public const string CompletionRequiresInProgressMessage = "Task must be InProgress to complete.";

    /// <summary>
    /// Error message when the provided collection of task IDs contains duplicate values.
    /// </summary>
    public const string DuplicateTaskIdsMessage = "TaskIds collection cannot contain duplicate values.";

    /// <summary>
    /// Error message when the provided task status is invalid.
    /// </summary>
    public const string InvalidStatusMessage = "The provided task status is invalid.";

    /// <summary>
    /// Error message when the task title exceeds the maximum allowed length.
    /// </summary>
    public static readonly string TooLongTitleMessage = $"Title cannot exceed {TaskTitle.MaxLength} characters.";

    /// <summary>
    /// Error message when the task title exceeds the maximum allowed length.
    /// </summary>
    public static readonly string TooLongDescriptionMessage = $"Description cannot exceed {TaskDescription.MaxLength} characters.";
}
