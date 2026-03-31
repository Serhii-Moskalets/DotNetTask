using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using DotNetTask.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Api.Controllers;

/// <summary>
/// Base controller that provides common properties and methods
/// for API controllers, such as retrieving the current user's ID and MediatR sender.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    private Guid? _currentUserId;

    private IWebHostEnvironment? _environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseController"/> class.
    /// </summary>
    /// <param name="mediator">The Mediatr sender for dispatching commands and queries.</param>
    protected BaseController(ISender mediator) => this.Mediator = mediator;

    /// <summary>
    /// Gets the Mediatr sender for dispatching commands and queries.
    /// </summary>
    protected ISender Mediator { get; }

    /// <summary>
    /// Gets the unique identifier of the currently authenticated user.
    /// </summary>
    /// <remarks>
    /// This property uses lazy initialization to retrieve and cache the user ID from the claims
    /// for the duration of the current request.
    /// </remarks>
    protected Guid CurrentUserId => this._currentUserId ??= this.GetCurrentUserId();

    /// <summary>
    /// Gets the current user's unique identifier if authenticated, otherwise null.
    /// </summary>
    /// <remarks>
    /// This property safely attempts to retrieve the user ID without throwing an exception.
    /// </remarks>
    protected Guid? CurrentUserIdOrNull
    {
        get
        {
            try
            {
                return this.CurrentUserId;
            }
            catch
            {
                return null;
            }
        }
    }

    private IWebHostEnvironment Environment => this._environment ??= this.HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();

    /// <summary>
    /// Retrieves the unique identifier of the currently authenticated user from the JWT claims.
    /// </summary>
    /// <returns>The <see cref="Guid"/> of the current user.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user identity is missing or invalid.</exception>
    protected Guid GetCurrentUserId()
    {
        Claim? userIdClaim = this.User.FindFirst(ClaimTypes.NameIdentifier)
            ?? this.User.FindFirst(JwtRegisteredClaimNames.Sub);

        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
        {
            throw new UnauthorizedAccessException(CommonPolicy.InvalidUserIdentityMessage);
        }

        return userId;
    }

    /// <summary>
    /// Handles the <see cref="Result{T}"/> by returning an <see cref="OkObjectResult"/> on success
    /// or a formatted problem response on failure.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the result.</typeparam>
    /// <param name="result">The result object to handle.</param>
    /// <returns>An <see cref="IActionResult"/> representing the response.</returns>
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return this.Ok(result.Value);
        }

        return this.CreateProblemDetails(result.Error!);
    }

    /// <summary>
    /// Handles a <see cref="Result{Boolean}"/> by returning <see cref="NoContentResult"/> on success
    /// or a formatted problem response on failure.
    /// </summary>
    /// <param name="result">The result object to handle.</param>
    /// <returns>An <see cref="IActionResult"/> representing the response.</returns>
    protected IActionResult HandleNoContent(Result<bool> result)
    {
        if (result.IsSuccess)
        {
            return this.NoContent();
        }

        return this.CreateProblemDetails(result.Error!);
    }

    /// <summary>
    /// Retrieves the client's IP address from the current connection, or returns <see cref="IPAddress.None"/> if unavailable.
    /// </summary>
    /// <returns>The <see cref="IPAddress"/> of the client, or <see cref="IPAddress.None"/> if it cannot be determined.</returns>
    protected IPAddress GetClientIpOrUnknown() => this.HttpContext.Connection.RemoteIpAddress ?? IPAddress.None;

    /// <summary>
    /// Generates a unique identity string used for throttling rate-limits.
    /// </summary>
    /// <remarks>
    /// In Development, combines IP and user ID (if available).
    /// In Production, uses only the IP for anonymous endpoints.
    /// </remarks>
    /// <returns>A string representing the throttling identity, combining IP and optionally user ID in development.</returns>
    protected string GetThrottlingIdentity() => this.GetClientIpOrUnknown().ToString();

    /// <summary>
    /// Creates a standardized <see cref="ProblemDetails"/> response based on the provided error.
    /// </summary>
    /// <param name="error">The error details used to populate the problem response.</param>
    /// <returns>An <see cref="ObjectResult"/> containing the problem details.</returns>
    /// <remarks>
    /// Maps internal error codes from <see cref="TinyResult"/> to appropriate HTTP status codes.
    /// </remarks>
    private ObjectResult CreateProblemDetails(Error error)
    {
        int statusCode = error.Code switch
        {
            ErrorCode.ValidationError => StatusCodes.Status400BadRequest,
            ErrorCode.NotFound => StatusCodes.Status404NotFound,
            ErrorCode.InvalidOperation => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError,
        };

        ProblemDetails problemDetails = new()
        {
            Status = statusCode,
            Title = error.Code.ToString(),
            Detail = error.Message,
            Instance = this.HttpContext.TraceIdentifier,
        };

        return this.StatusCode(statusCode, problemDetails);
    }
}
