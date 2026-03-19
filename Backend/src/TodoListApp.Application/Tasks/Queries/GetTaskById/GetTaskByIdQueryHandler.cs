using MediatR;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Common.Dtos;
using TodoListApp.Application.Tasks.Mappers;
using TodoListApp.Domain.Constants;

namespace TodoListApp.Application.Tasks.Queries.GetTaskById;

/// <summary>
/// Handles the <see cref="GetTaskByIdQuery"/> to retrieve a specific task for a given user.
/// </summary>
public class GetTaskByIdQueryHandler(
    IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<GetTaskByIdQuery, Result<TaskDto>>
{
    /// <summary>
    /// Handles the retrieval of a task and maps it to a <see cref="TaskDto"/>.
    /// </summary>
    /// <param name="query">The query containing the task and user identifiers.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A <see cref="Result{TaskDto}"/> representing the outcome of the operation:
    /// - Success with the <see cref="TaskDto"/> if the task exists
    /// - Failure with <see cref="ErrorCode.NotFound"/> if the task does not exist.
    /// </returns>
    public async Task<Result<TaskDto>> Handle(GetTaskByIdQuery query, CancellationToken cancellationToken)
    {
        var taskEntity = await this.UnitOfWork.Tasks
            .GetTaskByIdForUserAsync(query.TaskId, query.UserId, cancellationToken: cancellationToken);

        if (taskEntity is null)
        {
            return await Result<TaskDto>.FailureAsync(ErrorCode.NotFound, TaskPolicy.NotFoundMessage);
        }

        var taskDto = TaskMapper.Map(taskEntity);

        return await Result<TaskDto>.SuccessAsync(taskDto);
    }
}
