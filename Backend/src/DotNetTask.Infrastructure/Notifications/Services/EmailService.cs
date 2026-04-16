using DotNetTask.Application.Abstractions.Interfaces.Notifications;
using DotNetTask.Infrastructure.Notifications.Options;
using Microsoft.Extensions.Options;

namespace DotNetTask.Infrastructure.Notifications.Services;

/// <summary>
/// High-level service that coordinates email template processing and delivery.
/// </summary>
/// <param name="sender">The SMTP sender implementation.</param>
/// <param name="templateProvider">The template engine for loading HTML files.</param>
/// <param name="emailSubjectsOptions">Configuration containing predefined subjects for various system notifications.</param>
/// <param name="emailTemplatesOptions">Configuration containing file paths or keys for email templates.</param>
public class EmailService
    (IEmailSender sender,
    IEmailTemplateProvider templateProvider,
    IOptions<EmailSubjectsOptions> emailSubjectsOptions,
    IOptions<EmailTemplatesOptions> emailTemplatesOptions) : IEmailService
{

    private readonly EmailSubjectsOptions _subjects = emailSubjectsOptions.Value;
    private readonly EmailTemplatesOptions _templates = emailTemplatesOptions.Value;

    /// <summary>
    /// Prepares and sends a registration confirmation email to a new user.
    /// </summary>
    /// <param name="toEmail">The recipient's email address.</param>
    /// <param name="userName">The name of the user to be used in the greeting.</param>
    /// <param name="confirmLink">The URL for email verification.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task SendConfirmationEmailAsync(
        string toEmail,
        string userName,
        string confirmLink,
        CancellationToken cancellationToken = default)
    {
        Dictionary<string, string> placeholders = new()
        {
            { "USER_NAME", userName },
            { "VERIFY_LINK", confirmLink },
        };

        return this.SendEmailAsync(
            toEmail,
            this._subjects.RegistrationConfirmation,
            this._templates.EmailVerification,
            placeholders,
            cancellationToken);
    }

    /// <summary>
    /// Sends a confirmation email to verify a user's request to change their email address.
    /// </summary>
    /// <param name="toEmail">The recipient's new email address.</param>
    /// <param name="userName">The name of the user for personalization in the email.</param>
    /// <param name="changeLink">The unique link the user must click to confirm the email change.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task SendEmailChangeConfirmationAsync(
        string toEmail,
        string userName,
        string changeLink,
        CancellationToken cancellationToken = default)
    {
        Dictionary<string, string> placeholders = new()
        {
            { "USER_NAME", userName },
            { "CHANGE_LINK", changeLink },
        };

        return this.SendEmailAsync(
            toEmail,
            this._subjects.EmailChangeConfirmation,
            this._templates.EmailChange,
            placeholders,
            cancellationToken);
    }

    /// <summary>
    /// Sends a security alert email to inform the user about an email address change request.
    /// </summary>
    /// <param name="toEmail">The user's current (old) email address where the alert will be sent.</param>
    /// <param name="newEmail">The new email address that was requested.</param>
    /// <param name="userName">The name of the user for personalization in the email.</param>
    /// <param name="revertLink">The unique link used to cancel the email change and secure the account.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task SendEmailChangeSecurityAlertAsync(
        string toEmail,
        string newEmail,
        string userName,
        string revertLink,
        CancellationToken cancellationToken = default)
    {
        Dictionary<string, string> placeholders = new()
        {
            { "USER_NAME", userName },
            { "NEW_EMAIL", newEmail },
            { "REVERT_LINK", revertLink },
        };

        return this.SendEmailAsync(
            toEmail,
            this._subjects.EmailChangeSecurityAlert,
            this._templates.EmailChangeSecurityAlert,
            placeholders,
            cancellationToken);
    }

    /// <summary>
    /// Sends a confirmation email to verify a user's request to reset their password.
    /// </summary>
    /// <param name="toEmail">The recipient's current email address.</param>
    /// <param name="userName">The name of the user for personalization in the email.</param>
    /// <param name="resetLink">The unique link the user must click to confirm the password reset.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task SendPasswordResetEmailAsync(
        string toEmail,
        string userName,
        string resetLink,
        CancellationToken cancellationToken = default)
    {
        Dictionary<string, string> placeholders = new()
        {
            { "USER_NAME", userName },
            { "RESET_LINK", resetLink },
        };

        return this.SendEmailAsync(
            toEmail,
            this._subjects.PasswordReset,
            this._templates.PasswordReset,
            placeholders,
            cancellationToken);
    }

    /// <summary>
    /// Sends a notification email informing the user that their account has been successfully deleted.
    /// </summary>
    /// <param name="toEmail">The recipient's email address.</param>
    /// <param name="userName">The name of the user for personalization in the email.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task SendAccountDeletedEmailAsync(
        string toEmail,
        string userName,
        CancellationToken cancellationToken = default)
    {
        Dictionary<string, string> placeholders = new()
        {
            { "USER_NAME", userName },
        };

        return this.SendEmailAsync(
            toEmail,
            this._subjects.AccountDeleted,
            this._templates.AccountDeleted,
            placeholders,
            cancellationToken);
    }

    private async Task SendEmailAsync(
        string toEmail,
        string subject,
        string templateName,
        Dictionary<string, string> placeholders,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(toEmail);
        ArgumentException.ThrowIfNullOrEmpty(subject);
        ArgumentException.ThrowIfNullOrEmpty(templateName);

        string body = await templateProvider.GetEmailTemplateAsync(templateName, placeholders);

        await sender.SendAsync(
            toEmail,
            subject,
            body,
            cancellationToken);
    }
}
