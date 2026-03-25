using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.Users.Commands.RevertEmailChange;

/// <summary>
/// Represents a command to revert a recent email change back to the original address.
/// </summary>
/// <param name="Token">The secure revert token provided in the security alert email.</param>
public record RevertEmailChangeCommand(string Token) : ICommand<string>;
