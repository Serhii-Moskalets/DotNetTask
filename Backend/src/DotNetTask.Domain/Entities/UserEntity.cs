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
    /// Gets a value indicating whether the user's email is confirmed.
    /// </summary>
    public bool EmailConfirmed { get; private set; }

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
    /// Initiates an email verification request by generating a token.
    /// </summary>
    /// <param name="token">The token value.</param>
    /// <param name="duration">How long the token is valid.</param>
    /// <param name="currentTime">The current UTC time.</param>
    public void RequestEmailVerification(string token, TimeSpan duration, DateTime currentTime)
    {
        this.CurrentToken = SecurityToken.Create(token, duration, UserTokenType.EmailVerification, currentTime);
        this.EmailConfirmed = false;

        this.AddDomainEvent(new UserRegisteredDomainEvent(this, this.CurrentToken));
    }

    /// <summary>
    /// Resends the email verification request by generating a new token.
    /// </summary>
    /// <remarks>
    /// This method should be used when the user hasn't received the previous email or the token has expired.
    /// It will only proceed if the email is not already confirmed.
    /// </remarks>
    /// <param name="token">The new token value.</param>
    /// <param name="duration">How long the new token is valid.</param>
    /// <param name="currentTime">The current UTC time.</param>
    /// <returns>A <see cref="Result{T}"/> indicating whether the request was successfully re-initiated.</returns>
    public Result<bool> ResendEmailVerification(string token, TimeSpan duration, DateTime currentTime)
    {
        if (this.EmailConfirmed)
        {
            return Result<bool>.Failure(ErrorCode.ValidationError, UserPolicy.EmailAlreadyConfirmedMessage);
        }

        this.CurrentToken = SecurityToken.Create(token, duration, UserTokenType.EmailVerification, currentTime);
        this.AddDomainEvent(new VerificationEmailResendEvent(this, this.CurrentToken));

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Confirms the email verification using the provided token.
    /// </summary>
    /// <param name="token">The verification token.</param>
    /// <param name="currentTime">The current UTC time.</param>
    /// <returns>Return booean result true or false.</returns>
    public Result<bool> ConfirmEmailVerification(string token, DateTime currentTime)
    {
        if (this.CurrentToken?.IsValid(token, UserTokenType.EmailVerification, currentTime) is not true)
        {
            return Result<bool>.Failure(ErrorCode.Timeout, TokenPolicy.InvalidEmailVerificationTokenMessage);
        }

        this.EmailConfirmed = true;
        this.CurrentToken = null;
        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Updates the registration details for a user whose email address has not yet been confirmed. This method resets
    /// the user's registration information and generates a new email verification token.
    /// </summary>
    /// <remarks>This method should only be called for users who have not yet confirmed their email address.
    /// Calling this method will reset the user's registration details and invalidate any previous email verification
    /// tokens.</remarks>
    /// <param name="firstName">The first name to assign to the user during the registration update.</param>
    /// <param name="userName">The username to assign to the user during the registration update.</param>
    /// <param name="passwordHash">The hashed password to associate with the user during the registration update.</param>
    /// <param name="token">The token string to use for creating a new email verification token.</param>
    /// <param name="duration">The duration for which the email verification token remains valid.</param>
    /// <param name="currentTime">The current UTC time.</param>
    /// <param name="lastName">The last name to assign to the user during the registration update. This parameter is optional.</param>
    /// <exception cref="DomainException">Thrown if the user's email address has already been confirmed.</exception>
    public void UpdateUnconfirmedRegistration(
        FirstName firstName,
        UserName userName,
        PasswordHash passwordHash,
        string token,
        TimeSpan duration,
        DateTime currentTime,
        LastName? lastName = null)
    {
        if (this.EmailConfirmed)
        {
            throw new DomainException(UserPolicy.EmailAlreadyConfirmedMessage);
        }

        this.FirstName = firstName;
        this.LastName = lastName;
        this.PasswordHash = passwordHash;
        this.UserName = userName;

        this.RequestEmailVerification(token, duration, currentTime);

        this.UpdateSecurityStamp();
    }

    /// <summary>
    /// Initiates an email change request.
    /// </summary>
    /// <param name="newEmail">The requested new email address.</param>
    /// <param name="confirmationToken">The unique secure token for confirming the new email.</param>
    /// <param name="revertToken">The unique secure token for reverting the change.</param>
    /// <param name="duration">How long the tokens is valid.</param>
    /// <param name="currentTime">The current UTC time.</param>
    /// <returns>Return booean result true or false.</returns>
    public Result<bool> RequestEmailChange(Email newEmail, string confirmationToken, string revertToken, TimeSpan duration, DateTime currentTime)
    {
        if (newEmail == this.Email)
        {
            return Result<bool>.Failure(ErrorCode.ValidationError, EmailPolicy.SameAsCurrentMessage);
        }

        string oldEmail = this.Email.Value;

        this.CurrentToken = SecurityToken.Create(confirmationToken, duration, UserTokenType.EmailChange, currentTime, newEmail.Value);
        this.RevertToken = SecurityToken.Create(revertToken, duration, UserTokenType.EmailChangeRevert, currentTime, oldEmail);

        this.AddDomainEvent(new EmailChangeRequestedDomainEvent(this, this.CurrentToken, this.RevertToken));

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Confirms the pending email change using the confirmation token.
    /// </summary>
    /// <param name="token">The change token sent to the new email address.</param>
    /// <param name="currentTime">The current UTC time.</param>
    /// <returns>Return booean result true or false.</returns>
    public Result<bool> ConfirmEmailChange(string token, DateTime currentTime)
    {
        if (this.CurrentToken?.IsValid(token, UserTokenType.EmailChange, currentTime) is not true)
        {
            return Result<bool>.Failure(ErrorCode.Timeout, TokenPolicy.InvalidEmailChangeTokenMessage);
        }

        string pendingEmail = this.CurrentToken.Metadata ?? throw new DomainException(TokenPolicy.MissingPendingEmailMessage);

        this.Email = Email.Create(pendingEmail);
        this.EmailConfirmed = true;

        this.CurrentToken = null;

        this.UpdateSecurityStamp();

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Reverts the email change to the original address using the revert token.
    /// </summary>
    /// <param name="revertToken">The revert token sent to the original email address.</param>
    /// <param name="currentTime">The current UTC time.</param>
    /// <param name="resetToken">TThe unique secure token for password reset.</param>
    /// <param name="duration">The timeframe during which the token remains valid.</param>
    /// <returns>Return booean result true or false.</returns>
    public Result<bool> RevertEmailChange(string revertToken, DateTime currentTime, string resetToken, TimeSpan duration)
    {
        if (this.RevertToken?.IsValid(revertToken, UserTokenType.EmailChangeRevert, currentTime) is not true)
        {
            return Result<bool>.Failure(ErrorCode.Timeout, TokenPolicy.InvalidEmailRevertTokenMessage);
        }

        string oldEmail = this.RevertToken.Metadata ?? throw new DomainException(TokenPolicy.MissingOriginalEmailMessage);

        this.Email = Email.Create(oldEmail);
        this.EmailConfirmed = true;

        this.UpdateSecurityStamp();
        this.MustChangePassword = true;

        this.CurrentToken = SecurityToken.Create(resetToken, duration, UserTokenType.PasswordReset, currentTime);
        this.RevertToken = null;

        return Result<bool>.Success(true);
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
    /// Confirms the password reset and updates the password hash.
    /// </summary>
    /// <param name="newPasswordHash">The new password hash.</param>
    /// <param name="token">The reset token to validate.</param>
    /// <param name="currentTime">The current UTC time.</param>
    /// <returns>Add returns doccumentations.</returns>
    public Result<bool> ConfirmPasswordReset(PasswordHash newPasswordHash, string token, DateTime currentTime)
    {
        if (this.CurrentToken?.IsValid(token, UserTokenType.PasswordReset, currentTime) is not true)
        {
            return Result<bool>.Failure(ErrorCode.Timeout, TokenPolicy.InvalidPasswordResetTokenMessage);
        }

        this.UpdateSecurityStamp();
        this.SetPasswordHash(newPasswordHash);

        this.MustChangePassword = false;
        this.CurrentToken = null;
        this.EmailConfirmed = true;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Changes the user's password for authenticated users.
    /// </summary>
    /// <remarks>
    /// This method is intended only for authenticated users with a confirmed email.
    /// For unauthenticated password recovery, use <see cref="ConfirmPasswordReset"/> instead.
    /// </remarks>
    /// <param name="newPasswordHash">The new password hash to be set.</param>
    public void ChangePassword(PasswordHash newPasswordHash)
    {
        this.UpdateSecurityStamp();
        this.SetPasswordHash(newPasswordHash);
        this.MustChangePassword = false;
    }

    /// <summary>
    /// Updates the user's first name.
    /// </summary>
    /// <param name="newFirstName">The new first name.</param>
    /// <returns>True if the first name was changed; otherwise, false.</returns>
    public bool ChangeFirstName(FirstName newFirstName)
    {
        if (this.FirstName == newFirstName)
        {
            return false;
        }

        this.FirstName = newFirstName;
        return true;
    }

    /// <summary>
    /// Updates the user's last name.
    /// </summary>
    /// <param name="newLastName">The new last name.</param>
    /// <returns>True if the last name was changed; otherwise, false..</returns>
    public bool ChangeLastName(LastName? newLastName)
    {
        if (this.LastName == newLastName)
        {
            return false;
        }

        this.LastName = newLastName;
        return true;
    }

    /// <summary>
    /// Updates the user's account username.
    /// </summary>
    /// <param name="userName">The new username.</param>
    /// <returns>Return booean result true or false.</returns>
    public Result<bool> ChangeUserName(UserName userName)
    {
        if (this.UserName.Value.Equals(userName.Value, StringComparison.OrdinalIgnoreCase))
        {
            return Result<bool>.Failure(ErrorCode.ValidationError, UserNamePolicy.SameAsCurrentMessage);
        }

        this.UserName = userName;
        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Sets the new password hash.
    /// </summary>
    /// <param name="newPasswordHash">The new password hash.</param>
    private void SetPasswordHash(PasswordHash newPasswordHash)
    {
        if (this.PasswordHash == newPasswordHash)
        {
            throw new DomainException(PasswordPolicy.SameAsOldMessage);
        }

        this.PasswordHash = newPasswordHash;
    }

    /// <summary>
    /// Sets the new security stamp.
    /// </summary>
    private void UpdateSecurityStamp()
    {
        this.SecurityStamp = SecurityStamp.New();
    }
}
