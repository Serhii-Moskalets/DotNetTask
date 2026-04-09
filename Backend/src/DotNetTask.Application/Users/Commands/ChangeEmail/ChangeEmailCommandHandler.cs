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

using Unit = DotNetTask.Domain.Common.Unit;

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
    : HandlerBase(unitOfWork), IRequestHandler<ChangeEmailCommand, Result<Unit>>
{
    private readonly ITokenGenerator _tokenGenerator = tokenGenerator;
    private readonly IClock _clock = clock;

    /// <summary>
    /// Processes the request to change a user's email address.
    /// </summary>
    /// <param name="command">The command containing the user ID and the new email address.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A <see cref="Result{Unit}"/> indicating that the request was successfully created,
    /// or a failure result if validation fails or the user is not found.
    /// </returns>
    public async Task<Result<Unit>> Handle(ChangeEmailCommand command, CancellationToken cancellationToken)
    {
        Email newEmail = Email.Create(command.NewEmail);

        UserEntity? user = await this.UnitOfWork.Users.GetByIdAsync(command.UserId, asNoTracking: false, cancellationToken);
        if (user is null)
        {
            return Result<Unit>.Failure(ErrorCode.NotFound, UserPolicy.AccountNotFoundMessage);
        }

        if (user.Email == newEmail)
        {
            return Result<Unit>.Failure(ErrorCode.ValidationError, EmailPolicy.SameAsCurrentMessage);
        }

        bool emailExist = await this.UnitOfWork.Users.ExistsByEmailAsync(newEmail, cancellationToken);
        if (emailExist)
        {
            return Result<Unit>.Failure(ErrorCode.InvalidOperation, EmailPolicy.AlreadyInUseMessage);
        }

        string confirmationToken = this._tokenGenerator.GenerateSecureToken();
        string revertToken = this._tokenGenerator.GenerateSecureToken();
        DateTime now = this._clock.UtcNow;

        user.RequestEmailChange(newEmail, confirmationToken, revertToken, TimeSpan.FromHours(1), now);

        await this.UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}
