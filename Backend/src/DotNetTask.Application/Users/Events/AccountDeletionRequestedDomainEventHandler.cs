using DotNetTask.Application.Abstractions.Interfaces.Notifications;
using DotNetTask.Domain.Events;
using MediatR;

namespace DotNetTask.Application.Users.Events;

/// <summary>
/// Handles the <see cref="AccountDeletionRequestedDomainEvent"/> by sending a email change link.
/// </summary>
public class AccountDeletionRequestedDomainEventHandler(IEmailService emailService) : INotificationHandler<AccountDeletionRequestedDomainEvent>
{
    private readonly IEmailService _emailService = emailService;

    /// <inheritdoc />
    public async Task Handle(AccountDeletionRequestedDomainEvent notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        string userEmail = notification.User.Email.Value;
        string userName = notification.User.UserName.Value;

        await this._emailService.SendAccountDeletedEmailAsync(
            userEmail,
            userName,
            cancellationToken);
    }
}
