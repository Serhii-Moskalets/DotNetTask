using DotNetTask.Application.Abstractions.Interfaces.Common;
using DotNetTask.Application.Abstractions.Interfaces.Security;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;

using MediatR;

using TinyResult;

namespace DotNetTask.Application.Users.Commands.RevertEmailChange;

/// <summary>
/// Handles the <see cref="RevertEmailChangeCommand"/> to restore a user's original email address.
/// </summary>
/// <remarks>
/// This handler retrieves the user, invokes the domain logic to revert the email,
/// updates security credentials (must change password, security stamp),
/// and persists changes via the unit of work.
/// </remarks>
public class RevertEmailChangeCommandHandler(
    IUnitOfWork unitOfWork,
    ITokenGenerator tokenGenerator,
    IClock clock) : HandlerBase(unitOfWork), IRequestHandler<RevertEmailChangeCommand, Result<string>>
{
    private readonly ITokenGenerator _tokenGenerator = tokenGenerator;
    private readonly IClock _clock = clock;

    /// <summary>
    /// Processes the revert request.
    /// </summary>
    /// <param name="command">The revert command containing user ID and security token.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="Result{T}"/> indicating success or failure (e.g., if user not found).</returns>
    public async Task<Result<string>> Handle(RevertEmailChangeCommand command, CancellationToken cancellationToken)
    {
        UserEntity? user = await this.UnitOfWork.Users.GetBySecurityTokenAsync(command.Token, Domain.Enums.UserTokenType.EmailChangeRevert, cancellationToken);
        if (user is null)
        {
            return await Result<string>.FailureAsync(TinyResult.Enums.ErrorCode.NotFound, TokenPolicy.InvalidEmailRevertTokenMessage);
        }

        string resetToken = this._tokenGenerator.GenerateSecureToken();

        Result<bool> result = user.RevertEmailChange(command.Token, this._clock.UtcNow, resetToken, TimeSpan.FromMinutes(15));
        if (!result.IsSuccess)
        {
            return Result<string>.Failure(result.Error!.Code, result.Error.Message);
        }

        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<string>.SuccessAsync(resetToken);
    }
}
