namespace DotNetTask.Application.Abstractions.Interfaces.Notifications;

/// <summary>
/// Provides methods for sending email notifications.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends a confirmation email to a newly registered user.
    /// </summary>
    /// <param name="toEmail">The recipient's email address.</param>
    /// <param name="userName">The name of the user for the greeting.</param>
    /// <param name="confirmLink">The unique verification link.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SendConfirmationEmailAsync(
        string toEmail,
        string userName,
        string confirmLink,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a confirmation email to verify a user's request to change their email address.
    /// </summary>
    /// <param name="toEmail">The recipient's new email address.</param>
    /// <param name="userName">The name of the user for personalization in the email.</param>
    /// <param name="changeLink">The unique link the user must click to confirm the email change.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SendEmailChangeConfirmationAsync(
        string toEmail,
        string userName,
        string changeLink,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a security alert email to inform the user about an email address change request.
    /// </summary>
    /// <param name="toEmail">The user's current (old) email address where the alert will be sent.</param>
    /// <param name="newEmail">The new email address that was requested.</param>
    /// <param name="userName">The name of the user for personalization in the email.</param>
    /// <param name="revertLink">The unique link used to cancel the email change and secure the account.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SendEmailChangeSecurityAlertAsync(
        string toEmail,
        string newEmail,
        string userName,
        string revertLink,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a confirmation email to verify a user's request to reset their password.
    /// </summary>
    /// <param name="toEmail">The recipient's current email address.</param>
    /// <param name="userName">The name of the user for personalization in the email.</param>
    /// <param name="resetLink">The unique link the user must click to confirm the password reset.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SendPasswordResetEmailAsync(
        string toEmail,
        string userName,
        string resetLink,
        CancellationToken cancellationToken = default);
}
