using DotNetTask.Application.Abstractions.Interfaces.Common;
using DotNetTask.Application.Abstractions.Interfaces.Security;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Enums;
using DotNetTask.Domain.ValueObjects;

using MediatR;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Users.Commands.RegisterUser;

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
    ITokenGenerator tokenGenerator,
    IClock clock) : IRequestHandler<RegisterUserCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly ITokenGenerator _tokenGenerator = tokenGenerator;
    private readonly IClock _clock = clock;

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
        UserEntity? user = await this._unitOfWork.Users.GetByEmailAsync(Email.Create(command.Email), asNoTracking: false, cancellationToken);
        if (user is not null && user.Status is not UserStatus.Unconfirmed)
        {
            return Result<Guid>.Failure(ErrorCode.InvalidOperation, EmailPolicy.AlreadyInUseMessage);
        }

        bool userNameExists = await this._unitOfWork.Users.ExistsByUserNameAsync(
            UserName.Create(command.UserName),
            cancellationToken);

        if (userNameExists && (user == null || user.UserName.Value != command.UserName))
        {
            return Result<Guid>.Failure(ErrorCode.InvalidOperation, UserNamePolicy.AlreadyInUseMessage);
        }

        string passwordHash = this._passwordHasher.HashPassword(command.Password);
        string token = this._tokenGenerator.GenerateSecureToken();
        DateTime now = this._clock.UtcNow;

        FirstName firstName = FirstName.Create(command.FirstName);
        UserName userName = UserName.Create(command.UserName);
        Email email = Email.Create(command.Email);
        PasswordHash passwordHashVO = PasswordHash.Create(passwordHash);
        LastName? lastName = LastName.CreateOptional(command.LastName);

        if (user is not null)
        {
            user.UpdateUnconfirmedRegistration(
                firstName,
                userName,
                passwordHashVO,
                token,
                TimeSpan.FromMinutes(15),
                now,
                lastName);
        }
        else
        {
            user = new UserEntity(
                firstName,
                userName,
                email,
                passwordHashVO,
                lastName);

            user.RequestEmailVerification(token, TimeSpan.FromMinutes(15), now);

            await this._unitOfWork.Users.AddAsync(user, cancellationToken);
        }

        await this._unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(user.Id);
    }
}
