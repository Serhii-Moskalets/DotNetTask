namespace TodoListApp.Api.Requests.Comment;

/// <summary>
/// Represents the request data containing the text of a comment.
/// Used for creating or updating a comment.
/// </summary>
/// <param name="Content">The content of the comment.</param>
public record CommentTextRequest(string Content);
