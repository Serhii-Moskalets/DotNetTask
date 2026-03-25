using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.Tasks.Commands.RemoveTagFromTask;

/// <summary>
/// Command to remove a tag from a specific task.
/// </summary>
public record RemoveTagFromTaskCommand(Guid TaskId, Guid UserId)
    : ICommand;
