using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.Tasks.Commands.DeleteRangeTasks;

/// <summary>
/// Represents a command to delete multiple tasks identified by their IDs for a specific user.
/// </summary>
/// <param name="TaskIds">The collection of unique identifiers for the tasks to be deleted. Cannot be null or contain duplicate values.</param>
/// <param name="UserId">The unique identifier of the user who owns the tasks to be deleted.</param>
public record DeleteRangeTasksCommand(IEnumerable<Guid> TaskIds, Guid UserId)
    : ICommand<int>, IThrottledRequest
{
    /// <inheritdoc/>
    /// /// <value>Always returns "DeleteRangeTasks".</value>
    public string ActionName => "DeleteRangeTasks";

    /// <inheritdoc/>
    public string GetIdentity() => this.UserId.ToString();
}
