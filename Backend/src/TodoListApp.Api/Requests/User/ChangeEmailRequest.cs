namespace TodoListApp.Api.Requests.User;

/// <summary>
/// Represents a request to initiate the email change process for the current user.
/// </summary>
/// <param name="NewEmail">The new email address that the user wants to associate with their account.</param>
public record ChangeEmailRequest(string NewEmail);