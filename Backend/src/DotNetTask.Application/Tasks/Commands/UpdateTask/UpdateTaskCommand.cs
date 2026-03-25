using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Application.Tasks.Dtos;

namespace DotNetTask.Application.Tasks.Commands.UpdateTask;

/// <summary>
/// Command to update an existing task using a DTO.
/// </summary>
public record UpdateTaskCommand(UpdateTaskDto Dto, Guid UserId)
    : ICommand;
