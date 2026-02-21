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

        var userNewEmail = notification.User.Email.Value;
        var userId = notification.User.Id;

        var changeLink = this._urlProvider.GetEmailChangeLink(
            userId,
            notification.ConfirmationToken.Value,
            userNewEmail);

        var confirmTask = this._emailService.SendEmailChangeConfirmationAsync(
            userNewEmail,
            notification.User.UserName.Value,
            changeLink,
            cancellationToken);

        var revertLink = this._urlProvider.GetEmailRevertLink(
            userId,
            notification.RevertToken.Value);

        var alertTask = this._emailService.SendEmailChangeSecurityAlertAsync(
            originalEmail,
            userNewEmail,
            notification.User.UserName.Value,
            revertLink,
            cancellationToken);

        await Task.WhenAll(confirmTask, alertTask);
    }
}
