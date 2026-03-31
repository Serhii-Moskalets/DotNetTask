using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Application.Tasks.Dtos;

namespace DotNetTask.Application.Tasks.Commands.CreateTask;

/// <summary>
/// Represents a command to create a new task within a task list.
/// </summary>
/// <param name="Dto">The data transfer object containing task details like Title and Description.</param>
/// <param name="UserId">The unique identifier of the user creating the task.</param>
public record CreateTaskCommand(CreateTaskDto Dto, Guid UserId)
    : ICommand<Guid>, IThrottledRequest
{
    /// <inheritdoc/>
    /// <value>Always returns "CreateTask".</value>
    public string ActionName => "CreateTask";

    /// <inheritdoc/>
    public string GetIdentity() => this.UserId.ToString();
}
