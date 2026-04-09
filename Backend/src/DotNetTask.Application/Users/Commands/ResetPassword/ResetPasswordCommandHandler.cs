using DotNetTask.Application.Abstractions.Interfaces.Common;
using DotNetTask.Application.Abstractions.Interfaces.Security;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Abstractions.Messaging;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.ValueObjects;

using MediatR;

using TinyResult;

namespace DotNetTask.Application.Users.Commands.ResetPassword;

/// <summary>
/// Handles the <see cref="ResetPasswordCommand"/> to initiate the password recovery process.
/// </summary>
/// <remarks>
/// This handler checks for the user's existence by email. If found, it generates a secure token,
/// updates the user entity with a reset request, and saves changes to trigger potential
/// domain events for email delivery.
/// </remarks>
public class ResetPasswordCommandHandler(
    IUnitOfWork unitOfWork,
    ITokenGenerator tokenGenerator,
    IClock clock)
    : HandlerBase(unitOfWork), IRequestHandler<ResetPasswordCommand, Result<bool>>
{
    private readonly ITokenGenerator _tokenGenerator = tokenGenerator;
    private readonly IClock _clock = clock;

    /// <summary>
    /// Processes the request to initiate a password reset.
    /// </summary>
    /// <param name="command">The command containing the user's email address.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A <see cref="Result{Boolean}"/> indicating success (true).
    /// To prevent email enumeration, it returns success even if the user is not found.
    /// </returns>
    public async Task<Result<bool>> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        UserEntity? user = await this.UnitOfWork.Users.GetByEmailAsync(
            Email.Create(command.Email),
            asNoTracking: false,
            cancellationToken);

        if (user is null)
        {
            return Result<bool>.Success(true);
        }

        string token = this._tokenGenerator.GenerateSecureToken();
        DateTime now = this._clock.UtcNow;

        user.RequestPasswordReset(token, TimeSpan.FromHours(1), now);

        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
