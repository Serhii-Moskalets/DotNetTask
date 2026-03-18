using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Domain.Constants;

/// <summary>
/// Provides constants for defining email policy requirements.
/// </summary>
public static class CommentPolicy
{
    /// <summary>
    /// The error message used when a unique comment identifier is missing from a request.
    /// </summary>
    public const string CommentIdRequiredMessage = "Comment ID is required.";

    /// <summary>
    /// The error message when a comment update is attempted but the content is identical to the current one.
    /// </summary>
    public const string NoChangesDetectedMessage = "No changes detected. The comment content is the same as the current one.";

    /// <summary>
    /// Error message for null or empty comment content.
    /// </summary>
    public const string EmptyMessage = "Comment content is required.";

    /// <summary>
    /// The error message when the comment not found or deleted during a general search in a task.
    /// </summary>
    public const string CommentNotFoundMessage = "Comment not found. It may have been deleted.";

    /// <summary>
    /// The error message when a user attempts to delete a comment they do not have permission for.
    /// </summary>
    public const string DeleteAccessDeniedMessage = "You don't have permission to delete this comment.";

    /// <summary>
    /// The error message when a user attempts to modify a comment they do not have permission for.
    /// </summary>
    public const string UpdateAccessDeniedMessage = "You don't have permission to update this comment.";

    /// <summary>
    /// Error message when the comment content exceeds the maximum allowed length.
    /// </summary>
    public static readonly string TooLongMessage = $"Comment content cannot exceed {CommentContent.MaxLength} characters.";
}