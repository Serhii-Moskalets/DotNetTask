using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Application.Common.Dtos;

namespace DotNetTask.Application.Tasks.Queries.GetTaskByTitle;

/// <summary>
/// Query to retrieve a task by its title (or partial title) for a specific user.
/// </summary>
public sealed record GetTaskByTitleQuery(Guid UserId, string? Text, int Page = 1, int PageSize = 10)
    : IQuery<PagedResultDto<TaskBriefDto>>;
