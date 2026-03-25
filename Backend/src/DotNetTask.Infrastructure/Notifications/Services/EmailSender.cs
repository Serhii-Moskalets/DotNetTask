using DotNetTask.Application.Abstractions.Interfaces.Notifications;
using DotNetTask.Domain.Constants;
using DotNetTask.Infrastructure.Notifications.Exceptions;
using DotNetTask.Infrastructure.Notifications.Settings;

using MailKit.Net.Smtp;
using MailKit.Security;

using Microsoft.Extensions.Options;

using MimeKit;

namespace DotNetTask.Infrastructure.Notifications.Services;

/// <summary>
/// Provides low-level functionality to send emails using the SMTP protocol via MailKit.
/// </summary>
public class EmailSender(IOptions<EmailSettings> settings) : IEmailSender
{
    private readonly EmailSettings _settings = settings.Value;

    /// <summary>
    /// Asynchronously sends an email message.
    /// </summary>
    /// <param name="toEmail">The recipient's email address.</param>
    /// <param name="subject">The email subject line.</param>
    /// <param name="body">The HTML body content.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="EmailSendException">
    /// Thrown when SMTP delivery fails. Check inner exception for details (auth, network, etc.).
    /// </exception>
    /// /<exception cref="ArgumentException">
    /// Thrown when <paramref name="toEmail"/> is null, empty, or invalid.
    /// </exception>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SendAsync(string toEmail, string subject, string body, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            throw new ArgumentException(EmailPolicy.EmptyMessage, nameof(toEmail));
        }

        if (!MailboxAddress.TryParse(toEmail, out _))
        {
            throw new ArgumentException(EmailPolicy.InvalidFormatMessage, nameof(toEmail));
        }

        try
        {
            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress(this._settings.FromName, this._settings.FromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = body }.ToMessageBody();

            using SmtpClient client = new SmtpClient();

            client.Timeout = 10000;

            await client.ConnectAsync(this._settings.Host, this._settings.Port, SecureSocketOptions.Auto, ct);
            await client.AuthenticateAsync(this._settings.UserName, this._settings.Password, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
        }
        catch (Exception ex)
        {
            throw new EmailSendException(toEmail, string.Format(EmailPolicy.SendEmailFailedMessage, toEmail), ex);
        }
    }
}
