using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Domain.Enums;

namespace DotNetTask.Application.Tasks.Commands.ChangeTaskStatus;

/// <summary>
/// Command to change the status of a specific task.
/// </summary>
public record ChangeTaskStatusCommand(Guid TaskId, Guid UserId, StatusTask Status)
    : ICommand;
