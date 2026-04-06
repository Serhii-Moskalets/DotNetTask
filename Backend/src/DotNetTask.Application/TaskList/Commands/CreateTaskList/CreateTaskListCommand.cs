using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.TaskList.Commands.CreateTaskList;

/// <summary>
/// Command for creating a new task list for a specific user.
/// </summary>
/// <param name="UserId">
/// The unique identifier of the user who owns the task list.
/// </param>
/// <param name="Title">
/// The raw title of the task list.
/// This value will be validated and converted into a <see cref="Domain.ValueObjects.TaskListTitle"/> value object.
/// </param>
public record CreateTaskListCommand(Guid UserId, string Title)
    : ICommand<Guid>, IThrottledRequest
{
    /// <inheritdoc/>
    /// <value>Always returns "CreateTaskList".</value>
    public string ActionName => "CreateTaskList";

    /// <inheritdoc/>
    public string GetIdentity() => this.UserId.ToString();
}
