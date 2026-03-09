namespace TodoListApp.Api.Requests.Tag;

/// <summary>
/// Represents a request to create or update a tag's title.
/// </summary>
/// <param name="Name">The name of the tag. Optional.</param>
public record TagTitleRequest(string Name);
