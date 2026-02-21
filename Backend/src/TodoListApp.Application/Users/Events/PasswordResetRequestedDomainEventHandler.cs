using MediatR;
using TodoListApp.Application.Abstractions.Interfaces.Common;
using TodoListApp.Application.Abstractions.Interfaces.Notifications;
using TodoListApp.Domain.Events;

namespace TodoListApp.Application.Users.Events;

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

        var userEmail = notification.User.Email.Value;
        var userId = notification.User.Id;
        var tokenValue = notification.ResetToken.Value;

        var resetLink = this._urlPrivider.GetPasswordResetLink(userId, tokenValue);

        await this._emailService.SendPasswordResetEmailAsync(
            userEmail,
            notification.User.UserName.Value,
            resetLink,
            cancellationToken);
    }
}
