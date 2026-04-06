using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.Users.Commands.ResetPassword;

/// <summary>
/// Represents a command to reset a user's password.
/// </summary>
/// <param name="Email">The email address of the user.</param>
/// <param name="IpAddress">The IP address of the client making the request, used for security tracking.</param>
public record ResetPasswordCommand(string Email, string IpAddress)
    : ICommand<bool>, IThrottledRequest
{
    /// <inheritdoc/>
    /// <value>Always returns "PasswordReset".</value>
    public string ActionName => "PasswordReset";

    /// <inheritdoc/>
    public string GetIdentity() => $"{this.IpAddress}:{this.Email}";
}
