using DotNetTask.Application.Abstractions.Interfaces.Common;
using DotNetTask.Application.Abstractions.Interfaces.Security;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

using MediatR;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Users.Commands.ChangeEmail;

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
        Email newEmail = Email.Create(command.NewEmail);

        UserEntity? user = await this.UnitOfWork.Users.GetByIdAsync(command.UserId, asNoTracking: false, cancellationToken);
        if (user is null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, UserPolicy.AccountNotFoundMessage);
        }

        if (user.Email == newEmail)
        {
            return await Result<bool>.FailureAsync(ErrorCode.ValidationError, EmailPolicy.SameAsCurrentMessage);
        }

        bool emailExist = await this.UnitOfWork.Users.ExistsByEmailAsync(newEmail, cancellationToken);
        if (emailExist)
        {
            return await Result<bool>.FailureAsync(ErrorCode.InvalidOperation, EmailPolicy.AlreadyInUseMessage);
        }

        string confirmationToken = this._tokenGenerator.GenerateSecureToken();
        string revertToken = this._tokenGenerator.GenerateSecureToken();
        DateTime now = this._clock.UtcNow;

        user.RequestEmailChange(newEmail, confirmationToken, revertToken, TimeSpan.FromHours(1), now);

        await this.UnitOfWork.SaveChangesAsync(cancellationToken);
        return await Result<bool>.SuccessAsync(true);
    }
}
