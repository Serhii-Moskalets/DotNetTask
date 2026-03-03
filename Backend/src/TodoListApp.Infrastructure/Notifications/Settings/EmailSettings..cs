namespace TodoListApp.Infrastructure.Notifications.Settings;

/// <summary>
/// Represents the configuration settings for the email service.
/// Maps to the "EmailSettings" section in the configuration file.
/// </summary>
public class EmailSettings
{
    /// <summary>
    /// The name of the section in the configuration file.
    /// </summary>
    public const string SectionName = "EmailSettings";

    /// <summary>
    /// Gets the SMTP server host address (e.g., "smtp.gmail.com").
    /// </summary>
    public string Host { get; init; } = string.Empty;

    /// <summary>
    /// Gets the port number used by the SMTP server (e.g., 587 or 465).
    /// </summary>
    public int Port { get; init; }

    /// <summary>
    /// Gets the email address that will appear in the "From" field.
    /// </summary>
    public string FromEmail { get; init; } = string.Empty;

    /// <summary>
    /// Gets the display name for the sender (e.g., "To-Do Application").
    /// </summary>
    public string FromName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the username for SMTP authentication (usually the same as FromEmail).
    /// </summary>
    public string UserName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the password or app-specific password for SMTP authentication.
    /// </summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether SSL/TLS encryption should be enabled for the connection.
    /// </summary>
    public bool EnableSsl { get; init; }
}
