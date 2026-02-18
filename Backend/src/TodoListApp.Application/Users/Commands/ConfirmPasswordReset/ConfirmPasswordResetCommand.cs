using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Users.Commands.ConfirmPasswordReset;

/// <summary>
/// Represents a command to confirm a user's password reset.
/// </summary>
/// <param name="Email">The email address of the user.</param>
/// <param name="NewPassword">The new password to be set.</param>
/// <param name="Token">The token.</param>
public record ConfirmPasswordResetCommand(
    string Email,
    string NewPassword,
    string Token) : ICommand<bool>;
