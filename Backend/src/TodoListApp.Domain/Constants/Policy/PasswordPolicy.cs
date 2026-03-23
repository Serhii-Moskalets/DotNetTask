namespace TodoListApp.Domain.Constants;

/// <summary>
/// Provides constants for defining password policy requirements, including minimum length and character type
/// requirements.
/// </summary>
/// <remarks>
/// This class contains static members that can be used to enforce password complexity rules in
/// applications.
/// The defined constants can be utilized to validate user passwords against specified criteria.
/// </remarks>
public static class PasswordPolicy
{
    /// <summary>
    /// The minimum number of characters required for a valid password.
    /// </summary>
    public const int MinLength = 8;

    /// <summary>
    /// Regex pattern to ensure at least one uppercase letter is present.
    /// </summary>
    public const string UppercaseRegex = "[A-Z]";

    /// <summary>
    /// Regex pattern to ensure at least one lowercase letter is present.
    /// </summary>
    public const string LowercaseRegex = "[a-z]";

    /// <summary>
    /// Regex pattern to ensure at least one numeric digit is present.
    /// </summary>
    public const string NumberRegex = "[0-9]";

    /// <summary>
    /// Regex pattern to ensure at least one special character (!, ?, *, or .) is present.
    /// </summary>
    public const string SpecialCharRegex = @"[\!\?\*\.]";

    /// <summary>
    /// Error message for null or empty password.
    /// </summary>
    public const string EmptyMessage = "Password is required.";

    /// <summary>
    /// Error message when password lacks an uppercase letter.
    /// </summary>
    public const string UppercaseMessage = "Password must contain at least one uppercase letter.";

    /// <summary>
    /// Error message when password lacks a lowercase letter.
    /// </summary>
    public const string LowercaseMessage = "Password must contain at least one lowercase letter.";

    /// <summary>
    /// Error message when password lacks a number.
    /// </summary>
    public const string NumberMessage = "Password must contain at least one number.";

    /// <summary>
    /// Error message when password lacks a special character.
    /// </summary>
    public const string SpecialCharMessage = "Password must contain at least one special character (!?*.).";

    /// <summary>
    /// Error message when the new password is the same as the old one.
    /// </summary>
    public const string SameAsOldMessage = "New password cannot be the same as the old one.";

    /// <summary>
    /// Error message when the provided current password is incorrect.
    /// </summary>
    public const string IncorrectMessage = "Incorrect password.";

    /// <summary>
    /// Error message when password is too short.
    /// </summary>
    public static readonly string TooShortMessage = $"Password must be at least {MinLength} characters long.";
}