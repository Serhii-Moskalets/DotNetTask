using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.UserTaskAccess.Commands.DeleteTaskAccessesByTask
{
    /// <summary>
    /// Represents a command to delete all user-task access entries for a specific task.
    /// </summary>
    public record DeleteTaskAccessesByTaskCommand(Guid TaskId, Guid UserId)
        : ICommand;
}
