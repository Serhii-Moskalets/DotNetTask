using DotNetTask.Domain.Common;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Enums;
using DotNetTask.Domain.Events;
using DotNetTask.Domain.Exceptions;
using DotNetTask.Domain.ValueObjects;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Domain.Entities;

/// <summary>
/// Represents an application user with tasks, task lists, comments, and tags.
/// </summary>
public class UserEntity : BaseEntity
{
    private readonly HashSet<CommentEntity> _comments = new();
    private readonly HashSet<TagEntity> _tags = new();
    private readonly HashSet<TaskListEntity> _taskLists = new();
    private readonly HashSet<TaskEntity> _ownedTasks = new();
    private readonly HashSet<UserTaskAccessEntity> _userAccesses = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UserEntity"/> class.
    /// </summary>
    /// <param name="firstName">The first name of the user.</param>
    /// <param name="userName">The username of the user.</param>
    /// <param name="email">The email of the user.</param>
    /// <param name="passwordHash">The password hash of the user.</param>
    /// <param name="lastName">The last name of the user (optional).</param>
    /// <exception cref="DomainException">
    /// Thrown when
    /// <paramref name="firstName"/>, <paramref name="userName"/>,
    /// <paramref name="email"/>, <paramref name="passwordHash"/>
    /// is null, empty, or consists only of white-space characters.
    /// Thrown when <paramref name="firstName"/> contain more than <see cref="FirstName.MaxLength"/> characters.
    /// Thrown when <paramref name="lastName"/> contain more than <see cref="LastName.MaxLength"/> characters.
    /// </exception>
    public UserEntity(
        FirstName firstName,
        UserName userName,
        Email email,
        PasswordHash passwordHash,
        LastName? lastName = null)
    {
        this.FirstName = firstName;
        this.UserName = userName;
        this.Email = email;
        this.PasswordHash = passwordHash;
        this.LastName = lastName;
    }

    private UserEntity() { }

    /// <summary>
    /// Gets the first name of the user.
    /// </summary>
    public FirstName FirstName { get; private set; } = null!;

    /// <summary>
    /// Gets the last name of the user.
    /// </summary>
    public LastName? LastName { get; private set; }

    /// <summary>
    /// Gets the username of the user.
    /// </summary>
    public UserName UserName { get; private set; } = null!;

    /// <summary>
    /// Gets the email address of the user.
    /// </summary>
    public Email Email { get; private set; } = null!;

    /// <summary>
    /// Gets a value indicating whether the user is required to change their password on the next login.
    /// </summary>
    public bool MustChangePassword { get; private set; } = false;

    /// <summary>
    /// Gets a random value that changes whenever the user's security credentials are updated.
    /// Used for invalidating active sessions.
    /// </summary>
    public SecurityStamp SecurityStamp { get; private set; } = SecurityStamp.New();

    /// <summary>
    /// Gets the current security token assigned to the user for verification or resets.
    /// </summary>
    public SecurityToken? CurrentToken { get; private set; }

    /// <summary>
    /// Gets the security token assigned to the user for revert email change.
    /// </summary>
    public SecurityToken? RevertToken { get; private set; }

    /// <summary>
    /// Gets the hashed password of the user.
    /// </summary>
    public PasswordHash PasswordHash { get; private set; } = null!;

    /// <summary>
    /// Gets the current status of the user account, such as unconfirmed, active, or pending deletion.
    /// </summary>
    public UserStatus Status { get; private set; } = UserStatus.Unconfirmed;

    /// <summary>
    /// Gets the comments created by the user.
    /// </summary>
    public virtual IReadOnlyCollection<CommentEntity> Comments => this._comments;

    /// <summary>
    /// Gets the tags owned by the user.
    /// </summary>
    public virtual IReadOnlyCollection<TagEntity> Tags => this._tags;

    /// <summary>
    /// Gets the task lists owned by the user.
    /// </summary>
    public virtual IReadOnlyCollection<TaskListEntity> TaskLists => this._taskLists;

    /// <summary>
    /// Gets the tasks owned by the user.
    /// </summary>
    public virtual IReadOnlyCollection<TaskEntity> OwnedTasks => this._ownedTasks;

    /// <summary>
    /// Gets the task access records for the user.
    /// </summary>
    public virtual IReadOnlyCollection<UserTaskAccessEntity> UserAccesses => this._userAccesses;

    /// <summary>
    /// Initiates an email verification request by generating a token and raising a registration domain event.
    /// </summary>
    /// <remarks>
    /// This method ensures the user is in an unconfirmed state before generating the token.
    /// If the user is already active or pending deletion, the request will fail.
    /// </remarks>
    /// <param name="token">The token value used for verification.</param>
    /// <param name="duration">The timespan for which the token remains valid.</param>
    /// <param name="currentTime">The current UTC time to calculate expiration.</param>
    /// <returns>
    /// A <see cref="Result{Unit}"/> indicating success, or a failure if the user
    /// is not in a state that allows email verification.
    /// </returns>
    public Result<Unit> RequestEmailVerification(string token, TimeSpan duration, DateTime currentTime)
    {
        Result<Unit> result = this.EnsureUnconfirmed();
        if (result.IsFailure)
        {
            return result;
        }

        this.CurrentToken = SecurityToken.Create(token, duration, UserTokenType.EmailVerification, currentTime);
        this.AddDomainEvent(new UserRegisteredDomainEvent(this, this.CurrentToken));

        return Result<Unit>.Success(Unit.Value);
    }

    /// <summary>
    /// Resends the email verification request by generating a new token, provided the user is still unconfirmed.
    /// </summary>
    /// <param name="token">The new token value.</param>
    /// <param name="duration">The timespan for which the new token remains valid.</param>
    /// <param name="currentTime">The current UTC time.</param>
    /// <returns>A <see cref="Result{Unit}"/> indicating success, or a failure if the user is already confirmed or pending deletion.</returns>
    public Result<Unit> ResendEmailVerification(string token, TimeSpan duration, DateTime currentTime)
    {
        Result<Unit> result = this.EnsureUnconfirmed();
        if (result.IsFailure)
        {
            return result;
        }

        this.CurrentToken = SecurityToken.Create(token, duration, UserTokenType.EmailVerification, currentTime);
        this.AddDomainEvent(new VerificationEmailResendEvent(this, this.CurrentToken));

        return Result<Unit>.Success(Unit.Value);
    }

    /// <summary>
    /// Confirms the email verification using the provided token.
    /// </summary>
    /// <param name="token">The verification token.</param>
    /// <param name="currentTime">The current UTC time.</param>
    /// <returns>A <see cref="Result{Unit}"/> indicating success, or a failure if the token is invalid, expired, or the user is already confirmed.</returns>
    public Result<Unit> ConfirmEmailVerification(string token, DateTime currentTime)
    {
        Result<Unit> result = this.EnsureUnconfirmed();
        if (result.IsFailure)
        {
            return result;
        }

        if (this.CurrentToken?.IsValid(token, UserTokenType.EmailVerification, currentTime) is not true)
        {
            return Result<Unit>.Failure(ErrorCode.Timeout, TokenPolicy.InvalidEmailVerificationTokenMessage);
        }

        this.CurrentToken = null;
        this.Status = UserStatus.Active;

        return Result<Unit>.Success(Unit.Value);
    }

    /// <summary>
    /// Updates registration details (name, username, password) and restarts the email verification process for an unconfirmed user.
    /// </summary>
    /// <param name="firstName">The updated first name.</param>
    /// <param name="userName">The updated username.</param>
    /// <param name="passwordHash">The updated password hash.</param>
    /// <param name="token">The new verification token.</param>
    /// <param name="duration">The validity duration of the new token.</param>
    /// <param name="currentTime">The current UTC time.</param>
    /// <param name="lastName">The updated last name (optional).</param>
    /// <returns>A <see cref="Result{Unit}"/> indicating the outcome of the update and token regeneration.</returns>
    public Result<Unit> UpdateUnconfirmedRegistration(
        FirstName firstName,
        UserName userName,
        PasswordHash passwordHash,
        string token,
        TimeSpan duration,
        DateTime currentTime,
        LastName? lastName = null)
    {
        Result<Unit> result = this.EnsureUnconfirmed();
        if (result.IsFailure)
        {
            return result;
        }

        this.FirstName = firstName;
        this.LastName = lastName;
        this.PasswordHash = passwordHash;
        this.UserName = userName;

        Result<Unit> verificationResult = this.RequestEmailVerification(token, duration, currentTime);
        if (verificationResult.IsFailure)
        {
            return verificationResult;
        }

        this.UpdateSecurityStamp();

        return Result<Unit>.Success(Unit.Value);
    }

    /// <summary>
    /// Initiates a request to change the user's email address by generating both a confirmation token and a revert token.
    /// </summary>
    /// <param name="newEmail">The new email address requested.</param>
    /// <param name="confirmationToken">The token to be sent to the new email.</param>
    /// <param name="revertToken">The token to be sent to the current email to allow reverting the change.</param>
    /// <param name="duration">The validity duration for both tokens.</param>
    /// <param name="currentTime">The current UTC time.</param>
    /// <returns>A <see cref="Result{Unit}"/> indicating success, or a failure if the email is identical to the current one or the account is not active.</returns>
    public Result<Unit> RequestEmailChange(Email newEmail, string confirmationToken, string revertToken, TimeSpan duration, DateTime currentTime)
    {
        Result<Unit> result = this.EnsureActive();
        if (result.IsFailure)
        {
            return result;
        }

        if (newEmail == this.Email)
        {
            return Result<Unit>.Failure(ErrorCode.ValidationError, EmailPolicy.SameAsCurrentMessage);
        }

        string oldEmail = this.Email.Value;
        this.Status = UserStatus.Unconfirmed;

        this.CurrentToken = SecurityToken.Create(confirmationToken, duration, UserTokenType.EmailChange, currentTime, newEmail.Value);
        this.RevertToken = SecurityToken.Create(revertToken, duration, UserTokenType.EmailChangeRevert, currentTime, oldEmail);

        this.AddDomainEvent(new EmailChangeRequestedDomainEvent(this, this.CurrentToken, this.RevertToken));

        return Result<Unit>.Success(Unit.Value);
    }

    /// <summary>
    /// Confirms the pending email change using the token sent to the new address and updates the security stamp.
    /// </summary>
    /// <param name="token">The confirmation token.</param>
    /// <param name="currentTime">The current UTC time.</param>
    /// <returns>A <see cref="Result{Unit}"/> indicating success, or a failure if the token is invalid or the metadata is missing.</returns>
    public Result<Unit> ConfirmEmailChange(string token, DateTime currentTime)
    {
        Result<Unit> result = this.EnsureUnconfirmed();
        if (result.IsFailure)
        {
            return result;
        }

        if (this.CurrentToken?.IsValid(token, UserTokenType.EmailChange, currentTime) is not true)
        {
            return Result<Unit>.Failure(ErrorCode.Timeout, TokenPolicy.InvalidEmailChangeTokenMessage);
        }

        string pendingEmail = this.CurrentToken.Metadata ?? throw new DomainException(TokenPolicy.MissingPendingEmailMessage);

        this.Email = Email.Create(pendingEmail);

        this.Status = UserStatus.Active;
        this.CurrentToken = null;

        this.UpdateSecurityStamp();

        return Result<Unit>.Success(Unit.Value);
    }

    /// <summary>
    /// Reverts the email address to the previous one using a revert token and forces a password change for security.
    /// </summary>
    /// <param name="revertToken">The revert token sent to the original email.</param>
    /// <param name="currentTime">The current UTC time.</param>
    /// <param name="resetToken">The token used to facilitate the forced password reset.</param>
    /// <param name="duration">The validity duration of the reset token.</param>
    /// <returns>A <see cref="Result{Unit}"/> indicating success, or a failure if the revert token is invalid.</returns>
    public Result<Unit> RevertEmailChange(string revertToken, DateTime currentTime, string resetToken, TimeSpan duration)
    {
        if (this.RevertToken?.IsValid(revertToken, UserTokenType.EmailChangeRevert, currentTime) is not true)
        {
            return Result<Unit>.Failure(ErrorCode.Timeout, TokenPolicy.InvalidEmailRevertTokenMessage);
        }

        string oldEmail = this.RevertToken.Metadata ?? throw new DomainException(TokenPolicy.MissingOriginalEmailMessage);

        this.Email = Email.Create(oldEmail);
        this.Status = UserStatus.Active;

        this.UpdateSecurityStamp();
        this.MustChangePassword = true;

        this.CurrentToken = SecurityToken.Create(resetToken, duration, UserTokenType.PasswordReset, currentTime);
        this.RevertToken = null;

        return Result<Unit>.Success(Unit.Value);
    }

    /// <summary>
    /// Initiates a password reset request.
    /// </summary>
    /// <param name="token">The unique secure token for password reset.</param>
    /// <param name="duration">The timeframe during which the token remains valid.</param>
    /// <param name="currentTime">The current UTC time.</param>
    public void RequestPasswordReset(string token, TimeSpan duration, DateTime currentTime)
    {
        this.MustChangePassword = false;
        this.CurrentToken = SecurityToken.Create(token, duration, UserTokenType.PasswordReset, currentTime);
        this.AddDomainEvent(new PasswordResetRequestedDomainEvent(this, this.CurrentToken));
    }

    /// <summary>
    /// Validates the reset token and updates the user's password hash and security stamp.
    /// </summary>
    /// <param name="newPasswordHash">The new hashed password.</param>
    /// <param name="token">The reset token to validate.</param>
    /// <param name="currentTime">The current UTC time.</param>
    /// <returns>A <see cref="Result{Unit}"/> indicating success, or a failure if the token is invalid or the account is inactive.</returns>
    public Result<Unit> ConfirmPasswordReset(PasswordHash newPasswordHash, string token, DateTime currentTime)
    {
        if (this.CurrentToken?.IsValid(token, UserTokenType.PasswordReset, currentTime) is not true)
        {
            return Result<Unit>.Failure(ErrorCode.Timeout, TokenPolicy.InvalidPasswordResetTokenMessage);
        }

        this.UpdateSecurityStamp();

        Result<Unit> hashResult = this.SetPasswordHash(newPasswordHash);
        if (hashResult.IsFailure)
        {
            return hashResult;
        }

        this.MustChangePassword = false;
        this.CurrentToken = null;
        this.Status = UserStatus.Active;

        return Result<Unit>.Success(Unit.Value);
    }

    /// <summary>
    /// Updates the password hash for an already authenticated and active user.
    /// </summary>
    /// <param name="newPasswordHash">The new hashed password.</param>
    /// <returns>A <see cref="Result{Unit}"/> indicating success, or a failure if the account is not active.</returns>
    public Result<Unit> ChangePassword(PasswordHash newPasswordHash)
    {
        Result<Unit> result = this.EnsureActive();
        if (result.IsFailure)
        {
            return result;
        }

        this.UpdateSecurityStamp();

        Result<Unit> hashResult = this.SetPasswordHash(newPasswordHash);
        if (hashResult.IsFailure)
        {
            return hashResult;
        }

        this.MustChangePassword = false;

        return Result<Unit>.Success(Unit.Value);
    }

    /// <summary>
    /// Updates the user's first name if the account is active.
    /// </summary>
    /// <param name="newFirstName">The new first name to set.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> where the value is <c>true</c> if the name was updated,
    /// or <c>false</c> if the new name is identical to the current one.
    /// Returns a failure if the account is not in an active state.
    /// </returns>
    public Result<bool> ChangeFirstName(FirstName newFirstName)
    {
        Result<Unit> result = this.EnsureActive();
        if (result.IsFailure)
        {
            return Result<bool>.Failure(result.Error!.Code, result.Error!.Message);
        }

        if (this.FirstName == newFirstName)
        {
            return Result<bool>.Success(false);
        }

        this.FirstName = newFirstName;
        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Updates the user's last name if the account is active.
    /// </summary>
    /// <param name="newLastName">The new last name to set (can be null).</param>
    /// <returns>
    /// A <see cref="Result{T}"/> where the value is <c>true</c> if the name was updated,
    /// or <c>false</c> if the new name is identical to the current one.
    /// Returns a failure if the account is not in an active state.
    /// </returns>
    public Result<bool> ChangeLastName(LastName? newLastName)
    {
        Result<Unit> result = this.EnsureActive();
        if (result.IsFailure)
        {
            return Result<bool>.Failure(result.Error!.Code, result.Error!.Message);
        }

        if (this.LastName == newLastName)
        {
            return Result<bool>.Success(false);
        }

        this.LastName = newLastName;
        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Updates the account's username, ensuring it is not the same as the current one.
    /// </summary>
    /// <param name="userName">The new username to set.</param>
    /// <returns>A <see cref="Result{Unit}"/> indicating success, or a failure if the username is identical or the account is inactive.</returns>
    public Result<Unit> ChangeUserName(UserName userName)
    {
        Result<Unit> result = this.EnsureActive();
        if (result.IsFailure)
        {
            return result;
        }

        if (this.UserName.Value.Equals(userName.Value, StringComparison.OrdinalIgnoreCase))
        {
            return Result<Unit>.Failure(ErrorCode.ValidationError, UserNamePolicy.SameAsCurrentMessage);
        }

        this.UserName = userName;
        return Result<Unit>.Success(Unit.Value);
    }

    /// <summary>
    /// Internal helper to update the password hash after ensuring the user is active and the new password is different from the old one.
    /// </summary>
    /// <param name="newPasswordHash">The new password hash.</param>
    /// <returns>A <see cref="Result{Unit}"/> indicating whether the hash was successfully updated.</returns>
    private Result<Unit> SetPasswordHash(PasswordHash newPasswordHash)
    {
        if (this.PasswordHash == newPasswordHash)
        {
            return Result<Unit>.Failure(ErrorCode.ValidationError, PasswordPolicy.SameAsOldMessage);
        }

        this.PasswordHash = newPasswordHash;
        return Result<Unit>.Success(Unit.Value);
    }

    /// <summary>
    /// Updates the <see cref="SecurityStamp"/> with a new unique value.
    /// </summary>
    private void UpdateSecurityStamp() => this.SecurityStamp = SecurityStamp.New();

    /// <summary>
    /// Validates that the user's current status allows for active operations.
    /// </summary>
    /// <returns>A success result if active; otherwise, a failure describing why the account is restricted.</returns>
    private Result<Unit> EnsureActive() =>
        this.Status switch
        {
            UserStatus.Unconfirmed => Result<Unit>.Failure(ErrorCode.ValidationError, UserPolicy.EmailIsNotConfirmedMessage),
            UserStatus.Active => Result<Unit>.Success(Unit.Value),
            _ => throw new DomainException(UserPolicy.InvalidUserStatus),
        };

    /// <summary>
    /// Validates that the user is currently in an unconfirmed state.
    /// </summary>
    /// <returns>A success result if unconfirmed; otherwise, a failure if already confirmed or pending deletion.</returns>
    private Result<Unit> EnsureUnconfirmed() =>
        this.Status switch
        {
            UserStatus.Unconfirmed => Result<Unit>.Success(Unit.Value),
            UserStatus.Active => Result<Unit>.Failure(ErrorCode.ValidationError, UserPolicy.EmailAlreadyConfirmedMessage),
            _ => throw new DomainException("Unknown user status"),
            _ => throw new DomainException(UserPolicy.InvalidUserStatus),
        };
        };
}
