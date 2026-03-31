using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.Users.Commands.RegisterUser;

/// <summary>
/// Represents a command to register a new user in the system.
/// </summary>
/// <param name="FirstName">The user's first name.</param>
/// <param name="LastName">The user's last name (optional).</param>
/// <param name="UserName">The unique username chosen by the user.</param>
/// <param name="Email">The unique email address of the user used for communication and security.</param>
/// <param name="Password">The plain-text password provided by the user, to be hashed before storage.</param>
/// <param name="IpAddress">The IP address of the client making the request, used for security tracking.</param>
public record RegisterUserCommand(
    string FirstName,
    string? LastName,
    string UserName,
    string Email,
    string Password,
    string IpAddress) : ICommand<Guid>, IThrottledRequest
{
    /// <inheritdoc/>
    /// <value>Always returns "Register".</value>
    public string ActionName => "Register";

    /// <inheritdoc/>
    public string GetIdentity() => this.IpAddress;
}
