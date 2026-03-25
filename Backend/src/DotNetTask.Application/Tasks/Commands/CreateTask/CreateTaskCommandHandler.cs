using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

using MediatR;

using TinyResult;

namespace DotNetTask.Application.Tasks.Commands.CreateTask;

/// <summary>
/// Handles the <see cref="CreateTaskCommand"/> to create a new task.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CreateTaskCommandHandler"/> class.
/// </remarks>
public class CreateTaskCommandHandler(
    IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<CreateTaskCommand, Result<Guid>>
{
    /// <summary>
    /// Handles the creation of a new task based on the provided command.
    /// </summary>
    /// <param name="command">The <see cref="CreateTaskCommand"/> containing task details.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Result{Guid}"/> containing the created task identifier.</returns>
    public async Task<Result<Guid>> Handle(CreateTaskCommand command, CancellationToken cancellationToken)
    {
        TaskListEntity? taskList = await this.UnitOfWork.TaskLists
            .GetTaskListByIdForUserAsync(command.Dto.TaskListId, command.UserId, cancellationToken: cancellationToken);

        if (taskList is null)
        {
            return await Result<Guid>.FailureAsync(TinyResult.Enums.ErrorCode.NotFound, TaskListPolicy.NotFoundMessage);
        }

        TaskTitle taskTitle = TaskTitle.Create(command.Dto.Title);

        TaskEntity task = new TaskEntity(
            command.UserId,
            command.Dto.TaskListId,
            taskTitle,
            command.Dto.DueDate);

        await this.UnitOfWork.Tasks.AddAsync(task, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<Guid>.SuccessAsync(task.Id);
    }
}
