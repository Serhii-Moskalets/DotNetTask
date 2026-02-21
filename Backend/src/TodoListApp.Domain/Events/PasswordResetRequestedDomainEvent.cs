using TodoListApp.Domain.Common;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Domain.Events;

/// <summary>
/// Domain event raised when a user requests a password reset link.
/// </summary>
/// <param name="User">The user entity requesting the password reset.</param>
/// <param name="ResetToken">The security token generated specifically for the password reset operation.</param>
public record PasswordResetRequestedDomainEvent(
    UserEntity User,
    SecurityToken ResetToken)
    : IDomainEvent;
