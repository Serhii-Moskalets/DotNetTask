namespace DotNetTask.Api.Requests.Task;

/// <summary>
/// Represents the parameters for retrieving tasks by their title.
/// </summary>
/// <param name="Page">The page number (1-based).</param>
/// <param name="PageSize">The number of items per page.</param>
/// <param name="Title">Optional. The title (or part of the title) to filter tasks by.</param>
public record GetTaskByTitleRequest(
    int Page,
    int PageSize,
    string? Title = null);