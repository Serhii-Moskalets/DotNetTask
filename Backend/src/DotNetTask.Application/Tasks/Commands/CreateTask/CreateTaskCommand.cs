using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Application.Tasks.Dtos;

namespace DotNetTask.Application.Tasks.Commands.CreateTask;

/// <summary>
/// Command to create a new task using a DTO.
/// </summary>
public record CreateTaskCommand(CreateTaskDto Dto, Guid UserId)
    : ICommand<Guid>;
