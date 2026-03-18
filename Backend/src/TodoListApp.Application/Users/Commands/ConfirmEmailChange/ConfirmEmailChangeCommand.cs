using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Users.Commands.ConfirmEmailChange;

/// <summary>
/// Represents a command to confirm a user's email change.
/// </summary>
/// <param name="Token">The token.</param>
public record ConfirmEmailChangeCommand(string Token) : ICommand<bool>;
