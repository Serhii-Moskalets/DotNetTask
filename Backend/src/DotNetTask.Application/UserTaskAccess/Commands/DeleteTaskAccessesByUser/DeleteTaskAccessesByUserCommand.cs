using DotNetTask.Application.Abstractions.Messaging;

namespace DotNetTask.Application.UserTaskAccess.Commands.DeleteTaskAccessesByUser;

/// <summary>
/// Represents a command to delete all user-task access entries for a specific user.
/// </summary>
public record DeleteTaskAccessesByUserCommand(Guid UserId)
    : ICommand;
