using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Users.Commands.RevertEmailChange;

/// <summary>
/// Represents a command to revert a recent email change back to the original address.
/// </summary>
/// <param name="UserId">The unique identifier of the user whose email change is being reverted.</param>
/// <param name="Token">The secure revert token provided in the security alert email.</param>
public record RevertEmailChangeCommand(
    Guid UserId,
    string Token) : ICommand<bool>;
