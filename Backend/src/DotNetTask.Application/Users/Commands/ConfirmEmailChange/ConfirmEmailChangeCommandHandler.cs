using DotNetTask.Application.Abstractions.Interfaces.Common;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;

using MediatR;

using TinyResult;

using Unit = DotNetTask.Domain.Common.Unit;

namespace DotNetTask.Application.Users.Commands.ConfirmEmailChange;

/// <summary>
/// Handles the <see cref="ConfirmEmailChangeCommand"/> to finalize a pending email change request.
/// </summary>
/// <remarks>
/// This handler validates the provided token against the user's stored security token.
/// If valid, it updates the user's email address and clears the temporary token.
/// </remarks>
public class ConfirmEmailChangeCommandHandler(
    IUnitOfWork unitOfWork,
    IClock clock)
    : HandlerBase(unitOfWork), IRequestHandler<ConfirmEmailChangeCommand, Result<Unit>>
{
    private readonly IClock _clock = clock;

    /// <summary>
    /// Processes the confirmation of a user's email change.
    /// </summary>
    /// <param name="command">The command containing the user ID and the verification token.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A <see cref="Result{Unit}"/> indicating that the email change was successfully applied,
    /// or a failure result if the token is invalid, expired, or the associated user is not found.
    /// </returns>
    public async Task<Result<Unit>> Handle(ConfirmEmailChangeCommand command, CancellationToken cancellationToken)
    {
        UserEntity? user = await this.UnitOfWork.Users.GetBySecurityTokenAsync(command.Token, Domain.Enums.UserTokenType.EmailChange, cancellationToken);
        if (user is null)
        {
            return Result<Unit>.Failure(TinyResult.Enums.ErrorCode.NotFound, TokenPolicy.InvalidEmailChangeTokenMessage);
        }

        Result<Unit> result = user.ConfirmEmailChange(command.Token, this._clock.UtcNow);
        if (!result.IsSuccess)
        {
            return result;
        }

        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
