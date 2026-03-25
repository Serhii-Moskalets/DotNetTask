using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Application.Common.Dtos;
using DotNetTask.Domain.Enums;

namespace DotNetTask.Application.Tasks.Queries.GetTasks;

/// <summary>
/// Query to retrieve a list of tasks for a specific user with optional filters and sorting.
/// </summary>
public sealed record GetTasksQuery(
    Guid UserId,
    Guid TaskListId,
    int Page = 1,
    int PageSize = 10,
    IReadOnlyCollection<StatusTask>? TaskStatuses = null,
    DateTime? DueBefore = null,
    DateTime? DueAfter = null,
    TaskSortBy? TaskSortBy = null,
    bool Ascending = true)
    : IQuery<PagedResultDto<TaskBriefDto>>;
