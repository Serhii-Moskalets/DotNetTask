namespace DotNetTask.Application.Abstractions.Interfaces.Notifications;

/// <summary>
/// Contains predefined email subject lines used for notification emails.
/// </summary>
/// <remarks>
/// These constants define the visible subject text that appears
/// in the user's email client.
/// </remarks>
public static class EmailSubjects
{
    /// <summary>
    /// Subject line for user registration confirmation emails.
    /// </summary>
    public const string RegistrationConfirmation = "Registration confirmation";

    /// <summary>
    /// Subject line for email change confirmation emails.
    /// </summary>
    public const string EmailChangeConfirmation = "Confirm your new email address";

    /// <summary>
    /// Subject line for email change security alert.
    /// </summary>
    public const string EmailChangeSecurityAlert = "Security Alert: Email Change Requested";

    /// <summary>
    /// Subject line for password reset emails.
    /// </summary>
    public const string PasswordReset = "Reset your password";
}
