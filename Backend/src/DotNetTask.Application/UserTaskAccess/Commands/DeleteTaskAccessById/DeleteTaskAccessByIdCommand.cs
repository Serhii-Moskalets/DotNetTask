using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.UserTaskAccess.Commands.DeleteTaskAccessById;

/// <summary>
/// Represents a command to delete a user-task access entry by task and user IDs.
/// </summary>
public record DeleteTaskAccessByIdCommand(Guid TaskId, Guid UserId, Guid OwnerId)
    : ICommand;
