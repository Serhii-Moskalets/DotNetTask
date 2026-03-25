using DotNetTask.Domain.ValueObjects;

namespace DotNetTask.Domain.Constants;

/// <summary>
/// Provides constants for defining user name policy requirements.
/// </summary>
public static class UserNamePolicy
{
    /// <summary>
    /// Regex pattern for valid username: letters, numbers, and underscores.
    /// </summary>
    public const string FormatRegex = "^[a-zA-Z0-9_]+$";

    /// <summary>
    /// Error message for null or empty username.
    /// </summary>
    public const string EmptyMessage = "Username is required.";

    /// <summary>
    /// Error message when the new username is the same as the current one.
    /// </summary>
    public const string SameAsCurrentMessage = "New username cannot be the same as the current one.";

    /// <summary>
    /// Error message when the username contains invalid characters.
    /// </summary>
    public const string InvalidCharactersMessage = "User name can only contain letters, numbers, and underscores.";

    /// <summary>
    /// Error message when the username is already associated with another account.
    /// </summary>
    public const string AlreadyInUseMessage = "User name is already exists.";

    /// <summary>
    /// Error message when the username length is outside the permitted range.
    /// </summary>
    public static readonly string LengthMessage =
        $"Username must be between {UserName.MinLength} and {UserName.MaxLength} characters.";
}
