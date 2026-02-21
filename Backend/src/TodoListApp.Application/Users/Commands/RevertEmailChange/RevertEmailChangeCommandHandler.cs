using MediatR;
using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Users.Commands.RevertEmailChange;

/// <summary>
/// Handles the <see cref="RevertEmailChangeCommand"/> to restore a user's original email address.
/// </summary>
/// <remarks>
/// This handler retrieves the user, invokes the domain logic to revert the email,
/// updates security credentials (must change password, security stamp),
/// and persists changes via the unit of work.
/// </remarks>
public class RevertEmailChangeCommandHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<RevertEmailChangeCommand, Result<bool>>
{
    /// <summary>
    /// Processes the revert request.
    /// </summary>
    /// <param name="command">The revert command containing user ID and security token.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="Result{T}"/> indicating success or failure (e.g., if user not found).</returns>
    public async Task<Result<bool>> Handle(RevertEmailChangeCommand command, CancellationToken cancellationToken)
    {
        var user = await this.UnitOfWork.Users.GetByIdAsync(command.UserId, asNoTracking: false, cancellationToken);
        if (user is null)
        {
            return await Result<bool>.FailureAsync(TinyResult.Enums.ErrorCode.NotFound, "User not found.");
        }

        user.RevertEmailChange(command.Token, DateTime.UtcNow);

        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
