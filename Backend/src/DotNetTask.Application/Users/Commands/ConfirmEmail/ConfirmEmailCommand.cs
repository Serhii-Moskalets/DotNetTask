using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.Users.Commands.ConfirmEmail;

/// <summary>
/// Command to confirm user's email address.
/// </summary>
/// <param name="Token">The verification token from the email link.</param>
public record ConfirmEmailCommand(string Token) : ICommand<bool>;
