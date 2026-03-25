using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Application.Common.Dtos;
using DotNetTask.Application.Common.Extensions;
using DotNetTask.Application.Tasks.Mappers;

using MediatR;

using TinyResult;

namespace DotNetTask.Application.Tasks.Queries.GetTaskByTitle;

/// <summary>
/// Handles the <see cref="GetTaskByTitleQuery"/> to search for tasks by title for a specific user.
/// </summary>
public class GetTaskByTitleQueryHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<GetTaskByTitleQuery, Result<PagedResultDto<TaskBriefDto>>>
{
    /// <summary>
    /// Handles the retrieval of tasks that match the title search text and maps them to <see cref="TaskBriefDto"/>.
    /// </summary>
    /// <param name="query">The query containing the user ID and title text to search for.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A <see cref="TinyResult.Result{T}"/> representing the outcome of the operation.
    /// </returns>
    /// /// <remarks>
    /// If the search text is null, empty, or consists only of whitespace,
    /// an empty collection is returned.
    /// </remarks>
    public async Task<Result<PagedResultDto<TaskBriefDto>>> Handle(GetTaskByTitleQuery query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query.Text))
        {
            return await Result<PagedResultDto<TaskBriefDto>>.SuccessAsync(new PagedResultDto<TaskBriefDto>
            {
                Items = Array.Empty<TaskBriefDto>(),
                TotalCount = 0,
                Page = query.Page,
                PageSize = query.PageSize,
            });
        }

        (IReadOnlyCollection<Domain.Entities.TaskEntity>? items, int totalCount) = await this.UnitOfWork.Tasks
            .SearchByTitleAsync(query.UserId, query.Text, query.Page, query.PageSize, cancellationToken);

        PagedResultDto<TaskBriefDto> result = items.ToPagedResult(totalCount, query.Page, query.PageSize, TaskMapper.MapToBrief);

        return await Result<PagedResultDto<TaskBriefDto>>.SuccessAsync(result);
    }
}
