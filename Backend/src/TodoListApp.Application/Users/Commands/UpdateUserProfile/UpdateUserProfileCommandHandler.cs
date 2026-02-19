using MediatR;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Users.Commands.UpdateUserProfile;

/// <summary>
/// Handles the <see cref="UpdateUserProfileCommand"/> to update the user profile for current user.
/// </summary>
public class UpdateUserProfileCommandHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<UpdateUserProfileCommand, Result<bool>>
{
    /// <summary>
    /// Handles updating the user profile for current user.
    /// </summary>
    /// <param name="command">The <see cref="UpdateUserProfileCommand"/> containing the new user profile.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>A <see cref="Result{Boolean}"/> indicating success or failure of the operation.</returns>
    public async Task<Result<bool>> Handle(UpdateUserProfileCommand command, CancellationToken cancellationToken)
    {
        var user = await this.UnitOfWork.Users.GetByIdAsync(command.UserId, asNoTracking: false, cancellationToken);

        if (user is null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, "User not found.");
        }

        var isChanged = false;

        if (command.FirstName is not null)
        {
            isChanged |= user.ChangeFirstName(FirstName.Create(command.FirstName));
        }

        if (command.LastName is not null)
        {
            isChanged |= user.ChangeLastName(LastName.Create(command.LastName));
        }

        if (!isChanged)
        {
            return await Result<bool>.FailureAsync(ErrorCode.InvalidOperation, "No changes detected. Names are the same as current.");
        }

        await this.UnitOfWork.SaveChangesAsync(cancellationToken);
        return await Result<bool>.SuccessAsync(true);
    }
}
