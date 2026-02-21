using MediatR;
using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Users.Commands.ConfirmChangeEmail;

/// <summary>
/// Handles the <see cref="ConfirmChangeEmailCommand"/> to finalize a pending email change request.
/// </summary>
/// <remarks>
/// This handler validates the provided token against the user's stored security token.
/// If valid, it updates the user's email address and clears the temporary token.
/// </remarks>
public class ConfirmChangeEmailCommandHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<ConfirmChangeEmailCommand, Result<bool>>
{
    /// <summary>
    /// Processes the confirmation of a user's email change.
    /// </summary>
    /// <param name="command">The command containing the user ID and the verification token.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A <see cref="Result{Boolean}"/> indicating success (true) if the email was successfully changed.
    /// Returns a failure result if the user is not found.
    /// </returns>
    public async Task<Result<bool>> Handle(ConfirmChangeEmailCommand command, CancellationToken cancellationToken)
    {
        var user = await this.UnitOfWork.Users.GetByIdAsync(command.UserId, asNoTracking: false, cancellationToken);
        if (user is null)
        {
            return await Result<bool>.FailureAsync(TinyResult.Enums.ErrorCode.NotFound, "User not found.");
        }

        user.ConfirmEmailChange(command.Token, DateTime.UtcNow);

        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
