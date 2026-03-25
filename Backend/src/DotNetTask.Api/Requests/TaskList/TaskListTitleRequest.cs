namespace DotNetTask.Api.Requests.TaskList;

/// <summary>
/// Represents a request to create or update a task list title.
/// </summary>
/// <param name="Title">The title of the task list.</param>
public record TaskListTitleRequest(string Title);
