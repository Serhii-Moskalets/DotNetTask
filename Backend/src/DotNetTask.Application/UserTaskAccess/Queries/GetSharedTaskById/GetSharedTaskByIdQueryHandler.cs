using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Application.Common.Dtos;
using DotNetTask.Application.UserTaskAccess.Mappers;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;

using MediatR;

using TinyResult;

namespace DotNetTask.Application.UserTaskAccess.Queries.GetSharedTaskById;

/// <summary>
/// Handles retrieval of a task shared with a specific user.
/// </summary>
public class GetSharedTaskByIdQueryHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<GetSharedTaskByIdQuery, Result<TaskDto>>
{
    /// <summary>
    /// Handles the <see cref="GetSharedTaskByIdQuery"/> request.
    /// </summary>
    /// <param name="query">
    /// The query containing the task identifier and the user identifier.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.
    /// </param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the shared task data if the user has access;
    /// otherwise, a failure result indicating that the task was not found or access is denied.
    /// </returns>
    public async Task<Result<TaskDto>> Handle(GetSharedTaskByIdQuery query, CancellationToken cancellationToken)
    {
        UserTaskAccessEntity? access = await this.UnitOfWork.UserTaskAccesses
            .GetByTaskAndUserIdAsync(query.TaskId, query.UserId, cancellationToken);

        if (access is null)
        {
            return Result<TaskDto>.Failure(
                TinyResult.Enums.ErrorCode.NotFound,
                TaskPolicy.NotFoundMessage);
        }

        return Result<TaskDto>.Success(
            TaskAccessForUserMapper.Map(access));
    }
}
