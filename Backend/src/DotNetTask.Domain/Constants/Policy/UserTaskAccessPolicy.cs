namespace DotNetTask.Domain.Constants;

/// <summary>
/// Provides constants for defining user task access policy requirements.
/// </summary>
public static class UserTaskAccessPolicy
{
    /// <summary>
    /// The error message when the provided user entity does not match the specified owner identifier.
    /// </summary>
    public const string OwnerIdRequired = "OwnerId is required.";

    /// <summary>
    /// The error message when attempting to remove access for the task owner.
    /// </summary>
    public const string OwnerAccessRemovalMessage = "Cannot remove access for the owner of the task.";

    /// <summary>
    /// The error message returned when a user tries to modify a task they do not own.
    /// </summary>
    public const string AccessDeniedMessage = "You do not have permission to manage access for this task.";

    /// <summary>
    /// The error message when the task access entry is not found.
    /// </summary>
    public const string AccessNotFoundMessage = "The specified user-task access entry was not found.";

    /// <summary>
    /// The error message returned when a user with the specified email is not found.
    /// </summary>
    public const string UserNotFoundMessage = "No user found with the provided email address.";

    /// <summary>
    /// The error message returned when a task access entry could not be removed.
    /// </summary>
    public const string DeleteFailedMessage = "Failed to remove access. Please try again.";

    /// <summary>
    /// The error message returned when an unexpected operation error occurs.
    /// </summary>
    public const string OperationFailedMessage = "Something went wrong while processing your request.";

    /// <summary>
    /// The error message returned when no shared accesses exist for the specified task.
    /// </summary>
    public const string NoAccessesFoundMessage = "This task has no shared users to remove.";

    /// <summary>
    /// The error message returned when a user has no shared tasks.
    /// </summary>
    public const string NoUserAccessesMessage = "You don’t have any shared tasks.";

    /// <summary>
    /// The error message returned when a task is not found or the user does not have access to it.
    /// </summary>
    public const string TaskNotFoundOrAccessDeniedMessage = "The task was not found or you do not have access to it.";

    /// <summary>
    /// The error message when attemptiong to share a task with someone who already has access to it.
    /// </summary>
    public const string AlreadySharedMessage = "Task already shared with this user.";

    /// <summary>
    /// The error message when attempting to share a task with its owner.
    /// </summary>
    public const string CannotShareWithOwnerMessage = "Task cannot be shared with ith owner.";
}
