using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Application.Common.Dtos;
using DotNetTask.Application.Common.Extensions;
using DotNetTask.Application.TaskList.Dtos;
using DotNetTask.Application.TaskList.Mappers;

using MediatR;

using TinyResult;

namespace DotNetTask.Application.TaskList.Queries.GetTaskLists;

/// <summary>
/// Handles the <see cref="GetTaskListsQuery"/> by retrieving all task lists
/// for a specific user and mapping them to <see cref="TaskListDto"/> objects.
/// </summary>
public class GetTaskListsQueryHandler(
    IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<GetTaskListsQuery, Result<PagedResultDto<TaskListDto>>>
{
    /// <summary>
    /// Processes the query to get all task lists for a given user.
    /// </summary>
    /// <param name="query">The query containing the user ID.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing a collection of <see cref="TaskListDto"/> objects.
    /// </returns>
    public async Task<Result<PagedResultDto<TaskListDto>>> Handle(GetTaskListsQuery query, CancellationToken cancellationToken)
    {
        (IReadOnlyCollection<Domain.Entities.TaskListEntity>? items, int totalCount) = await this.UnitOfWork.TaskLists
            .GetTaskListsAsync(
            query.UserId,
            query.Page,
            query.PageSize,
            cancellationToken);

        PagedResultDto<TaskListDto> result = items.ToPagedResult(totalCount, query.Page, query.PageSize, TaskListMapper.Map);

        return await Result<PagedResultDto<TaskListDto>>.SuccessAsync(result);
    }
}
