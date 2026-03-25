using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Application.Common.Dtos;

namespace DotNetTask.Application.Tag.Queries.GetTags;

/// <summary>
/// Query to retrieve all tags for a specific user.
/// </summary>
public record GetTagsQuery(Guid UserId, int Page = 1, int PageSize = 10)
    : IQuery<PagedResultDto<TagDto>>;
