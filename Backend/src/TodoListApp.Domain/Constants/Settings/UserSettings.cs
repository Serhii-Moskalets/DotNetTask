namespace TodoListApp.Domain.Constants.Settings;

/// <summary>
/// Provides user-related configuration from appsettings.
/// </summary>
public class UserSettings
{
    /// <summary>
    /// Gets or sets endpoint for resending the email verification.
    /// </summary>
    public string ResendEmailVerificationEndpoint { get; set; } = "/api/users/resend-email-verification";
}
