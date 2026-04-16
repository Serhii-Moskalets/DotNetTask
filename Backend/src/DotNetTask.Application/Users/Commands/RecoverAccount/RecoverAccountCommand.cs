using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.Users.Commands.RecoverAccount;

/// <summary>
/// Command to recover account.
/// </summary>
/// <param name="UserId">The unique identifier of the user whose email is being changed.</param>
public record RecoverAccountCommand(Guid UserId) : ICommand, IThrottledRequest
{
    /// <inheritdoc/>
    /// <value>Always returns "ChangeEmail".</value>
    public string ActionName => "RecoverAccount";

    /// <inheritdoc/>
    public string GetIdentity() => this.UserId.ToString();
}
