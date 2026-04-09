using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Application.Tasks.Dtos;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

using MediatR;

using TinyResult;
using TinyResult.Enums;

using Unit = DotNetTask.Domain.Common.Unit;

namespace DotNetTask.Application.Tasks.Commands.UpdateTask;

/// <summary>
/// Handles the <see cref="UpdateTaskCommand"/> to update an existing task.
/// </summary>
public class UpdateTaskCommandHandler(
    IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<UpdateTaskCommand, Result<Unit>>
{
    /// <summary>
    /// Handles updating a task for a specific user.
    /// </summary>
    /// <param name="command">The <see cref="UpdateTaskCommand"/> containing update task dto.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A <see cref="Result{Unit}"/> indicating successful completion,
    /// or a failure result if the task is not found.
    /// </returns>
    public async Task<Result<Unit>> Handle(UpdateTaskCommand command, CancellationToken cancellationToken)
    {
        TaskEntity? task = await this.UnitOfWork.Tasks
            .GetTaskByIdForUserAsync(command.Dto.TaskId, command.UserId, false, cancellationToken: cancellationToken);
        if (task == null)
        {
            return Result<Unit>.Failure(ErrorCode.NotFound, TaskPolicy.NotFoundMessage);
        }

        UpdateTaskDto dto = command.Dto;

        if ((dto.Title == task.Title.Value || dto.Title is null) &&
            dto.Description == task.Description?.Value &&
            dto.DueDate == task.DueDate)
        {
            return Result<Unit>.Success(Unit.Value);
        }

        TaskTitle? taskTitle = TaskTitle.CreateOptional(dto.Title);
        TaskDescription? taskDescription = TaskDescription.CreateOptional(dto.Description);

        task.UpdateDetails(taskTitle, taskDescription, command.Dto.DueDate);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
