namespace TodoListApp.Application.Abstractions.Interfaces.Notifications;

/// <summary>
/// Provides low-level functionality to send emails using the SMTP protocol via MailKit.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Asynchronously sends an email message.
    /// </summary>
    /// <param name="toEmail">The recipient's email address.</param>
    /// <param name="subject">The email subject line.</param>
    /// <param name="body">The HTML body content.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when SMTP delivery fails. Check inner exception for details (auth, network, etc.).
    /// </exception>
    /// <returns>>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task SendAsync(string toEmail, string subject, string body, CancellationToken ct = default);
}
