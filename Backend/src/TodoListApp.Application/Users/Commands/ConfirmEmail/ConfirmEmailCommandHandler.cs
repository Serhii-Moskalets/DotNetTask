using MediatR;
using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Users.Commands.ConfirmEmail;

/// <summary>
/// Handles the <see cref="ConfirmEmailCommand"/> to verify a user's email address during registration.
/// </summary>
public class ConfirmEmailCommandHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<ConfirmEmailCommand, Result<bool>>
{
    /// <summary>
    /// Processes the email confirmation.
    /// </summary>
    /// <param name="command">The command containing UserId and Token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success result if verified, otherwise a failure.</returns>
    public async Task<Result<bool>> Handle(ConfirmEmailCommand command, CancellationToken cancellationToken)
    {
        var user = await this.UnitOfWork.Users.GetBySecurityTokenAsync(command.Token, Domain.Enums.UserTokenType.EmailVerification, cancellationToken);
        if (user is null)
        {
            return await Result<bool>.FailureAsync(TinyResult.Enums.ErrorCode.NotFound, "Invalid or expired email verification token.");
        }

        user.ConfirmEmailVerification(command.Token, DateTime.UtcNow);

        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
