using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Users.Commands.ConfirmChangeEmail;

/// <summary>
/// Represents a command to confirm a user's email change.
/// </summary>
/// <param name="Token">The token.</param>
public record ConfirmChangeEmailCommand(string Token) : ICommand<bool>;
