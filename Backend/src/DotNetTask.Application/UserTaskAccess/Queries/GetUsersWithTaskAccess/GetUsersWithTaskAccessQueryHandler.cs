using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Application.Common.Extensions;
using DotNetTask.Application.UserTaskAccess.Dtos;
using DotNetTask.Application.UserTaskAccess.Mappers;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;

using MediatR;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.UserTaskAccess.Queries.GetUsersWithTaskAccess;

/// <summary>
/// Handles the <see cref="GetUsersWithTaskAccessQuery"/> and returns a list of users
/// who have shared access to a specific task.
/// </summary>
public class GetUsersWithTaskAccessQueryHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<GetUsersWithTaskAccessQuery, Result<TaskAccessListDto>>
{
    /// <summary>
    /// Handles the query to retrieve users with shared access to a task.
    /// </summary>
    /// <param name="query">The query containing the task ID and the requesting user ID.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Result{TaskAccessListDto}"/> containing the list of users with access,
    /// or a failure result if the requesting user is not the owner of the task.
    /// </returns>
    public async Task<Result<TaskAccessListDto>> Handle(GetUsersWithTaskAccessQuery query, CancellationToken cancellationToken)
    {
        TaskEntity? task = await this.UnitOfWork.Tasks
            .GetTaskByIdForUserAsync(query.TaskId, query.UserId, asNoTracking: true, cancellationToken);
        if (task is null)
        {
            return await Result<TaskAccessListDto>.FailureAsync(ErrorCode.NotFound, UserTaskAccessPolicy.TaskNotFoundOrAccessDeniedMessage);
        }

        (IReadOnlyCollection<UserTaskAccessEntity>? items, int totalCount) = await this.UnitOfWork.UserTaskAccesses
            .GetUserTaskAccessByTaskIdAsync(
            query.TaskId,
            query.Page,
            query.PageSize,
            cancellationToken);

        return await Result<TaskAccessListDto>.SuccessAsync(new TaskAccessListDto
        {
            Id = task.Id,
            Title = task.Title.Value,
            Users = items.ToPagedResult(totalCount, query.Page, query.PageSize, TaskAccessForOwnerMapper.Map),
        });
    }
}
