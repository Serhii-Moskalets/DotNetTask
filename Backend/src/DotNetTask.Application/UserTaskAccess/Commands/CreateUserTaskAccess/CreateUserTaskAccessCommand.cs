using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.UserTaskAccess.Commands.CreateUserTaskAccess;

/// <summary>
/// Command to create a user-task access relationship.
/// </summary>
public record CreateUserTaskAccessCommand(Guid TaskId, Guid OwnerId, string Email)
    : ICommand;