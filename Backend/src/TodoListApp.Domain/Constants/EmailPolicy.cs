using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Domain.Constants;

/// <summary>
/// Provides constants for defining email policy requirements.
/// </summary>
public static class EmailPolicy
{
    /// <summary>Standard email validation regex pattern.</summary>
    public const string FormatRegex = @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$";

    /// <summary>Regex pattern to ensure email does not contain angle brackets.</summary>
    public const string NoAngleBracketsRegex = "^[^<>]+$";

    /// <summary>Error message for null or empty email.</summary>
    public const string EmptyMessage = "Email is required.";

    /// <summary>Error message for incorrect email format.</summary>
    public const string InvalidFormatMessage = "Email address is incorrect.";

    /// <summary>Error message when the new email is the same as the current one.</summary>
    public const string SameAsCurrentMessage = "New email is same as current.";

    /// <summary>Error message when the email is already associated with another account.</summary>
    public const string AlreadyInUseMessage = "This email is already in use.";

    /// <summary>Error message when the email has not been confirmed.</summary>
    public const string NotConfirmedMessage = "Please confirm your email before logging in.";

    /// <summary>Error message when the email exceeds the maximum allowed length.</summary>
    public static readonly string TooLongMessage = $"Email cannot be longer than {Email.MaxLength} characters.";
}
