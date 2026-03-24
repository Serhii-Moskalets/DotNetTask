using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TodoListApp.Application.Abstractions.Interfaces.Security;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Domain.Constants;
using TodoListApp.Domain.Constants.Settings;
using TodoListApp.Domain.Exceptions;

namespace TodoListApp.Api.Middleware;

/// <summary>
/// Middleware responsible for verifying the user's security state on every authenticated request.
/// </summary>
/// <remarks>
/// This middleware checks if the session is still valid by comparing the security stamp from the JWT
/// with the one in the database. It also enforces mandatory password changes by restricting access
/// to non-auth resources when the <c>MustChangePassword</c> flag is set.
/// </remarks>
/// <param name="next">The next delegate in the HTTP request pipeline.</param>
/// <param name="authSettings">The authorization settings.</param>
/// <param name="userSettings">The users settings.</param>
public class UserSecurityMiddleware(RequestDelegate next, AuthSettings authSettings, UserSettings userSettings)
{
    /// <summary>
    /// Invokes the security verification logic for the current request.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
    /// <param name="unitOfWork">The unit of work providing access to user security data.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the token is invalid, the user is missing, or the session has expired.</exception>
    /// <exception cref="PasswordChangeRequiredException">Thrown when a password change is required but the user attempts to access other resources.</exception>
    public async Task InvokeAsync(HttpContext context, IUnitOfWork unitOfWork)
    {
        if (context.Request.Path.StartsWithSegments("/api/auth", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated is true)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)
                    ?? context.User.FindFirst(JwtRegisteredClaimNames.Sub);

            var tokenStamp = context.User.FindFirst(CustomClaims.SecurityStamp)?.Value;

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId) || tokenStamp is null)
            {
                throw new UnauthorizedAccessException(TokenPolicy.InvalidUserIdentityMessage);
            }

            var (securityStamp, mustChangePassword, isEmailConfirmed) = await unitOfWork.Users.GetUsersSecurityInfoAsync(userId, context.RequestAborted)
                ?? throw new UnauthorizedAccessException(UserPolicy.AccountNotFoundMessage);

            if (securityStamp != tokenStamp)
            {
                throw new UnauthorizedAccessException(UserPolicy.SessionExpiredMessage);
            }

            if (!isEmailConfirmed)
            {
                bool isResendEmailEndpoint = context.Request.Path.StartsWithSegments(userSettings.ResendEmailVerificationEndpoint, StringComparison.OrdinalIgnoreCase);

                if (!isResendEmailEndpoint)
                {
                    throw new EmailResendVerificationException();
                }
            }

            if (mustChangePassword)
            {
                bool isChangePasswordEndpoint = context.Request.Path.StartsWithSegments(authSettings.ResetPasswordEndpoint, StringComparison.OrdinalIgnoreCase);

                if (!isChangePasswordEndpoint)
                {
                    throw new PasswordChangeRequiredException();
                }
            }
        }

        await next(context);
    }
}
