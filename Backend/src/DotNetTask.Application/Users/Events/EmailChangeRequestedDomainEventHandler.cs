using DotNetTask.Application.Abstractions.Interfaces.Common;
using DotNetTask.Application.Abstractions.Interfaces.Notifications;
using DotNetTask.Domain.Events;

using MediatR;

namespace DotNetTask.Application.Users.Events;

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

        string? originalEmail = notification.RevertToken.Metadata;
        ArgumentNullException.ThrowIfNull(originalEmail);

        string? userNewEmail = notification.ConfirmationToken.Metadata;
        ArgumentNullException.ThrowIfNull(userNewEmail);

        string changeLink = this._urlProvider.GetEmailChangeLink(notification.ConfirmationToken.Value);

        Task confirmTask = this._emailService.SendEmailChangeConfirmationAsync(
            userNewEmail,
            notification.User.UserName.Value,
            changeLink,
            cancellationToken);

        string revertLink = this._urlProvider.GetEmailRevertLink(notification.RevertToken.Value);

        Task alertTask = this._emailService.SendEmailChangeSecurityAlertAsync(
            originalEmail,
            userNewEmail,
            notification.User.UserName.Value,
            revertLink,
            cancellationToken);

        await Task.WhenAll(confirmTask, alertTask);
    }
}
