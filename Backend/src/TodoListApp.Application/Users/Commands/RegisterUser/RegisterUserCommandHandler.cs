using MediatR;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Security;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Users.Commands.RegisterUser;

/// <summary>
/// Handles the <see cref="RegisterUserCommand"/> to create a new user account.
/// </summary>
/// <remarks>
/// This handler performs uniqueness checks for email and username, hashes the password,
/// creates a new user entity, and initiates the email verification process.
/// </remarks>
public class RegisterUserCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ITokenGenerator tokenGenerator) : IRequestHandler<RegisterUserCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly ITokenGenerator _tokenGenerator = tokenGenerator;

    /// <summary>
    /// Processes the user registration request.
    /// </summary>
    /// <param name="command">The command containing user registration details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the new user's unique identifier on success,
    /// or a failure result with a validation error if the email or username is already taken.
    /// </returns>
    public async Task<Result<Guid>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var user = await this._unitOfWork.Users.GetByEmailAsync(Email.Create(command.Email), asNoTracking: false, cancellationToken);
        if (user?.EmailConfirmed is true)
        {
            return await Result<Guid>.FailureAsync(ErrorCode.ValidationError, "Email is already exists.");
        }

        var userNameExists = await this._unitOfWork.Users.ExistsByUserNameAsync(
            UserName.Create(command.UserName),
            cancellationToken);

        if (userNameExists && (user == null || user.UserName.Value != command.UserName))
        {
            return await Result<Guid>.FailureAsync(ErrorCode.ValidationError, "User name is already exists.");
        }

        var passwordHash = this._passwordHasher.HashPassword(command.Password);
        var token = this._tokenGenerator.GenerateSecureToken();

        if (user is not null)
        {
            user.UpdateUnconfirmedRegistration(
                FirstName.Create(command.FirstName),
                UserName.Create(command.UserName),
                PasswordHash.Create(passwordHash),
                token,
                TimeSpan.FromMinutes(15),
                LastName.Create(command.LastName));
        }
        else
        {
            user = new UserEntity(
                command.FirstName,
                command.UserName,
                command.Email,
                passwordHash,
                command.LastName);

            user.RequestEmailVerification(token, TimeSpan.FromMinutes(15));

            await this._unitOfWork.Users.AddAsync(user, cancellationToken);
        }

        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<Guid>.SuccessAsync(user.Id);
    }
}
