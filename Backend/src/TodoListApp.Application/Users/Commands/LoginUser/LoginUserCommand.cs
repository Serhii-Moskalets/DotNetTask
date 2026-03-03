using TinyResult;
using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Users.Commands.LoginUser;

/// <summary>
/// Represents a command for user authentication.
/// </summary>
/// <param name="Email">The email address of the user.</param>
/// <param name="Password">The plain-text password to be verified.</param>
public record LoginUserCommand(
    string Email,
    string Password) : ICommand<LoginResponse>;
