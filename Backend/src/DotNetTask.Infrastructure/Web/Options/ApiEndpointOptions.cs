namespace DotNetTask.Infrastructure.Web.Options;

/// <summary>
/// Represents the configuration options for various API endpoints used within the application.
/// </summary>
public class ApiEndpointOptions
{
    /// <summary>
    /// The name of the configuration section in the settings file (e.g., appsettings.json).
    /// </summary>
    public const string SectionName = "ApiEndpoints";

    /// <summary>
    /// Gets or sets the relative or absolute URL for the email verification resend endpoint.
    /// </summary>
    public string ResendEmailVerificationEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the relative or absolute URL for the password reset endpoint.
    /// </summary>
    public string ResetPasswordEndpoint { get; set; } = string.Empty;
}
