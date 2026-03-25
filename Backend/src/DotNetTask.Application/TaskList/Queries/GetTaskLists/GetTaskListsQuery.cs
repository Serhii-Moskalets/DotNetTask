using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Application.Common.Dtos;
using DotNetTask.Application.TaskList.Dtos;

namespace DotNetTask.Application.TaskList.Queries.GetTaskLists;

/// <summary>
/// Represents a query to retrieve all task lists for a specific user.
/// </summary>
public record GetTaskListsQuery(Guid UserId, int Page = 1, int PageSize = 10)
    : IQuery<PagedResultDto<TaskListDto>>;
