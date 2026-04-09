namespace DotNetTask.Api.Requests.Task;

/// <summary>
/// Represents a request to delete multiple tasks identified by their unique identifiers.
/// </summary>
/// <param name="TaskIds">The collection of task identifiers to delete. Each identifier must correspond to an existing task.</param>
public record DeleteRangeTasksRequest(IEnumerable<Guid> TaskIds);
