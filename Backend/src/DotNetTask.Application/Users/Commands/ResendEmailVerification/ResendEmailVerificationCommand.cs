using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.Users.Commands.ResendEmailVerification;

/// <summary>
/// Represents a command to resend email verification.
/// </summary>
/// <param name="UserId">The unique identifier of the user whose is being updated.</param>
/// <param name="IpAddress">The IP address of the client making the request, used for security tracking.</param>
public record ResendEmailVerificationCommand(Guid UserId, string IpAddress)
    : ICommand<bool>, IThrottledRequest
{
    /// <inheritdoc/>
    /// <value>Always returns "ResendEmailVerification".</value>
    public string ActionName => "ResendEmailVerification";

    /// <inheritdoc/>
    public string GetIdentity() => $"{this.UserId}:{this.IpAddress}";
}
