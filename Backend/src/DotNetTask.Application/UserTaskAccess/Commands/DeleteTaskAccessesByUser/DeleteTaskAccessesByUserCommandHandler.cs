using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Domain.Constants;

using MediatR;

using TinyResult;

using Unit = DotNetTask.Domain.Common.Unit;

namespace DotNetTask.Application.UserTaskAccess.Commands.DeleteTaskAccessesByUser;

/// <summary>
/// Handles the <see cref="DeleteTaskAccessesByUserCommand"/> to remove all user-task access entries for a specific user.
/// </summary>
public class DeleteTaskAccessesByUserCommandHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<DeleteTaskAccessesByUserCommand, Result<Unit>>
{
    /// <summary>
    /// Processes the command to delete all user-task access entries for a given user.
    /// </summary>
    /// <param name="command">
    /// The command containing the user ID whose access entries should be removed.
    /// </param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating success if the access entries were deleted.
    /// </returns>
    public async Task<Result<Unit>> Handle(DeleteTaskAccessesByUserCommand command, CancellationToken cancellationToken)
    {
        bool exists = await this.UnitOfWork.UserTaskAccesses.ExistsByUserIdAsync(command.UserId, cancellationToken);
        if (!exists)
        {
            return Result<Unit>.Failure(TinyResult.Enums.ErrorCode.InvalidOperation, UserTaskAccessPolicy.NoUserAccessesMessage);
        }

        await this.UnitOfWork.UserTaskAccesses.DeleteAllByUserIdAsync(command.UserId, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
