using System.ComponentModel.DataAnnotations;

namespace DotNetTask.Infrastructure.Notifications.Options;

/// <summary>
/// Represents the configuration settings for the frontend application.
/// </summary>
public class FrontendSettingsOptions
{
    /// <summary>
    /// The name of the section in the configuration file.
    /// </summary>
    public const string SectionName = "FrontendSettings";

    /// <summary>
    /// Gets the base URL address for the frontend application.
    /// </summary>
    [Required]
    public string BaseUrl { get; init; } = null!;

    /// <summary>
    /// Gets the relative path used for email confirmation requests.
    /// </summary>
    [Required]
    public string EmailConfirmationPath { get; init; } = null!;

    /// <summary>
    /// Gets the relative path used for email change confirmation requests.
    /// </summary>
    [Required]
    public string EmailChangeConfirmationPath { get; init; } = null!;

    /// <summary>
    /// Gets the relative path used for password resetting requests.
    /// </summary>
    [Required]
    public string PasswordResetPath { get; init; } = null!;

    /// <summary>
    /// Gets the relative path used for revert email changing requests.
    /// </summary>
    [Required]
    public string EmailRevertPath { get; init; } = null!;
}
