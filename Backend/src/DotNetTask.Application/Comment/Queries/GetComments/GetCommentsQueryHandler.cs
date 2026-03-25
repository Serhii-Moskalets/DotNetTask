using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Application.Comment.Mappers;
using DotNetTask.Application.Common.Dtos;
using DotNetTask.Application.Common.Extensions;
using DotNetTask.Domain.Constants;

using MediatR;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Comment.Queries.GetComments;

/// <summary>
/// Handles retrieving comments for a specific task.
/// </summary>
/// <remarks>
/// Checks if the user has access to the task and returns mapped comments.
/// </remarks>
public class GetCommentsQueryHandler(
    IUnitOfWork unitOfWork,
    ITaskAccessService taskAccessService)
    : HandlerBase(unitOfWork), IRequestHandler<GetCommentsQuery, Result<PagedResultDto<CommentDto>>>
{
    private readonly ITaskAccessService _taskAccessService = taskAccessService;

    /// <summary>
    /// Handles the specified <see cref="GetCommentsQuery"/>.
    /// </summary>
    /// <param name="query">The query containing the task ID and requesting user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the mapped comments if access is allowed,
    /// or an error if the user does not have access.
    /// </returns>
    public async Task<Result<PagedResultDto<CommentDto>>> Handle(GetCommentsQuery query, CancellationToken cancellationToken)
    {
        if (!await this._taskAccessService.HasAccessAsync(query.TaskId, query.UserId, cancellationToken))
        {
            return await Result<PagedResultDto<CommentDto>>
                .FailureAsync(ErrorCode.InvalidOperation, TaskPolicy.AccessDeniedMessage);
        }

        (IReadOnlyCollection<Domain.Entities.CommentEntity>? items, int totalCount) = await this.UnitOfWork.Comments
            .GetCommentsByTaskIdAsync(
            query.TaskId,
            query.Page,
            query.PageSize,
            cancellationToken);

        PagedResultDto<CommentDto> result = items.ToPagedResult(totalCount, query.Page, query.PageSize, CommentMapper.Map);

        return await Result<PagedResultDto<CommentDto>>.SuccessAsync(result);
    }
}