using DotNetTask.Application.Abstractions.Interfaces.Common;
using DotNetTask.Application.Abstractions.Interfaces.Notifications;
using DotNetTask.Domain.Events;

using MediatR;

namespace DotNetTask.Application.Users.Events;

/// <summary>
/// Handles the <see cref="UserRegisteredDomainEvent"/> by sending a verification email.
/// </summary>
public class UserRegisteredDomainEventHandler(
    IEmailService emailService,
    IUrlProvider urlProvider)
    : INotificationHandler<UserRegisteredDomainEvent>
{
    private readonly IEmailService _emailService = emailService;
    private readonly IUrlProvider _urlProvider = urlProvider;

    /// <inheritdoc />
    public async Task Handle(UserRegisteredDomainEvent notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        string userEmail = notification.User.Email.Value;
        string userName = notification.User.UserName.Value;
        string tokenValue = notification.VerificationToken.Value;

        string confirmationLink = this._urlProvider.GetEmailConfirmationLink(tokenValue);

        await this._emailService.SendConfirmationEmailAsync(
            userEmail,
            userName,
            confirmationLink,
            cancellationToken);
    }
}
