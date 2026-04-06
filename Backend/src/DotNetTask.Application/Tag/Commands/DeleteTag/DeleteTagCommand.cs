using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.Tag.Commands.DeleteTag;

/// <summary>
/// Represents a command to delete a specific tag from the system.
/// </summary>
/// <param name="TagId">The unique identifier of the tag to be deleted.</param>
/// <param name="UserId">The unique identifier of the user performing the deletion.</param>
public record DeleteTagCommand(Guid TagId, Guid UserId)
    : ICommand, IThrottledRequest
{
    /// <inheritdoc/>
    /// <value>Always returns "DeleteTag".</value>
    public string ActionName => "DeleteTag";

    /// <inheritdoc/>
    public string GetIdentity() => this.UserId.ToString();
}
