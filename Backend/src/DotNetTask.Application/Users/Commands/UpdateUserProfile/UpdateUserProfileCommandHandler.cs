using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

using MediatR;

using TinyResult;
using TinyResult.Enums;

using Unit = DotNetTask.Domain.Common.Unit;

namespace DotNetTask.Application.Users.Commands.UpdateUserProfile;

/// <summary>
/// Handles the <see cref="UpdateUserProfileCommand"/> to update the user profile for current user.
/// </summary>
public class UpdateUserProfileCommandHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<UpdateUserProfileCommand, Result<Unit>>
{
    /// <summary>
    /// Handles updating the user profile for current user.
    /// </summary>
    /// <param name="command">The <see cref="UpdateUserProfileCommand"/> containing the new user profile.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A <see cref="Result{Unit}"/> indicating that the profile was successfully updated,
    /// or a failure result if the user was not found or no changes were detected.
    /// </returns>
    public async Task<Result<Unit>> Handle(UpdateUserProfileCommand command, CancellationToken cancellationToken)
    {
        UserEntity? user = await this.UnitOfWork.Users.GetByIdAsync(command.UserId, asNoTracking: false, cancellationToken);

        if (user is null)
        {
            return Result<Unit>.Failure(ErrorCode.NotFound, UserPolicy.AccountNotFoundMessage);
        }

        bool isChanged = false;

        if (command.FirstName is not null)
        {
            Result<bool> result = user.ChangeFirstName(FirstName.Create(command.FirstName));
            if (result.IsFailure)
            {
                return Result<Unit>.Failure(result.Error!.Code, result.Error.Message);
            }

            isChanged |= result.Value;
        }

        if (command.LastName is not null)
        {
            Result<bool> result = user.ChangeLastName(LastName.Create(command.LastName));
            if (result.IsFailure)
            {
                return Result<Unit>.Failure(result.Error!.Code, result.Error.Message);
            }

            isChanged |= result.Value;
        }

        if (!isChanged)
        {
            return Result<Unit>.Failure(ErrorCode.InvalidOperation, UserPolicy.NoChangesDetectedMessage);
        }

        await this.UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}
