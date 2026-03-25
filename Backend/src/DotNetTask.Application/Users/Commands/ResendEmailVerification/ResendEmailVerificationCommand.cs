using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.Users.Commands.ResendEmailVerification;

/// <summary>
/// Represents a command to resend email verification.
/// </summary>
/// <param name="UserId">The unique identifier of the user whose  is being updated.</param>
public record ResendEmailVerificationCommand(Guid UserId) : ICommand<bool>;
