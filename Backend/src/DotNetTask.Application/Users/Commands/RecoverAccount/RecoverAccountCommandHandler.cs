using DotNetTask.Application.Abstractions.Interfaces.Common;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using MediatR;
using TinyResult;

using Unit = DotNetTask.Domain.Common.Unit;

namespace DotNetTask.Application.Users.Commands.RecoverAccount;

/// <summary>
/// Handles the <see cref="RecoverAccountCommand"/> to restore a user account from a pending deletion state.
/// </summary>
public class RecoverAccountCommandHandler(IUnitOfWork unitOfWork, IClock clock)
    : HandlerBase(unitOfWork), IRequestHandler<RecoverAccountCommand, Result<Unit>>
{
    private readonly IClock _clock = clock;

    /// <summary>
    /// Processes the account recovery request.
    /// </summary>
    /// <param name="command">The command containing the ID of the user whose account is to be recovered.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Result{Unit}"/> indicating success if the account was restored,
    /// or a failure result if the user was not found or the recovery period has expired.
    /// </returns>
    public async Task<Result<Unit>> Handle(RecoverAccountCommand command, CancellationToken cancellationToken)
    {
        UserEntity? user = await this.UnitOfWork.Users.GetByIdAsync(command.UserId, asNoTracking: false, cancellationToken);
        if (user is null)
        {
            return Result<Unit>.Failure(TinyResult.Enums.ErrorCode.NotFound, UserPolicy.AccountNotFoundMessage);
        }

        Result<Unit> result = user.RecoverAccount(this._clock.UtcNow);
        if (!result.IsSuccess)
        {
            return result;
        }

        await this.UnitOfWork.SaveChangesAsync();

        return Result<Unit>.Success(Unit.Value);
    }
}
