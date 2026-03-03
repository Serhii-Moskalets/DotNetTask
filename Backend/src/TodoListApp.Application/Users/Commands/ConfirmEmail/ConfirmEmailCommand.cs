using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Users.Commands.ConfirmEmail;

/// <summary>
/// Command to confirm user's email address.
/// </summary>
/// <param name="Token">The verification token from the email link.</param>
public record ConfirmEmailCommand(string Token) : ICommand<bool>;
