using DotNetTask.Application.Abstractions.Interfaces.Security;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

using MediatR;

using TinyResult;
using TinyResult.Enums;

using Unit = DotNetTask.Domain.Common.Unit;

namespace DotNetTask.Application.Users.Commands.UpdatePassword;

/// <summary>
/// Handles the <see cref="UpdatePasswordCommand"/> to change the user's password.
/// </summary>
/// <remarks>
/// This handler verifies the current password using <see cref="IPasswordHasher"/>,
/// generates a new hash for the provided password, and updates the user entity
/// ensuring that domain rules are respected.
/// </remarks>
public class UpdatePasswordCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher)
    : HandlerBase(unitOfWork), IRequestHandler<UpdatePasswordCommand, Result<Unit>>
{
    private readonly IPasswordHasher _passwordHasher = passwordHasher;

    /// <summary>
    /// Processes the password update request.
    /// </summary>
    /// <param name="command">The command containing password update details and user identity.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A <see cref="Result{Unit}"/> indicating that the password was successfully updated,
    /// or a failure result if the current password is incorrect, the user is not found, or validation fails.
    /// </returns>
    public async Task<Result<Unit>> Handle(UpdatePasswordCommand command, CancellationToken cancellationToken)
    {
        UserEntity? user = await this.UnitOfWork.Users.GetByIdAsync(command.UserId, asNoTracking: false, cancellationToken);

        if (user is null)
        {
            return Result<Unit>.Failure(ErrorCode.NotFound, UserPolicy.AccountNotFoundMessage);
        }

        bool isPasswordValid = this._passwordHasher.VerifyPassword(command.CurrentPassword, user.PasswordHash.Value);

        if (!isPasswordValid)
        {
            return Result<Unit>.Failure(ErrorCode.ValidationError, PasswordPolicy.IncorrectMessage);
        }

        string newHashString = this._passwordHasher.HashPassword(command.NewPassword);
        PasswordHash newPasswordHash = PasswordHash.Create(newHashString);

        user.ChangePassword(newPasswordHash);

        await this.UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}
