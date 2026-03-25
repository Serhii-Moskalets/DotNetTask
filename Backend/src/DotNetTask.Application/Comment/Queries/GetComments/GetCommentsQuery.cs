using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Application.Common.Dtos;

namespace DotNetTask.Application.Comment.Queries.GetComments;

/// <summary>
/// Represents a query to retrieve all comments for a specific task.
/// </summary>
public record GetCommentsQuery(
    Guid TaskId,
    Guid UserId,
    int Page = 1,
    int PageSize = 10)
    : IQuery<PagedResultDto<CommentDto>>;