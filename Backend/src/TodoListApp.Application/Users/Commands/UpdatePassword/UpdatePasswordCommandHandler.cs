using MediatR;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Security;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Users.Commands.UpdatePassword;

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
    : HandlerBase(unitOfWork), IRequestHandler<UpdatePasswordCommand, Result<bool>>
{
    private readonly IPasswordHasher _passwordHasher = passwordHasher;

    /// <summary>
    /// Processes the password update request.
    /// </summary>
    /// <param name="command">The command containing password update details and user identity.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A <see cref="Result{Boolean}"/> indicating success (true) if the password was updated,
    /// or a failure result if the user was not found, the current password is incorrect,
    /// or domain validation failed.
    /// </returns>
    public async Task<Result<bool>> Handle(UpdatePasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await this.UnitOfWork.Users.GetByIdAsync(command.UserId, asNoTracking: false, cancellationToken);

        if (user is null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, "User not found.");
        }

        var isPasswordValid = this._passwordHasher.VerifyPassword(command.CurrentPassword, user.PasswordHash.Value);

        if (!isPasswordValid)
        {
            return await Result<bool>.FailureAsync(ErrorCode.ValidationError, "Incorrect password.");
        }

        var newHashString = this._passwordHasher.HashPassword(command.NewPassword);
        var newPasswordHash = PasswordHash.Create(newHashString);

        user.ChangePassword(newPasswordHash);

        await this.UnitOfWork.SaveChangesAsync(cancellationToken);
        return await Result<bool>.SuccessAsync(true);
    }
}
