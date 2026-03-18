using MediatR;
using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.Common;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Users.Commands.ConfirmEmailChange;

/// <summary>
/// Handles the <see cref="ConfirmEmailChangeCommand"/> to finalize a pending email change request.
/// </summary>
/// <remarks>
/// This handler validates the provided token against the user's stored security token.
/// If valid, it updates the user's email address and clears the temporary token.
/// </remarks>
public class ConfirmEmailChangeCommandHandler(
    IUnitOfWork unitOfWork,
    IClock clock)
    : HandlerBase(unitOfWork), IRequestHandler<ConfirmEmailChangeCommand, Result<bool>>
{
    private readonly IClock _clock = clock;

    /// <summary>
    /// Processes the confirmation of a user's email change.
    /// </summary>
    /// <param name="command">The command containing the user ID and the verification token.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A <see cref="Result{Boolean}"/> indicating success (true) if the email was successfully changed.
    /// Returns a failure result if the user is not found.
    /// </returns>
    public async Task<Result<bool>> Handle(ConfirmEmailChangeCommand command, CancellationToken cancellationToken)
    {
        var user = await this.UnitOfWork.Users.GetBySecurityTokenAsync(command.Token, Domain.Enums.UserTokenType.EmailChange, cancellationToken);
        if (user is null)
        {
            return await Result<bool>.FailureAsync(TinyResult.Enums.ErrorCode.NotFound, "Invalid or expired email change token.");
        }

        var result = user.ConfirmEmailChange(command.Token, this._clock.UtcNow);
        if (!result.IsSuccess)
        {
            return result;
        }

        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
