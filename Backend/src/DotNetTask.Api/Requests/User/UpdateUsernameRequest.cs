namespace DotNetTask.Api.Requests.User;

/// <summary>
/// Represents a request to update a user's username.
/// </summary>
/// <param name="NewUsername">
/// The new username to assign to the user.
/// This value must be a non-empty string and must comply with the application's
/// username requirements.
/// </param>
public record UpdateUsernameRequest(string NewUsername);
