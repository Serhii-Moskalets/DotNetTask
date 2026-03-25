using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Application.Tasks.Dtos;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

using MediatR;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Tasks.Commands.UpdateTask;

/// <summary>
/// Handles the <see cref="UpdateTaskCommand"/> to update an existing task.
/// </summary>
public class UpdateTaskCommandHandler(
    IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<UpdateTaskCommand, Result<bool>>
{
    /// <summary>
    /// Handles updating a task for a specific user.
    /// </summary>
    /// <param name="command">The <see cref="UpdateTaskCommand"/> containing update task dto.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>A <see cref="Result{Boolean}"/> indicating success or failure of the operation.</returns>
    public async Task<Result<bool>> Handle(UpdateTaskCommand command, CancellationToken cancellationToken)
    {
        TaskEntity? task = await this.UnitOfWork.Tasks
            .GetTaskByIdForUserAsync(command.Dto.TaskId, command.UserId, false, cancellationToken: cancellationToken);
        if (task == null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, TaskPolicy.NotFoundMessage);
        }

        UpdateTaskDto dto = command.Dto;

        if ((dto.Title == task.Title.Value || dto.Title is null) &&
            dto.Description == task.Description?.Value &&
            dto.DueDate == task.DueDate)
        {
            return await Result<bool>.SuccessAsync(true);
        }

        TaskTitle? taskTitle = TaskTitle.CreateOptional(dto.Title);
        TaskDescription? taskDescription = TaskDescription.CreateOptional(dto.Description);

        task.UpdateDetails(taskTitle, taskDescription, command.Dto.DueDate);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
