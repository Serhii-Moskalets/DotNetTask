namespace TodoListApp.Domain.Constants.Settings;

/// <summary>
/// Provides authentication-related configuration from appsettings.
/// </summary>
public class AuthSettings
{
    /// <summary>
    /// Gets or sets endpoint for resetting the password.
    /// </summary>
    public string ResetPasswordEndpoint { get; set; } = "/api/auth/reset-password";
}
