using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.Users.Commands.ResetPassword;

/// <summary>
/// Represents a command to reset a user's password.
/// </summary>
/// <param name="Email">The email address of the user.</param>
public record ResetPasswordCommand(string Email) : ICommand<bool>;
