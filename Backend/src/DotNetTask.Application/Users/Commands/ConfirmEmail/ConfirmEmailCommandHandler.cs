using DotNetTask.Application.Abstractions.Interfaces.Common;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;

using MediatR;

using TinyResult;

using Unit = DotNetTask.Domain.Common.Unit;

namespace DotNetTask.Application.Users.Commands.ConfirmEmail;

/// <summary>
/// Handles the <see cref="ConfirmEmailCommand"/> to verify a user's email address during registration.
/// </summary>
public class ConfirmEmailCommandHandler(
    IUnitOfWork unitOfWork,
    IClock clock)
    : HandlerBase(unitOfWork), IRequestHandler<ConfirmEmailCommand, Result<Unit>>
{
    private readonly IClock _clock = clock;

    /// <summary>
    /// Processes the email confirmation.
    /// </summary>
    /// <param name="command">The command containing UserId and Token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success result if verified, otherwise a failure.</returns>
    public async Task<Result<Unit>> Handle(ConfirmEmailCommand command, CancellationToken cancellationToken)
    {
        UserEntity? user = await this.UnitOfWork.Users.GetBySecurityTokenAsync(command.Token, Domain.Enums.UserTokenType.EmailVerification, cancellationToken);
        if (user is null)
        {
            return Result<Unit>.Failure(TinyResult.Enums.ErrorCode.NotFound, TokenPolicy.InvalidEmailVerificationTokenMessage);
        }

        Result<Unit> result = user.ConfirmEmailVerification(command.Token, this._clock.UtcNow);
        if (!result.IsSuccess)
        {
            return result;
        }

        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
