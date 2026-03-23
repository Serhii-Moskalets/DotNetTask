using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Domain.Constants;

/// <summary>
/// Provides constants for defining tag policy requirements.
/// </summary>
public static class TagPolicy
{
    /// <summary>
    /// The error message used when a unique tag identifier is missing from a request.
    /// </summary>
    public const string IdRequiredMessage = "Tag ID is required.";

    /// <summary>
    /// Error message for null or empty tag name.
    /// </summary>
    public const string EmptyMessage = "Tag name is required.";

    /// <summary>
    /// The error message when the tag not found or deleted during a general search in a task.
    /// </summary>
    public const string NotFoundMessage = "Tag not found. It may have been deleted.";

    /// <summary>
    /// The error message when the user doesn't have permission to this tag.
    /// </summary>
    public const string DoNotHavePermission = "You don't have permission to this tag.";

    /// <summary>
    /// Error message when the tag name exceeds the maximum allowed length.
    /// </summary>
    public static readonly string TooLongMessage = $"Tag name cannot exceed {TagName.MaxLength} characters.";
}
