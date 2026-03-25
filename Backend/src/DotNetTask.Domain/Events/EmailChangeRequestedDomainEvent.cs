using DotNetTask.Domain.Common;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

namespace DotNetTask.Domain.Events;

/// <summary>
/// Domain event raised when a user requests to change their email address.
/// </summary>
/// <param name="User">The user entity whose email is being changed.</param>
/// <param name="ConfirmationToken">The security token used to confirm the new email address.</param>
/// <param name="RevertToken">The security token used to cancel the request and secure the account from the old email.</param>
public record EmailChangeRequestedDomainEvent(
    UserEntity User,
    SecurityToken ConfirmationToken,
    SecurityToken RevertToken)
    : IDomainEvent;
