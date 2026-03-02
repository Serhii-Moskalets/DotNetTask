namespace TodoListApp.Api.Requests.User;

/// <summary>
/// Represents a request to update user profile information.
/// </summary>
/// <param name="NewFirstName">The new first name for the currently authenticated user.</param>
/// <param name="NewLastName">The new last name for the currently authenticated user.</param>
public record UpdateUserProfileRequest(string? NewFirstName, string? NewLastName);
