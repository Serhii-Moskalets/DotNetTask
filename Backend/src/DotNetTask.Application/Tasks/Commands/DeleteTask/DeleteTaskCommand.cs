using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.Tasks.Commands.DeleteTask;

/// <summary>
/// Command to delete a specific task for a given user.
/// </summary>
public record DeleteTaskCommand(Guid TaskId, Guid UserId)
    : ICommand;
