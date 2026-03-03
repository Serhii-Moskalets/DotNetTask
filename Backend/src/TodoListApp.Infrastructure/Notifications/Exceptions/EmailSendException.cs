namespace TodoListApp.Infrastructure.Notifications.Exceptions;

/// <summary>
/// Represents an error that occurs when sending an email fails.
/// </summary>
public class EmailSendException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmailSendException"/> class.
    /// </summary>
    public EmailSendException()
        : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailSendException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public EmailSendException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailSendException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public EmailSendException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailSendException"/> class.
    /// </summary>
    /// <param name="recipientEmail">The email address of the intended recipient.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public EmailSendException(string recipientEmail, string message, Exception innerException)
        : base(message, innerException)
    {
        this.RecipientEmail = recipientEmail;
    }

    /// <summary>
    /// Gets email address of the recipient for which sending failed.
    /// </summary>
    public string? RecipientEmail { get; }
}
