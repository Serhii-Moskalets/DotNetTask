using TodoListApp.Domain.Common;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Domain.Events;

/// <summary>
/// Represents a domain event that occurs when a user requests to resend their email verification.
/// </summary>
/// <param name="User">The user entity associated with the verification request.</param>
/// <param name="ResendVerificationToken">The newly generated security token for email confirmation.</param>
public record VerificationEmailResentEvent(
    UserEntity User,
    SecurityToken ResendVerificationToken)
    : IDomainEvent;
