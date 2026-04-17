using DotNetTask.Domain.Common;
using DotNetTask.Domain.Entities;

namespace DotNetTask.Domain.Events;

/// <summary>
/// Domain event raised when a user initiates the process of deleting their account.
/// </summary>
/// <param name="User">The user entity who requested the account deletion.</param>
public record AccountDeletionRequestedDomainEvent(
    UserEntity User)
    : IDomainEvent;
