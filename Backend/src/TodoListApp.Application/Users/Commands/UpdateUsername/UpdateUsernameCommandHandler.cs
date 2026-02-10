using MediatR;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Users.Commands.UpdateUsername;

/// <summary>
/// Handles the <see cref="UpdateUsernameCommand"/> to update the username for current user.
/// </summary>
public class UpdateUsernameCommandHandler(
    IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<UpdateUsernameCommand, Result<bool>>
{
    /// <summary>
    /// Handles updating the username for current user.
    /// </summary>
    /// <param name="command">The <see cref="UpdateUsernameCommand"/> containing the new username.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>A <see cref="Result{Boolean}"/> indicating success or failure of the operation.</returns>
    public async Task<Result<bool>> Handle(UpdateUsernameCommand command, CancellationToken cancellationToken)
    {
        var user = await this.UnitOfWork.Users.GetByIdAsync(command.UserId, asNoTracking: false, cancellationToken);

        if (user is null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, "User not found.");
        }

        var trimmedUsername = command.UserName.Trim();

        if (user.UserName.Value.Equals(trimmedUsername, StringComparison.OrdinalIgnoreCase))
        {
            return await Result<bool>.SuccessAsync(true);
        }

        if (await this.UnitOfWork.Users.ExistsByUserNameAsync(trimmedUsername, cancellationToken))
        {
            return await Result<bool>.FailureAsync(ErrorCode.ValidationError, "Username is already taken.");
        }

        user.ChangeUserName(trimmedUsername);

        await this.UnitOfWork.SaveChangesAsync(cancellationToken);
        return await Result<bool>.SuccessAsync(true);
    }
}
