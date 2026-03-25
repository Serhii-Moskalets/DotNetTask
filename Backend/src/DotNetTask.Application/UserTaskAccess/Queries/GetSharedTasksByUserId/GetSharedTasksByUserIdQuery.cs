using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Application.Common.Dtos;

namespace DotNetTask.Application.UserTaskAccess.Queries.GetSharedTasksByUserId;

/// <summary>
/// Represents a query for retrieving all tasks that are shared with a specific user.
/// </summary>
public record GetSharedTasksByUserIdQuery(Guid UserId, int Page = 1, int PageSize = 10)
    : IQuery<PagedResultDto<TaskDto>>;