using TodoListApp.Domain.Common;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Domain.Events;

/// <summary>
/// Domain event raised when a user requests to change their email address.
/// </summary>
/// <param name="User">The user entity whose email is being changed.</param>
/// <param name="ConfirmationToken">The security token used to confirm the new email address.</param>
/// <param name="OldEmail">The user's previous email address, maintained for notification or logging purposes.</param>
public record EmailChangeRequestedDomainEvent(
    UserEntity User,
    SecurityToken ConfirmationToken,
    string OldEmail)
    : IDomainEvent;