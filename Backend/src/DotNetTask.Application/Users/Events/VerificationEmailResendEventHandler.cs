using DotNetTask.Application.Abstractions.Interfaces.Common;
using DotNetTask.Application.Abstractions.Interfaces.Notifications;
using DotNetTask.Domain.Events;
using MediatR;

namespace DotNetTask.Application.Users.Events;

/// <summary>
/// Handles the <see cref="VerificationEmailResendEvent"/> by resending a verification email.
/// </summary>
public class VerificationEmailResendEventHandler(
    IEmailService emailService,
    IUrlProvider urlProvider)
    : INotificationHandler<VerificationEmailResendEvent>
{
    private readonly IEmailService _emailService = emailService;
    private readonly IUrlProvider _urlProvider = urlProvider;

    /// <inheritdoc />
    public async Task Handle(VerificationEmailResendEvent notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        string userEmail = notification.User.Email.Value;
        string userName = notification.User.UserName.Value;
        string tokenValue = notification.ResendVerificationToken.Value;

        string confirmationLink = this._urlProvider.GetEmailConfirmationLink(tokenValue);

        await this._emailService.SendConfirmationEmailAsync(
            userEmail,
            userName,
            confirmationLink,
            cancellationToken);
    }
}
