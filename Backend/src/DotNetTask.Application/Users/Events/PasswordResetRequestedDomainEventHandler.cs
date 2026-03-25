using DotNetTask.Application.Abstractions.Interfaces.Common;
using DotNetTask.Application.Abstractions.Interfaces.Notifications;
using DotNetTask.Domain.Events;

using MediatR;

namespace DotNetTask.Application.Users.Events;

/// <summary>
/// Handles the <see cref="PasswordResetRequestedDomainEvent"/> by sending a password reset link.
/// </summary>
public class PasswordResetRequestedDomainEventHandler(
    IEmailService emailService,
    IUrlProvider urlProvider)
    : INotificationHandler<PasswordResetRequestedDomainEvent>
{
    private readonly IEmailService _emailService = emailService;
    private readonly IUrlProvider _urlPrivider = urlProvider;

    /// <inheritdoc />
    public async Task Handle(PasswordResetRequestedDomainEvent notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        string userEmail = notification.User.Email.Value;
        string tokenValue = notification.ResetToken.Value;

        string resetLink = this._urlPrivider.GetPasswordResetLink(tokenValue);

        await this._emailService.SendPasswordResetEmailAsync(
            userEmail,
            notification.User.UserName.Value,
            resetLink,
            cancellationToken);
    }
}
