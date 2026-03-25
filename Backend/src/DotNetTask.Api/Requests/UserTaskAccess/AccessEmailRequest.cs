namespace DotNetTask.Api.Requests.UserTaskAccess;

/// <summary>
/// Represents the request data containing the email of a user
/// for granting or revoking access to a task.
/// </summary>
/// <param name="Email">
/// The email address of the user.
/// Can be <see langword="null"/> if no email is provided.
/// </param>
public record AccessEmailRequest(string? Email);
