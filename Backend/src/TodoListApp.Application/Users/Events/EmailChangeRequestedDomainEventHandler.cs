using MediatR;
using TodoListApp.Application.Abstractions.Interfaces.Common;
using TodoListApp.Application.Abstractions.Interfaces.Notifications;
using TodoListApp.Domain.Events;

namespace TodoListApp.Application.Users.Events;

/// <summary>
/// Handles the <see cref="EmailChangeRequestedDomainEvent"/> by sending a email change link.
/// </summary>
public class EmailChangeRequestedDomainEventHandler(
    IEmailService emailService,
    IUrlProvider urlProvider)
    : INotificationHandler<EmailChangeRequestedDomainEvent>
{
    private readonly IEmailService _emailService = emailService;
    private readonly IUrlProvider _urlProvider = urlProvider;

    /// <inheritdoc />
    public async Task Handle(EmailChangeRequestedDomainEvent notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var originalEmail = notification.RevertToken.Metadata;
        ArgumentNullException.ThrowIfNull(originalEmail);

        var userNewEmail = notification.ConfirmationToken.Metadata;
        ArgumentNullException.ThrowIfNull(userNewEmail);

        var changeLink = this._urlProvider.GetEmailChangeLink(notification.ConfirmationToken.Value);

        var confirmTask = this._emailService.SendEmailChangeConfirmationAsync(
            userNewEmail,
            notification.User.UserName.Value,
            changeLink,
            cancellationToken);

        var revertLink = this._urlProvider.GetEmailRevertLink(notification.RevertToken.Value);

        var alertTask = this._emailService.SendEmailChangeSecurityAlertAsync(
            originalEmail,
            userNewEmail,
            notification.User.UserName.Value,
            revertLink,
            cancellationToken);

        await Task.WhenAll(confirmTask, alertTask);
    }
}
