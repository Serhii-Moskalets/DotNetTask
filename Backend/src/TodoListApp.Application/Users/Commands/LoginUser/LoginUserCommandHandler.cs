using MediatR;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Security;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;

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
    /// <param name="request">The command containing authentication credentials.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the <see cref="LoginResponse"/> on success,
    /// or a failure result with a validation error if credentials are incorrect.
    /// </returns>
    public async Task<Result<LoginResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await this._unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null || !this._passwordHasher.VerifyPassword(request.Password, user.PasswordHash.Value))
        {
            return await Result<LoginResponse>.FailureAsync(
                ErrorCode.ValidationError,
                "Invalid email or password.");
        }

        if (!user.EmailConfirmed)
        {
            return await Result<LoginResponse>.FailureAsync(
                ErrorCode.ValidationError,
                "Please confirm your email before logging in.");
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
