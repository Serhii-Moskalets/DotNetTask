namespace DotNetTask.Application.Abstractions.Interfaces.Notifications;

/// <summary>
/// Contains identifiers of email templates used by the template provider.
/// </summary>
/// <remarks>
/// These values correspond to HTML template file names
/// without file extensions.
/// </remarks>
public static class EmailTemplates
{
    /// <summary>
    /// Template name for user email verification.
    /// </summary>
    public const string EmailVerification = "email_verification";

    /// <summary>
    /// Template name for email change confirmation.
    /// </summary>
    public const string EmailChange = "email_change";

    /// <summary>
    /// Template name for email change security alert.
    /// </summary>
    public const string EmailChangeSecurityAlert = "email_change_security_alert";

    /// <summary>
    /// Template name for password reset confirmation.
    /// </summary>
    public const string PasswordReset = "password_reset";

    /// <summary>
    /// Template name for account deletion notification email sent after the account has been deleted.
    /// </summary>
    public const string AccountDeleted = "account_deleted";
}
