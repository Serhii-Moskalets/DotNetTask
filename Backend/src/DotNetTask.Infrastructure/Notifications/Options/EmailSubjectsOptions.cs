using System.ComponentModel.DataAnnotations;

namespace DotNetTask.Infrastructure.Notifications.Options;

/// <summary>
/// Contains predefined email subject lines used for notification emails.
/// </summary>
/// <remarks>
/// These constants define the visible subject text that appears
/// in the user's email client.
/// </remarks>
public class EmailSubjectsOptions
{
    /// <summary>
    /// The name of the section in the configuration file.
    /// </summary>
    public const string SectionName = "EmailSubjects";

    /// <summary>
    /// Gets subject line for user registration confirmation emails.
    /// </summary>
    [Required]
    public string RegistrationConfirmation { get; init; } = null!;

    /// <summary>
    /// Gets subject line for email change confirmation emails.
    /// </summary>
    [Required]
    public string EmailChangeConfirmation { get; init; } = null!;

    /// <summary>
    /// Gets subject line for email change security alert.
    /// </summary>
    [Required]
    public string EmailChangeSecurityAlert { get; init; } = null!;

    /// <summary>
    /// Gets subject line for password reset emails.
    /// </summary>
    [Required]
    public string PasswordReset { get; init; } = null!;

    /// <summary>
    /// Gets subject line for account deletion notification emails.
    /// </summary>
    [Required]
    public string AccountDeleted { get; init; } = null!;
}
