using TodoListApp.Domain.Common;
using TodoListApp.Domain.Enums;
using TodoListApp.Domain.Events;
using TodoListApp.Domain.Exceptions;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Domain.Entities;

/// <summary>
/// Represents an application user with tasks, task lists, comments, and tags.
/// </summary>
public class UserEntity : BaseEntity
{
    private readonly HashSet<CommentEntity> _comments = new();
    private readonly HashSet<TagEntity> _tags = new();
    private readonly HashSet<TaskListEntity> _taskLists = new();
    private readonly HashSet<TaskEntity> _tasks = new();
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
    /// Thrown when <paramref name="firstName"/> contain more than 20 characters.
    /// Thrown when <paramref name="lastName"/> contain more than 30 characters.
    /// </exception>
    public UserEntity(string firstName, string userName, string email, string passwordHash, string? lastName = null)
    {
        this.FirstName = FirstName.Create(firstName);
        this.UserName = UserName.Create(userName);
        this.Email = Email.Create(email);
        this.PasswordHash = PasswordHash.Create(passwordHash);
        this.LastName = LastName.Create(lastName);
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
    public string SecurityStamp { get; private set; } = Guid.NewGuid().ToString();

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
    public virtual IReadOnlyCollection<TaskEntity> OwnedTasks => this._tasks;

    /// <summary>
    /// Gets the task access records for the user.
    /// </summary>
    public virtual IReadOnlyCollection<UserTaskAccessEntity> TaskAccesses => this._userAccesses;

    /// <summary>
    /// Initiates an email verification request by generating a token.
    /// </summary>
    /// <param name="token">The token value.</param>
    /// <param name="duration">How long the token is valid.</param>
    public void RequestEmailVerification(string token, TimeSpan duration)
    {
        this.CurrentToken = SecurityToken.Create(token, duration, UserTokenType.EmailVerification);
        this.EmailConfirmed = false;

        this.AddDomainEvent(new UserRegisteredDomainEvent(this, this.CurrentToken));
    }

    /// <summary>
    /// Confirms the email verification using the provided token.
    /// </summary>
    /// <param name="token">The verification token.</param>
    /// <param name="currentTime">The current UTC time.</param>
    public void ConfirmEmailVerification(string token, DateTime currentTime)
    {
        if (this.CurrentToken?.IsValid(token, UserTokenType.EmailVerification, currentTime) is not true)
        {
            throw new DomainException("Invalid or expired email verification token.");
        }

        this.EmailConfirmed = true;
        this.CurrentToken = null;
    }

    /// <summary>
    /// Initiates an email change request.
    /// </summary>
    /// <param name="newEmail">The requested new email address.</param>
    /// <param name="confirmationToken">The unique secure token for confirming the new email.</param>
    /// <param name="revertToken">The unique secure token for reverting the change.</param>
    /// <param name="duration">How long the tokens is valid.</param>
    public void RequestEmailChange(Email newEmail, string confirmationToken, string revertToken, TimeSpan duration)
    {
        if (newEmail == this.Email)
        {
            throw new DomainException("New email is same as current.");
        }

        string oldEmail = this.Email.Value;

        this.CurrentToken = SecurityToken.Create(confirmationToken, duration, UserTokenType.EmailChange, newEmail.Value);
        this.RevertToken = SecurityToken.Create(revertToken, duration, UserTokenType.EmailChangeRevert, oldEmail);

        this.AddDomainEvent(new EmailChangeRequestedDomainEvent(this, this.CurrentToken, this.RevertToken));
    }

    /// <summary>
    /// Confirms the pending email change using the confirmation token.
    /// </summary>
    /// <param name="token">The change token sent to the new email address.</param>
    /// <param name="currentTime">The current UTC time.</param>
    public void ConfirmEmailChange(string token, DateTime currentTime)
    {
        if (this.CurrentToken?.IsValid(token, UserTokenType.EmailChange, currentTime) is not true)
        {
            throw new DomainException("Invalid or expired email change token.");
        }

        var pendingEmail = this.CurrentToken.Metadata ?? throw new DomainException("Pending email data is missing.");

        this.Email = Email.Create(pendingEmail);
        this.EmailConfirmed = true;

        this.CurrentToken = null;

        this.UpdateSecurityStamp();
    }

    /// <summary>
    /// Reverts the email change to the original address using the revert token.
    /// </summary>
    /// <param name="token">The revert token sent to the original email address.</param>
    /// <param name="currentTime">The current UTC time.</param>
    public void RevertEmailChange(string token, DateTime currentTime)
    {
        if (this.RevertToken?.IsValid(token, UserTokenType.EmailChangeRevert, currentTime) is not true)
        {
            throw new DomainException("Invalid or expired email change revert token.");
        }

        var oldEmail = this.RevertToken.Metadata ?? throw new DomainException("Original email data is missing.");

        this.Email = Email.Create(oldEmail);
        this.EmailConfirmed = true;

        this.UpdateSecurityStamp();
        this.MustChangePassword = true;

        this.CurrentToken = null;
        this.RevertToken = null;
    }

    /// <summary>
    /// Initiates a password reset request.
    /// </summary>
    /// <param name="token">The unique secure token for password reset.</param>
    /// <param name="duration">The timeframe during which the token remains valid.</param>
    public void RequestPasswordReset(string token, TimeSpan duration)
    {
        this.MustChangePassword = false;
        this.CurrentToken = SecurityToken.Create(token, duration, UserTokenType.PasswordReset);
        this.AddDomainEvent(new PasswordResetRequestedDomainEvent(this, this.CurrentToken));
    }

    /// <summary>
    /// Confirms the password reset and updates the password hash.
    /// </summary>
    /// <param name="newPasswordHash">The new password hash.</param>
    /// <param name="token">The reset token to validate.</param>
    /// <param name="currentTime">The current time to check token expiration.</param>
    /// <exception cref="DomainException">
    /// Thrown when the token is invalid, expired, or the new password is the same as the old one.
    /// </exception>
    public void ConfirmPasswordReset(PasswordHash newPasswordHash, string token, DateTime currentTime)
    {
        if (this.CurrentToken?.IsValid(token, UserTokenType.PasswordReset, currentTime) is not true)
        {
            throw new DomainException("Invalid or expired password reset token.");
        }

        this.UpdateSecurityStamp();
        this.SetPasswordHash(newPasswordHash);

        this.MustChangePassword = false;
        this.CurrentToken = null;
        this.EmailConfirmed = true;
    }

    /// <summary>
    /// Changes the user's password for authenticated users.
    /// </summary>
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
    public void ChangeUserName(UserName userName)
    {
        if (this.UserName.Value.Equals(userName.Value, StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException("New Username is same as current.");
        }

        this.UserName = userName;
    }

    /// <summary>
    /// Sets the new password hash.
    /// </summary>
    /// <param name="newPasswordHash">The new password hash.</param>
    private void SetPasswordHash(PasswordHash newPasswordHash)
    {
        if (this.PasswordHash == newPasswordHash)
        {
            throw new DomainException("New password cannot be the same as the old one");
        }

        this.PasswordHash = newPasswordHash;
    }

    /// <summary>
    /// Sets the new security stamp.
    /// </summary>
    private void UpdateSecurityStamp()
    {
        this.SecurityStamp = Guid.NewGuid().ToString();
    }
}
