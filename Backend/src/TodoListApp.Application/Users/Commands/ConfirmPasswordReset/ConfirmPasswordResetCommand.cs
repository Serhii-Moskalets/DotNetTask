using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Users.Commands.ConfirmPasswordReset;

/// <summary>
/// Represents a command to confirm a user's password reset.
/// </summary>
/// <param name="UserId">The unique identifier of the user.</param>
/// <param name="NewPassword">The new password to be set.</param>
/// <param name="Token">The token.</param>
public record ConfirmPasswordResetCommand(
    Guid UserId,
    string NewPassword,
    string Token) : ICommand<bool>;
