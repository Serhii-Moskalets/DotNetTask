using DotNetTask.Domain.Common;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

namespace DotNetTask.Domain.Events;

/// <summary>
/// Represents a domain event that occurs when a user requests to resend their email verification.
/// </summary>
/// <param name="User">The user entity associated with the verification request.</param>
/// <param name="ResendVerificationToken">The newly generated security token for email confirmation.</param>
public record VerificationEmailResendEvent(
    UserEntity User,
    SecurityToken ResendVerificationToken)
    : IDomainEvent;
