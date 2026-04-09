using DotNetTask.Application.Abstractions.Interfaces.Common;
using DotNetTask.Application.Abstractions.Interfaces.Security;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

using MediatR;

using TinyResult;

using Unit = DotNetTask.Domain.Common.Unit;

namespace DotNetTask.Application.Users.Commands.ConfirmPasswordReset;

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
    : HandlerBase(unitOfWork), IRequestHandler<ConfirmPasswordResetCommand, Result<Unit>>
{
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IClock _clock = clock;

    /// <summary>
    /// Processes the password reset confirmation request.
    /// </summary>
    /// <param name="command">The command containing the email, token, and new password.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A <see cref="Result{Unit}"/> indicating that the password was successfully reset,
    /// or a failure result if the token is invalid, expired, or the user is not found.
    /// </returns>
    public async Task<Result<Unit>> Handle(ConfirmPasswordResetCommand command, CancellationToken cancellationToken)
    {
        UserEntity? user = await this.UnitOfWork.Users.GetBySecurityTokenAsync(command.Token, Domain.Enums.UserTokenType.PasswordReset, cancellationToken);

        if (user is null)
        {
            return Result<Unit>.Failure(TinyResult.Enums.ErrorCode.NotFound, TokenPolicy.InvalidPasswordResetTokenMessage);
        }

        string hash = this._passwordHasher.HashPassword(command.NewPassword);
        PasswordHash newPasswordHash = PasswordHash.Create(hash);

        Result<Unit> result = user.ConfirmPasswordReset(newPasswordHash, command.Token, this._clock.UtcNow);
        if (!result.IsSuccess)
        {
            return result;
        }

        await this.UnitOfWork.SaveChangesAsync();
        return Result<Unit>.Success(Unit.Value);
    }
}
