using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.Users.Commands.ConfirmEmailChange;

/// <summary>
/// Represents a command to confirm a user's email change.
/// </summary>
/// <param name="Token">The token.</param>
public record ConfirmEmailChangeCommand(string Token) : ICommand<bool>;
