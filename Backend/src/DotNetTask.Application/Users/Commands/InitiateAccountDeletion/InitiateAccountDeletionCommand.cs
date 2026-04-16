using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.Users.Commands.InitiateAccountDeletion;

/// <summary>
/// Represents a command to initiate the deletion process for a user account.
/// </summary>
/// <param name="UserId">The unique identifier of the user whose account is to be deleted.</param>
public record InitiateAccountDeletionCommand(Guid UserId) : ICommand, IThrottledRequest
{
    /// <inheritdoc/>
    /// <value>Always returns "InitiateAccountDeletion".</value>
    public string ActionName => "InitiateAccountDeletion";

    /// <inheritdoc/>
    public string GetIdentity() => this.UserId.ToString();
}
