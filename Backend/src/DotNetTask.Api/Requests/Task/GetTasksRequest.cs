using DotNetTask.Domain.Enums;

namespace DotNetTask.Api.Requests.Task;

/// <summary>
/// Represents the parameters for retrieving tasks with optional filtering, sorting, and date ranges.
/// </summary>
/// <param name="TaskListId">The ID of the task list from which to retrieve tasks.</param>
/// <param name="Page">The page number (1-based).</param>
/// <param name="PageSize">The number of items per page.</param>
/// <param name="TaskStatuses">Optional. A collection of task statuses to filter the tasks by.</param>
/// <param name="DueBefore">Optional. Only include tasks with a due date before this value.</param>
/// <param name="DueAfter">Optional. Only include tasks with a due date after this value.</param>
/// <param name="TaskSortBy">Optional. Specifies the field by which to sort the tasks.</param>
/// <param name="Ascending">Specifies whether to sort in ascending order. Default is <c>true</c>.</param>
public record GetTasksRequest(
    Guid TaskListId,
    int Page = 1,
    int PageSize = 10,
    IReadOnlyCollection<StatusTask>? TaskStatuses = null,
    DateTime? DueBefore = null,
    DateTime? DueAfter = null,
    TaskSortBy? TaskSortBy = null,
    bool Ascending = true);
