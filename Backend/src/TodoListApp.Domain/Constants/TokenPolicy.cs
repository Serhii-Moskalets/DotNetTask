namespace TodoListApp.Domain.Constants;

/// <summary>
/// Provides constants for token-related policies, including validation error messages.
/// </summary>
public static class TokenPolicy
{
    /// <summary>
    /// The error message returned when a required security token is missing.
    /// </summary>
    public const string RequiredMessage = "Token is required.";

    /// <summary>
    /// The error message when the email verification token is invalid or expired.
    /// </summary>
    public const string InvalidEmailVerificationTokenMessage = "Invalid or expired the email verification token.";

    /// <summary>
    /// The error message when the email change token is invalid or expired.
    /// </summary>
    public const string InvalidEmailChangeTokenMessage = "Invalid or expired the email change token.";

    /// <summary>
    /// The error message when the email revert token is invalid or expired.
    /// </summary>
    public const string InvalidEmailRevertTokenMessage = "Invalid or expired the email revert token.";

    /// <summary>
    /// The error message when the password reset token is invalid or expired.
    /// </summary>
    public const string InvalidPasswordResetTokenMessage = "Invalid or expired the password reset token.";

    /// <summary>
    /// Error message used when the metadata for a pending email change is not found.
    /// </summary>
    public const string MissingPendingEmailMessage = "Pending email data is missing.";

    /// <summary>
    /// Error message used when the metadata for the original email (for reverting) is not found.
    /// </summary>
    public const string MissingOriginalEmailMessage = "Original email data is missing.";

    /// <summary>
    /// Error message when the security token is null or empty.
    /// </summary>
    public const string InvalidOrMissingMessage = "Invalid or missing security token.";

    /// <summary>
    /// Error message when the token duration is zero or negative.
    /// </summary>
    public const string PositiveDurationMessage = "Token duration must be positive.";
}