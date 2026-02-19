using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Users.Commands.ConfirmChangeEmail;

/// <summary>
/// Represents a command to confirm a user's email change.
/// </summary>
/// <param name="UserId">The UserId of the user.</param>
/// <param name="Token">The token.</param>
public record ConfirmChangeEmailCommand(
    Guid UserId,
    string Token) : ICommand<bool>;
