using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

using MediatR;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Users.Commands.UpdateUsername;

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
        UserName newUserName = UserName.Create(command.UserName);

        UserEntity? user = await this.UnitOfWork.Users.GetByIdAsync(command.UserId, asNoTracking: false, cancellationToken);

        if (user is null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, UserPolicy.AccountNotFoundMessage);
        }

        if (newUserName == user.UserName)
        {
            return await Result<bool>.FailureAsync(ErrorCode.ValidationError, UserNamePolicy.SameAsCurrentMessage);
        }

        if (await this.UnitOfWork.Users.ExistsByUserNameAsync(newUserName, cancellationToken))
        {
            return await Result<bool>.FailureAsync(ErrorCode.InvalidOperation, UserNamePolicy.AlreadyInUseMessage);
        }

        user.ChangeUserName(newUserName);

        await this.UnitOfWork.SaveChangesAsync(cancellationToken);
        return await Result<bool>.SuccessAsync(true);
    }
}
