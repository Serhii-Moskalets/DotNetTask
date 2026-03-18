using MediatR;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Services;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.Comment.Mappers;
using TodoListApp.Application.Common.Dtos;
using TodoListApp.Application.Common.Extensions;
using TodoListApp.Domain.Constants;

namespace TodoListApp.Application.Comment.Queries.GetComments;

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

        var (items, totalCount) = await this.UnitOfWork.Comments
            .GetCommentsByTaskIdAsync(
            query.TaskId,
            query.Page,
            query.PageSize,
            cancellationToken);

        var result = items.ToPagedResult(totalCount, query.Page, query.PageSize, CommentMapper.Map);

        return await Result<PagedResultDto<CommentDto>>.SuccessAsync(result);
    }
}