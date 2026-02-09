using TodoListApp.Domain.Common;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Domain.Events;

/// <summary>
/// Domain event raised when a new user is successfully registered in the system.
/// </summary>
/// <param name="User">The newly created user entity.</param>
/// <param name="VerificationToken">The security token generated for initial email verification.</param>
public record UserRegisteredDomainEvent(
    UserEntity User,
    SecurityToken VerificationToken)
    : IDomainEvent;
