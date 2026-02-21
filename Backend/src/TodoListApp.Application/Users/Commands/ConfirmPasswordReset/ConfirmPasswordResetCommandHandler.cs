using MediatR;
using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.Security;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Users.Commands.ConfirmPasswordReset;

/// <summary>
/// Handles the <see cref="ConfirmPasswordResetCommand"/> to update a user's password using a reset token.
/// </summary>
/// <remarks>
/// This handler verifies the existence of the user by email, hashes the new password,
/// and delegates the token validation and password update logic to the domain entity.
/// </remarks>
public class ConfirmPasswordResetCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher)
    : HandlerBase(unitOfWork), IRequestHandler<ConfirmPasswordResetCommand, Result<bool>>
{
    private readonly IPasswordHasher _passwordHasher = passwordHasher;

    /// <summary>
    /// Processes the password reset confirmation request.
    /// </summary>
    /// <param name="command">The command containing the email, token, and new password.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A <see cref="Result{Boolean}"/> indicating success (true) if the password was reset,
    /// or a failure result if the user was not found or the token is invalid/expired.
    /// </returns>
    public async Task<Result<bool>> Handle(ConfirmPasswordResetCommand command, CancellationToken cancellationToken)
    {
        var user = await this.UnitOfWork.Users.GetByIdAsync(command.UserId, asNoTracking: false, cancellationToken);

        if (user is null)
        {
            return await Result<bool>.FailureAsync(TinyResult.Enums.ErrorCode.InvalidOperation, "...");
        }

        var hash = this._passwordHasher.HashPassword(command.NewPassword);
        var newPasswordHash = PasswordHash.Create(hash);

        user.ConfirmPasswordReset(newPasswordHash, command.Token, DateTime.UtcNow);

        await this.UnitOfWork.SaveChangesAsync();
        return await Result<bool>.SuccessAsync(true);
    }
}
