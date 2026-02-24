using System;

namespace TodoListApp.Api.Requests.Auth;

/// <summary>
/// Represents a request to confirm a password reset using a secure token.
/// </summary>
/// <param name="UserId">The unique identifier of the user resetting the password.</param>
/// <param name="NewPassword">The new password chosen by the user.</param>
/// <param name="Token">The secure reset token received via email.</param>
public record ConfirmPasswordResetRequest(
    Guid UserId,
    string NewPassword,
    string Token);
