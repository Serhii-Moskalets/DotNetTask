using System;

namespace TodoListApp.Api.Requests.Auth;

/// <summary>
/// Represents a request to confirm a email verification using a secure token.
/// </summary>
/// <param name="UserId">The unique identifier of the user confirmation the email.</param>
/// <param name="Token">The secure confirm token received via email.</param>
public record ConfirmEmailVerificationRequest(Guid UserId, string Token);