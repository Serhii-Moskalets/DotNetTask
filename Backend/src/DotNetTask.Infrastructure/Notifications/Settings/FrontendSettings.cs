namespace DotNetTask.Infrastructure.Notifications.Settings;

/// <summary>
/// Represents the configuration settings for the frontend application.
/// </summary>
public class FrontendSettings
{
    /// <summary>
    /// The name of the section in the configuration file.
    /// </summary>
    public const string SectionName = "FrontendSettings";

    /// <summary>
    /// Gets the base URL address for the frontend application.
    /// </summary>
    public string BaseUrl { get; init; } = string.Empty;

    /// <summary>
    /// Gets the relative path used for email confirmation requests.
    /// </summary>
    public string ConfirmEmailPath { get; init; } = "confirm-email";

    /// <summary>
    /// Gets the relative path used for email change confirmation requests.
    /// </summary>
    public string EmailChangeConfirmationPath { get; init; } = "confirm-email-change";

    /// <summary>
    /// Gets the relative path used for password resetting requests.
    /// </summary>
    public string PasswordResetPath { get; init; } = "reset-password";

    /// <summary>
    /// Gets the relative path used for revert email changing requests.
    /// </summary>
    public string RevertEmailChangePath { get; init; } = "revert-email-change";
}
