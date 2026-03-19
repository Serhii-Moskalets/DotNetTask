using MediatR;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Security;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Domain.Constants;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Users.Commands.LoginUser;

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
        var email = Email.Create(command.Email);

        var user = await this._unitOfWork.Users.GetByEmailAsync(email, asNoTracking: true, cancellationToken);

        if (user is null || !this._passwordHasher.VerifyPassword(command.Password, user.PasswordHash.Value))
        {
            return await Result<LoginResponse>.FailureAsync(
                ErrorCode.ValidationError, UserPolicy.InvalidCredentialsMessage);
        }

        if (!user.EmailConfirmed)
        {
            return await Result<LoginResponse>.FailureAsync(
                ErrorCode.ValidationError, EmailPolicy.NotConfirmedMessage);
        }

        if (user.MustChangePassword)
        {
            return Result<LoginResponse>.Success(new LoginResponse(
                user.Id,
                user.UserName.Value,
                user.Email.Value,
                Token: null,
                MustChangePassword: true));
        }

        var token = this._jwtTokenGenerator.GenerateToken(user);

        return Result<LoginResponse>.Success(new LoginResponse(
            user.Id,
            user.UserName.Value,
            user.Email.Value,
            token,
            MustChangePassword: false));
    }
}
