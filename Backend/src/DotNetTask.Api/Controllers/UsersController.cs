using DotNetTask.Api.Requests.User;
using DotNetTask.Application.Common.Dtos;
using DotNetTask.Application.Users.Commands.ChangeEmail;
using DotNetTask.Application.Users.Commands.ConfirmEmailChange;
using DotNetTask.Application.Users.Commands.ResendEmailVerification;
using DotNetTask.Application.Users.Commands.RevertEmailChange;
using DotNetTask.Application.Users.Commands.UpdatePassword;
using DotNetTask.Application.Users.Commands.UpdateUsername;
using DotNetTask.Application.Users.Commands.UpdateUserProfile;
using DotNetTask.Application.Users.Queries.GetUserProfile;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TinyResult;

namespace DotNetTask.Api.Controllers;

/// <summary>
/// Provides HTTP endpoints for user account management,
/// including registration, login, and password recovery.
/// </summary>
[Authorize]
[Route("api/users")]
public sealed class UsersController(ISender mediator) : BaseController(mediator)
{
    /// <summary>
    /// Returns the profile information of the currently authenticated user.
    /// </summary>
    /// <returns>The user's profile data.</returns>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(UserBriefDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUserProfile()
    {
        GetUserProfileQuery command = new(this.CurrentUserId);
        Result<UserBriefDto> result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleResult(result);
    }

    /// <summary>
    /// Requests a new email verification token for the authenticated user.
    /// </summary>
    /// <remarks>
    /// This endpoint should be used if the initial verification email was not received or the token has expired.
    /// </remarks>
    /// <returns>No content if the request was successfully processed.</returns>
    /// <response code="204">Verification email has been resent.</response>
    /// <response code="400">If the email is already confirmed or the request is invalid.</response>
    [HttpPost("resend-email-verification")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendEmailVerification()
    {
        ResendEmailVerificationCommand command = new(this.CurrentUserId, this.GetThrottlingIdentity());
        Result<bool> result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }

    /// <summary>
    /// Initiates the email change process for the currently authenticated user.
    /// </summary>
    /// <remarks>
    /// A verification link will be sent to the new email address.
    /// The change will not take effect until confirmed via the token.
    /// </remarks>
    /// <param name="request">The request containing the new email address.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the change process was successfully initiated.</response>
    /// <response code="400">If the new email is invalid or already in use.</response>
    [HttpPut("change-email")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailRequest request)
    {
        ChangeEmailCommand command = new(request.NewEmail, this.CurrentUserId);
        Result<bool> result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }

    /// <summary>
    /// Confirms the change of the user's email address using a secure verification token.
    /// </summary>
    /// <param name="token">The secure token received via the new email address to confirm the change.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Email change confirmed and updated successfully.</response>
    /// <response code="400">If the token is invalid, expired, or the user is not found.</response>
    [AllowAnonymous]
    [HttpPost("confirm-email-change")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmailChange([FromQuery] string token)
    {
        ConfirmEmailChangeCommand command = new(token, this.GetThrottlingIdentity());
        Result<bool> result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }

    /// <summary>
    /// Reverts a pending email change request for a user by validating the provided token.
    /// </summary>
    /// <remarks>This action is accessible without authentication and is intended for scenarios where a user
    /// needs to undo a previously requested email change. Ensure that the token is securely generated and transmitted
    /// to prevent unauthorized access.</remarks>
    /// <param name="token">The token that identifies the email change request to be reverted. The token must be valid and not expired.</param>
    /// <returns>An HTTP 200 response containing the unique identifier of the user whose email change was reverted if successful;
    /// otherwise, an error response.</returns>
    [AllowAnonymous]
    [HttpPost("revert-email-change")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RevertEmailChange([FromQuery] string token)
    {
        RevertEmailChangeCommand command = new(token, this.GetThrottlingIdentity());
        Result<string> result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleResult(result);
    }

    /// <summary>
    /// Updates the password for the currently authenticated user.
    /// </summary>
    /// <param name="request">The current and new password details.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Password updated successfully.</response>
    /// <response code="400">If the current password is incorrect.</response>
    [HttpPatch("password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
    {
        UpdatePasswordCommand command = new(request.CurrentPassword, request.NewPassword, this.CurrentUserId);
        Result<bool> result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }

    /// <summary>
    /// Updates the username for the currently authenticated user.
    /// </summary>
    /// <param name="request">The current and new username details.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Username updated successfully.</response>
    /// <response code="400">If the current username is incorrect.</response>
    [HttpPatch("username")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUsername([FromBody] UpdateUsernameRequest request)
    {
        UpdateUsernameCommand command = new(request.NewUsername, this.CurrentUserId);
        Result<bool> result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }

    /// <summary>
    /// Updates the first or last name for the currently authenticated user.
    /// </summary>
    /// <param name="request">The profile data to update.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Profile data updated successfully.</response>
    /// <response code="400">If the current profile datais incorrect.</response>
    [HttpPatch("profile")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserProfileRequest request)
    {
        UpdateUserProfileCommand command = new(request.NewFirstName, request.NewLastName, this.CurrentUserId);
        Result<bool> result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }
}
