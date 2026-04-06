using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.Users.Commands.LoginUser;

/// <summary>
/// Represents a command for user authentication that includes built-in rate limiting parameters.
/// </summary>
/// <param name="Email">The email address of the user attempting to log in.</param>
/// <param name="Password">The plain-text password to be verified</param>
/// <param name="IpAddress">The IP address of the client making the request, used for security tracking.</param>
public record LoginUserCommand(string Email, string Password, string IpAddress)
    : ICommand<LoginResponse>, IThrottledRequest
{
    /// <inheritdoc/>
    /// <value>Always returns "Login".</value>
    public string ActionName => "Login";

    /// <inheritdoc/>
    public string GetIdentity() => this.IpAddress;
}
