using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Api.Requests.Auth;
using TodoListApp.Application.Users.Commands.ConfirmEmail;
using TodoListApp.Application.Users.Commands.ConfirmPasswordReset;
using TodoListApp.Application.Users.Commands.LoginUser;
using TodoListApp.Application.Users.Commands.RegisterUser;
using TodoListApp.Application.Users.Commands.ResetPassword;

namespace TodoListApp.Api.Controllers;

/// <summary>
/// Provides HTTP endpoints for authentication and user account management,
/// including registration, login, and password recovery.
/// </summary>
[AllowAnonymous]
[Route("api/auth")]
public sealed class AuthController(ISender mediator) : BaseController(mediator)
{
    /// <summary>
    /// Registers a new user in the system.
    /// </summary>
    /// <param name="request">The registration details.</param>
    /// <returns>The unique identifier of the newly created user.</returns>
    /// <response code="200">Returns the ID of the new user.</response>
    /// <response code="400">If the registration data is invalid or the user already exists.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterUserCommand(
            request.FirstName,
            request.LastName,
            request.UserName,
            request.Email,
            request.Password);
        var result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleResult(result);
    }

    /// <summary>
    /// Confirms the user's email address using a secure verification token.
    /// </summary>
    /// <param name="token">The secure token received via email for confirmation.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Email confirmed successfully.</response>
    /// <response code="400">If the token is invalid, expired, or the user is not found.</response>
    [HttpPost("confirm-email")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
    {
        var command = new ConfirmEmailCommand(token);
        var result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }

    /// <summary>
    /// Authenticates a user and returns an access token.
    /// </summary>
    /// <param name="request">The login credentials (email and password).</param>
    /// <returns>An authentication result containing the JWT token.</returns>
    /// <response code="200">Returns the authentication token.</response>
    /// <response code="401">If credentials are invalid.</response>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginUserCommand(request.Email, request.Password);
        var result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleResult(result);
    }

    /// <summary>
    /// Initiates the password recovery process by sending a reset link to the user's email.
    /// </summary>
    /// <param name="request">The email address of the account.</param>
    /// <returns>No content.</returns>
    /// <response code="204">If the request was processed (always returns 204 for security reasons).</response>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var command = new ResetPasswordCommand(request.Email);
        var result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }

    /// <summary>
    /// Resets the user's password using a valid secure token.
    /// </summary>
    /// <param name="request">The reset details, including the token and the new password.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Password reset successfully.</response>
    /// <response code="400">If the token is invalid or expired.</response>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ConfirmPasswordResetRequest request)
    {
        var command = new ConfirmPasswordResetCommand(request.NewPassword, request.Token);
        var result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }
}
