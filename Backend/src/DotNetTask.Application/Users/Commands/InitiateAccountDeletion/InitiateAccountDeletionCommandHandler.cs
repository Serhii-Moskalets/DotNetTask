using DotNetTask.Application.Abstractions.Interfaces.Common;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using MediatR;
using TinyResult;

using Unit = DotNetTask.Domain.Common.Unit;

namespace DotNetTask.Application.Users.Commands.InitiateAccountDeletion;

/// <summary>
/// Handles the <see cref="InitiateAccountDeletionCommand"/> to start the account removal process.
/// </summary>
public class InitiateAccountDeletionCommandHandler(IUnitOfWork unitOfWork, IClock clock)
    : HandlerBase(unitOfWork), IRequestHandler<InitiateAccountDeletionCommand, Result<Unit>>
{
    private readonly IClock _clock = clock;

    /// <summary>
    /// Processes the account deletion initiation request.
    /// </summary>
    /// <param name="command">The command containing the ID of the user requesting deletion.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Result{Unit}"/> indicating whether the deletion process was successfully initiated.
    /// Returns a failure if the user is not found or the account state prohibits deletion.
    /// </returns>
    public async Task<Result<Unit>> Handle(InitiateAccountDeletionCommand command, CancellationToken cancellationToken)
    {
        UserEntity? user = await this.UnitOfWork.Users.GetByIdAsync(command.UserId, asNoTracking: false, cancellationToken);
        if (user is null)
        {
            return Result<Unit>.Failure(TinyResult.Enums.ErrorCode.NotFound, UserPolicy.AccountNotFoundMessage);
        }

        Result<Unit> result = user.RequestAccountDeletion(this._clock.UtcNow);
        if (!result.IsSuccess)
        {
            return result;
        }

        await this.UnitOfWork.SaveChangesAsync();

        return Result<Unit>.Success(Unit.Value);
    }
}
