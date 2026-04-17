using DotNetTask.Application.Abstractions.Interfaces.Common;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using MediatR;
using TinyResult;

namespace DotNetTask.Application.Users.Commands.PurgeExpiredAccounts;

/// <summary>
/// Handles the <see cref="PurgeExpiredAccountsCommand"/> to permanently remove
/// users whose deletion grace period has expired.
/// </summary>
/// <remarks>
/// This process is non-reversible. It identifies users in 'PendingDeletion'
/// whose scheduled date is in the past.
/// </remarks>
public class PurgeExpiredAccountsCommandHandler(IUnitOfWork unitOfWork, IClock clock)
    : HandlerBase(unitOfWork), IRequestHandler<PurgeExpiredAccountsCommand, Result<int>>
{
    /// <inheritdoc/>
    public async Task<Result<int>> Handle(PurgeExpiredAccountsCommand command, CancellationToken cancellationToken)
    {
        IReadOnlyList<Guid> usersIdsToDelete = await this.UnitOfWork.Users.GetPendingDeletionAsync(clock.UtcNow, cancellationToken);
        if (usersIdsToDelete.Count == 0)
        {
            return Result<int>.Success(0);
        }

        await this.UnitOfWork.Users.DeleteRangeAsync(usersIdsToDelete, cancellationToken);
        int deleted = await this.UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(deleted);

    }
}
