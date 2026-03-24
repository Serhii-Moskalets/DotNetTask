using MediatR;
using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.Common;
using TodoListApp.Application.Abstractions.Interfaces.Security;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.Constants;
using TodoListApp.Domain.Constants.Settings;

namespace TodoListApp.Application.Users.Commands.ResendEmailVerification;

/// <summary>
/// Handles the process of resending a verification email to a user.
/// </summary>
/// <remarks>
/// This handler retrieves the user, generates a new secure token using <see cref="ITokenGenerator"/>,
/// updates the user's state via the domain model, and persists changes through the unit of work.
/// </remarks>
/// <param name="unitOfWork">The unit of work used to access user data and persist changes.</param>
/// <param name="tokenGenerator">The service responsible for creating secure, unique tokens.</param>
/// <param name="tokenSettings">Configuration settings defining token lifespan and constraints.</param>
/// <param name="clock">The system clock abstraction for provider-independent time access.</param>
public class ResendEmailVerificationCommandHandler(
    IUnitOfWork unitOfWork,
    ITokenGenerator tokenGenerator,
    TokenSettings tokenSettings,
    IClock clock) : HandlerBase(unitOfWork), IRequestHandler<ResendEmailVerificationCommand, Result<bool>>
{
    private readonly ITokenGenerator _tokenGenerator = tokenGenerator;
    private readonly TokenSettings _tokenSettings = tokenSettings;
    private readonly IClock _clock = clock;

    /// <summary>
    /// Processes the resend email verification command.
    /// </summary>
    /// <param name="request">The command containing the ID of the user requesting a new verification email.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing <c>true</c> if the token was successfully regenerated
    /// and the domain event was dispatched; otherwise, a failure result with a specific error code.
    /// </returns>
    public async Task<Result<bool>> Handle(ResendEmailVerificationCommand request, CancellationToken cancellationToken)
    {
        var user = await this.UnitOfWork.Users.GetByIdAsync(request.UserId, asNoTracking: false, cancellationToken);

        if (user is null)
        {
            return Result<bool>.Failure(
                TinyResult.Enums.ErrorCode.NotFound,
                UserPolicy.AccountNotFoundMessage);
        }

        var secureToken = this._tokenGenerator.GenerateSecureToken();
        var now = this._clock.UtcNow;

        var result = user.ResendEmailVerification(
            secureToken,
            this._tokenSettings.EmailVerificationTokenDuration,
            now);

        if (!result.IsSuccess)
        {
            return result;
        }

        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
