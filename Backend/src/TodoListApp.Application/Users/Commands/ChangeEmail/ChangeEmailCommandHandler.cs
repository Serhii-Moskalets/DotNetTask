using MediatR;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Common;
using TodoListApp.Application.Abstractions.Interfaces.Security;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.Constants;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Users.Commands.ChangeEmail;

/// <summary>
/// Handles the <see cref="ChangeEmailCommand"/> to initiate the email change process for a user.
/// </summary>
/// <remarks>
/// This handler retrieves the user by their unique identifier, generates a secure verification token,
/// and updates the user entity with a pending email change request. The actual change is finalized
/// only after the new email is confirmed.
/// </remarks>
public class ChangeEmailCommandHandler(
    IUnitOfWork unitOfWork,
    ITokenGenerator tokenGenerator,
    IClock clock)
    : HandlerBase(unitOfWork), IRequestHandler<ChangeEmailCommand, Result<bool>>
{
    private readonly ITokenGenerator _tokenGenerator = tokenGenerator;
    private readonly IClock _clock = clock;

    /// <summary>
    /// Processes the request to change a user's email address.
    /// </summary>
    /// <param name="command">The command containing the user ID and the new email address.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A <see cref="Result{Boolean}"/> indicating success (true) if the email change was initiated,
    /// or a failure result if the user was not found or the email is already in use.
    /// </returns>
    public async Task<Result<bool>> Handle(ChangeEmailCommand command, CancellationToken cancellationToken)
    {
        var newEmail = Email.Create(command.NewEmail);

        var user = await this.UnitOfWork.Users.GetByIdAsync(command.UserId, asNoTracking: false, cancellationToken);
        if (user is null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, UserPolicy.AccountNotFoundMessage);
        }

        if (user.Email == newEmail)
        {
            return await Result<bool>.FailureAsync(ErrorCode.ValidationError, EmailPolicy.SameAsCurrentMessage);
        }

        var emailExist = await this.UnitOfWork.Users.ExistsByEmailAsync(newEmail, cancellationToken);
        if (emailExist)
        {
            return await Result<bool>.FailureAsync(ErrorCode.InvalidOperation, EmailPolicy.AlreadyInUseMessage);
        }

        var confirmationToken = this._tokenGenerator.GenerateSecureToken();
        var revertToken = this._tokenGenerator.GenerateSecureToken();
        var now = this._clock.UtcNow;

        user.RequestEmailChange(newEmail, confirmationToken, revertToken, TimeSpan.FromHours(1), now);

        await this.UnitOfWork.SaveChangesAsync(cancellationToken);
        return await Result<bool>.SuccessAsync(true);
    }
}
