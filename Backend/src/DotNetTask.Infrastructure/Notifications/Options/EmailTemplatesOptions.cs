using System.ComponentModel.DataAnnotations;

namespace DotNetTask.Infrastructure.Notifications.Options;

/// <summary>
/// Contains identifiers of email templates used by the template provider.
/// </summary>
/// <remarks>
/// These values correspond to HTML template file names
/// without file extensions.
/// </remarks>
public class EmailTemplatesOptions
{
    /// <summary>
    /// The name of the section in the configuration file.
    /// </summary>
    public const string SectionName = "EmailTemplates";

    /// <summary>
    /// Gets template name for user email verification.
    /// </summary>
    [Required]
    public string EmailVerification { get; init; } = null!;

    /// <summary>
    /// Gets template name for email change confirmation.
    /// </summary>
    [Required]
    public string EmailChange { get; init; } = null!;

    /// <summary>
    /// Gets template name for email change security alert.
    /// </summary>
    [Required]
    public string EmailChangeSecurityAlert { get; init; } = null!;

    /// <summary>
    /// Gets template name for password reset confirmation.
    /// </summary>
    [Required]
    public string PasswordReset { get; init; } = null!;

    /// <summary>
    /// Gets template name for account deletion notification email sent after the account has been deleted.
    /// </summary>
    [Required]
    public string AccountDeleted { get; init; } = null!;
}
