using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Application.Tasks.Dtos;

namespace DotNetTask.Application.Tasks.Commands.UpdateTask;

/// <summary>
/// Represents a command to update the details of an existing task.
/// </summary>
/// <param name="Dto">The data transfer object containing the updated task information.</param>
/// <param name="UserId">The unique identifier of the user performing the update.</param>
public record UpdateTaskCommand(UpdateTaskDto Dto, Guid UserId)
    : ICommand, IThrottledRequest
{
    /// <inheritdoc/>
    /// <value>Always returns "UpdateTask".</value>
    public string ActionName => "UpdateTask";

    /// <inheritdoc/>
    public string GetIdentity() => this.UserId.ToString();
}
