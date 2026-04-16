using System.ComponentModel.DataAnnotations;

namespace DotNetTask.Infrastructure.Notifications.Options;

/// <summary>
/// Represents the configuration settings for the email service.
/// Maps to the "EmailSettings" section in the configuration file.
/// </summary>
public class EmailSettingsOptions
{
    /// <summary>
    /// The name of the section in the configuration file.
    /// </summary>
    public const string SectionName = "EmailSettings";

    /// <summary>
    /// Gets the SMTP server host address (e.g., "smtp.gmail.com").
    /// </summary>
    [Required]
    public string Host { get; init; } = null!;

    /// <summary>
    /// Gets the port number used by the SMTP server (e.g., 587 or 465).
    /// </summary>
    [Range(1, 65535)]
    public int Port { get; init; }

    /// <summary>
    /// Gets the email address that will appear in the "From" field.
    /// </summary>
    [Required]
    [EmailAddress]
    public string FromEmail { get; init; } = null!;

    /// <summary>
    /// Gets the display name for the sender (e.g., "To-Do Application").
    /// </summary>
    [Required]
    public string FromName { get; init; } = null!;

    /// <summary>
    /// Gets the username for SMTP authentication (usually the same as FromEmail).
    /// </summary>
    [Required]
    public string UserName { get; init; } = null!;

    /// <summary>
    /// Gets the password or app-specific password for SMTP authentication.
    /// </summary>
    [Required]
    public string Password { get; init; } = null!;

    /// <summary>
    /// Gets a value indicating whether SSL/TLS encryption should be enabled for the connection.
    /// </summary>
    public bool EnableSsl { get; init; }
}
