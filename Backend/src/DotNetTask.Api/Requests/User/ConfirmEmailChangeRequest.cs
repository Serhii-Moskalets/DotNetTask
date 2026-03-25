namespace DotNetTask.Api.Requests.User;

/// <summary>
/// Represents a request to confirm a user's email address change using a verification token.
/// </summary>
/// <param name="Token">The token used to verify the email change request. This value must be a valid, non-expired token issued for the
/// user.</param>
public record ConfirmEmailChangeRequest(string Token);
