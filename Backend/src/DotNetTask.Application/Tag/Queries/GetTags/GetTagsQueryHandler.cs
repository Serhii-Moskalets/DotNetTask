using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Application.Common.Dtos;
using DotNetTask.Application.Common.Extensions;
using DotNetTask.Application.Tag.Mappers;

using MediatR;

using TinyResult;

namespace DotNetTask.Application.Tag.Queries.GetTags;

/// <summary>
/// Handles the <see cref="GetTagsQuery"/> to retrieve all tags for a specific user.
/// </summary>
public class GetTagsQueryHandler(
    IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<GetTagsQuery, Result<PagedResultDto<TagDto>>>
{
    /// <summary>
    /// Handles the query to get all tags for the specified user.
    /// </summary>
    /// <param name="query">The query containing the user's ID.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing a list of <see cref="TagDto"/> objects for the user.
    /// </returns>
    public async Task<Result<PagedResultDto<TagDto>>> Handle(GetTagsQuery query, CancellationToken cancellationToken)
    {
        (IReadOnlyCollection<Domain.Entities.TagEntity>? items, int totalCount) = await this.UnitOfWork.Tags
            .GetTagsAsync(
            query.UserId,
            query.Page,
            query.PageSize,
            cancellationToken);

        PagedResultDto<TagDto> result = items.ToPagedResult(totalCount, query.Page, query.PageSize, TagMapper.Map);

        return Result<PagedResultDto<TagDto>>.Success(result);
    }
}
