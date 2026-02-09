using MediatR;
using TinyResult;

namespace TodoListApp.Application.Users.Commands.LoginUser;

/// <summary>
/// Represents a command to authenticate a user in the system.
/// </summary>
/// <param name="Email">
/// The email address associated with the user account used for authentication.
/// </param>
/// <param name="Password">
/// The plain-text password provided by the user for authentication.
/// </param>
public record LoginUserCommand(
    string Email,
    string Password) : IRequest<Result<LoginResponse>>;
