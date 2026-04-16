using DotNetTask.Application.Abstractions.Interfaces.Security;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Enums;
using DotNetTask.Domain.ValueObjects;

using MediatR;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Users.Commands.LoginUser;

/// <summary>
/// Handles the <see cref="LoginUserCommand"/> to authenticate a user and generate a JWT.
/// </summary>
public class LoginUserCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator) : IRequestHandler<LoginUserCommand, Result<LoginResponse>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;

    /// <summary>
    /// Processes the login request by validating credentials and generating a security token.
    /// </summary>
    /// <param name="command">The command containing authentication credentials.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the <see cref="LoginResponse"/> on success,
    /// or a failure result with a validation error if credentials are incorrect.
    /// </returns>
    public async Task<Result<LoginResponse>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        Email email = Email.Create(command.Email);

        UserEntity? user = await this._unitOfWork.Users.GetByEmailAsync(email, asNoTracking: true, cancellationToken);

        if (user is null || !this._passwordHasher.VerifyPassword(command.Password, user.PasswordHash.Value))
        {
            return Result<LoginResponse>.Failure(
                ErrorCode.ValidationError, UserPolicy.InvalidCredentialsMessage);
        }

        string token = this._jwtTokenGenerator.GenerateToken(user);

        if (user.Status is UserStatus.PendingDeletion)
        {
            return Result<LoginResponse>.Success(new LoginResponse(
                user.Id,
                user.UserName.Value,
                user.Email.Value,
                Token: token,
                IsAccountPendingDeletion: true));
        }

        if (user.Status is UserStatus.Unconfirmed)
        {
            return Result<LoginResponse>.Success(new LoginResponse(
                user.Id,
                user.UserName.Value,
                user.Email.Value,
                Token: token,
                IsEmailConfirmed: false));
        }

        if (user.MustChangePassword)
        {
            return Result<LoginResponse>.Success(new LoginResponse(
                user.Id,
                user.UserName.Value,
                user.Email.Value,
                Token: token,
                MustChangePassword: true));
        }

        return Result<LoginResponse>.Success(new LoginResponse(
            user.Id,
            user.UserName.Value,
            user.Email.Value,
            token));
    }
}
