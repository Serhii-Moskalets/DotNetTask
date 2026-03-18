using MediatR;
using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.Common;
using TodoListApp.Application.Abstractions.Interfaces.Security;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.Constants;
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
    IPasswordHasher passwordHasher,
    IClock clock)
    : HandlerBase(unitOfWork), IRequestHandler<ConfirmPasswordResetCommand, Result<bool>>
{
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IClock _clock = clock;

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
        var user = await this.UnitOfWork.Users.GetBySecurityTokenAsync(command.Token, Domain.Enums.UserTokenType.PasswordReset, cancellationToken);

        if (user is null)
        {
            return await Result<bool>.FailureAsync(TinyResult.Enums.ErrorCode.NotFound, TokenPolicy.InvalidPasswordResetTokenMessage);
        }

        var hash = this._passwordHasher.HashPassword(command.NewPassword);
        var newPasswordHash = PasswordHash.Create(hash);

        var result = user.ConfirmPasswordReset(newPasswordHash, command.Token, this._clock.UtcNow);
        if (!result.IsSuccess)
        {
            return result;
        }

        await this.UnitOfWork.SaveChangesAsync();
        return Result<bool>.Success(true);
    }
}
