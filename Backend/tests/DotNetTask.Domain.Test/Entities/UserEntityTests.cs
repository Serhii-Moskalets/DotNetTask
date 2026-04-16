using DotNetTask.Domain.Common;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Entities;
using DotNetTask.Domain.Enums;
using DotNetTask.Domain.Events;
using DotNetTask.Domain.Test.Common;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Domain.Test.Entities;

/// <summary>
/// Unit tests for the <see cref="UserEntity"/> class.
/// </summary>
public class UserEntityTests : BaseTest
{
    private const string CurrentFirstName = "John";
    private const string CurrentLastName = "Doe";
    private const string CurrentUserName = "jdoe";
    private const string CurrentEmail = "john@example.com";

    private const string TokenValue = "token123";
    private const string RevertToken = "revert_secret";

    private static readonly Email NewEmail = Email.Create("newEmail@example.com");
    private static readonly string PasswordHashString = new('a', 64);
    private static readonly string NewPasswordHashString = new('b', 64);

    private static readonly TimeSpan Duration = TimeSpan.FromHours(1);

    /// <summary>
    /// Tests that the constructor creates a user with valid data.
    /// </summary>
    [Fact]
    public void Constructor_Should_CreateUser_When_ValidData()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();

        // Assert
        user.FirstName.Value.Should().Be(CurrentFirstName);
        user.LastName!.Value.Should().Be(CurrentLastName);
        user.UserName.Value.Should().Be(CurrentUserName);
        user.Email.Value.Should().Be(CurrentEmail);
        user.PasswordHash.Value.Should().Be(PasswordHashString);
        user.SecurityStamp.Should().NotBeNull();
        user.SecurityStamp.Value.Should().NotBeNullOrWhiteSpace();
        user.MustChangePassword.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.RequestEmailVerification"/> sets the verification token,
    /// raises the registration event, and ensures the user status is unconfirmed.
    /// </summary>
    [Fact]
    public void RequestEmailVerification_Should_SetToken_Event_And_MarkEmailUnconfirmed()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();

        // Act
        Result<Unit> result = user.RequestEmailVerification(TokenValue, Duration, this.Clock.UtcNow);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.CurrentToken.Should().NotBeNull();
        user.CurrentToken.Type.Should().Be(UserTokenType.EmailVerification);
        user.Status.Should().Be(UserStatus.Unconfirmed);

        UserRegisteredDomainEvent evt = user.DomainEvents
            .OfType<UserRegisteredDomainEvent>()
            .Should().ContainSingle()
            .Subject;

        evt.VerificationToken.Should().Be(user.CurrentToken);

        user.ClearDomainEvents();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.RequestEmailVerification"/> returns failure
    /// when the user already has an active status and a confirmed email.
    /// </summary>
    [Fact]
    public void RequestEmailVerification_Should_ReturnFailure_When_UserIsActive()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();

        // Act
        Result<Unit> result = user.RequestEmailVerification(TokenValue, Duration, this.Clock.UtcNow);

        // Assert
        AssertError<Unit>(result, ErrorCode.ValidationError, UserPolicy.EmailAlreadyConfirmedMessage);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ResendEmailVerification"/> successfully updates the token
    /// and raises a resend domain event.
    /// </summary>
    [Fact]
    public void ResendEmailVerification_Should_Success_SetToken_And_Event()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();

        // Act
        Result<Unit> result = user.ResendEmailVerification(TokenValue, Duration, this.Clock.UtcNow);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.CurrentToken.Should().NotBeNull();
        user.CurrentToken.Type.Should().Be(UserTokenType.EmailVerification);
        user.CurrentToken.Value.Should().Be(TokenValue);
        user.Status.Should().Be(UserStatus.Unconfirmed);

        VerificationEmailResendEvent evt = user.DomainEvents
            .OfType<VerificationEmailResendEvent>()
            .Should().ContainSingle()
            .Subject;

        evt.ResendVerificationToken.Should().Be(user.CurrentToken);

        user.ClearDomainEvents();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ResendEmailVerification"/> returns failure
    /// when the email has already been confirmed.
    /// </summary>
    [Fact]
    public void ResendEmailVerification_Should_ReturnFailure_When_EmailAlreadyConfirmed()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();

        // Act
        Result<Unit> result = user.ResendEmailVerification("new_token", Duration, this.Clock.UtcNow);

        // Assert
        AssertError<Unit>(result, ErrorCode.ValidationError, UserPolicy.EmailAlreadyConfirmedMessage);
        user.CurrentToken.Should().BeNull();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmEmailVerification"/> marks the user as active
    /// when a valid verification token is provided.
    /// </summary>
    [Fact]
    public void ConfirmEmailVerification_Should_SetConfirmed_When_TokenValid()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();

        // Act
        Result<Unit> requestEmailVerifResult = user.RequestEmailVerification(TokenValue, Duration, this.Clock.UtcNow);
        Result<Unit> confirmEmailVerifResult = user.ConfirmEmailVerification(TokenValue, this.Clock.UtcNow);

        // Assert
        requestEmailVerifResult.IsSuccess.Should().BeTrue();
        confirmEmailVerifResult.IsSuccess.Should().BeTrue();
        user.Status.Should().Be(UserStatus.Active);
        user.CurrentToken.Should().BeNull();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmEmailVerification"/> returns failure
    /// when the provided token has expired.
    /// </summary>
    [Fact]
    public void ConfirmEmailVerification_Should_ReturnFailure_When_TokenExpired()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();

        user.RequestEmailVerification(TokenValue, TimeSpan.FromMinutes(1), this.Clock.UtcNow);

        this.Clock.Advance(TimeSpan.FromMinutes(2));

        // Act
        Result<Unit> result = user.ConfirmEmailVerification(TokenValue, this.Clock.UtcNow);

        // Assert
        AssertError<Unit>(result, ErrorCode.Timeout, TokenPolicy.InvalidEmailVerificationTokenMessage);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmEmailVerification"/> returns failure
    /// when no verification request was previously made.
    /// </summary>
    [Fact]
    public void ConfirmEmailVerification_Should_ReturnFailure_When_NoRequestWasMade()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();

        // Act
        Result<Unit> result = user.ConfirmEmailVerification(TokenValue, this.Clock.UtcNow);

        // Assert
        AssertError<Unit>(result, ErrorCode.Timeout, TokenPolicy.InvalidEmailVerificationTokenMessage);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmEmailVerification"/> returns failure
    /// when the provided token value is incorrect.
    /// </summary>
    [Fact]
    public void ConfirmEmailVerification_Should_ReturnFailure_When_TokenInvalid()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();
        user.RequestEmailVerification(TokenValue, Duration, this.Clock.UtcNow);

        // Act
        Result<Unit> result = user.ConfirmEmailVerification("invalidtoken", this.Clock.UtcNow);

        // Assert
        AssertError<Unit>(result, ErrorCode.Timeout, TokenPolicy.InvalidEmailVerificationTokenMessage);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.RequestEmailChange"/> returns failure
    /// when the requested email is the same as the current one.
    /// </summary>
    [Fact]
    public void RequestEmailChange_Should_ReturnFailure_When_EmailIsSame()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();

        // Act
        Result<Unit> result = user.RequestEmailChange(user.Email, TokenValue, RevertToken, Duration, this.Clock.UtcNow);

        // Assert
        AssertError<Unit>(result, ErrorCode.ValidationError, EmailPolicy.SameAsCurrentMessage);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.RequestEmailChange"/> returns failure
    /// when the user account is not in an active state.
    /// </summary>
    [Fact]
    public void RequestEmailChange_Should_ReturnFailure_When_UserNotActive()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();

        // Act
        Result<Unit> result = user.RequestEmailChange(user.Email, TokenValue, RevertToken, Duration, this.Clock.UtcNow);

        // Assert
        AssertError<Unit>(result, ErrorCode.ValidationError, UserPolicy.EmailIsNotConfirmedMessage);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.RequestEmailChange"/> correctly initializes security tokens,
    /// changes user status, and raises the appropriate domain event.
    /// </summary>
    [Fact]
    public void RequestEmailChange_Should_SetTokens_Status_And_Event()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();

        // Act
        Result<Unit> result = user.RequestEmailChange(NewEmail, TokenValue, RevertToken, Duration, this.Clock.UtcNow);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Status.Should().Be(UserStatus.Unconfirmed);
        user.CurrentToken.Should().NotBeNull();
        user.CurrentToken.Value.Should().Be(TokenValue);

        EmailChangeRequestedDomainEvent emailEvent = user.DomainEvents
            .OfType<EmailChangeRequestedDomainEvent>()
            .Should().ContainSingle()
            .Subject;

        emailEvent.ConfirmationToken.Should().Be(user.CurrentToken);
        emailEvent.RevertToken.Should().Be(user.RevertToken);

        user.ClearDomainEvents();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmEmailChange"/> successfully updates the email address
    /// and activates the user when a valid token is provided.
    /// </summary>
    [Fact]
    public void ConfirmEmailChange_Should_UpdateEmail_When_Valid()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();
        user.RequestEmailChange(NewEmail, TokenValue, RevertToken, Duration, this.Clock.UtcNow);

        // Act
        Result<Unit> result = user.ConfirmEmailChange(TokenValue, this.Clock.UtcNow);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Email.Value.Should().Be(NewEmail.Value);
        user.Status.Should().Be(UserStatus.Active);
        user.CurrentToken.Should().BeNull();
        user.RevertToken.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmEmailChange"/> updates the security stamp
    /// to invalidate old sessions after a successful email change.
    /// </summary>
    [Fact]
    public void ConfirmEmailChange_Should_UpdateSecurityStamp_When_Valid()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();
        SecurityStamp initialStamp = user.SecurityStamp;

        user.RequestEmailChange(NewEmail, TokenValue, RevertToken, Duration, this.Clock.UtcNow);

        // Act
        user.ConfirmEmailChange(TokenValue, this.Clock.UtcNow);

        // Assert
        user.SecurityStamp.Should().NotBe(initialStamp);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmEmailChange"/> returns failure
    /// when the email change token has expired.
    /// </summary>>
    [Fact]
    public void ConfirmEmailChange_Should_Fail_When_TokenExpired()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();

        user.RequestEmailChange(NewEmail, TokenValue, RevertToken, TimeSpan.FromMinutes(1), this.Clock.UtcNow);

        this.Clock.Advance(TimeSpan.FromMinutes(2));

        // Act
        Result<Unit> result = user.ConfirmEmailChange(TokenValue, this.Clock.UtcNow);

        // Assert
        AssertError<Unit>(result, ErrorCode.Timeout, TokenPolicy.InvalidEmailChangeTokenMessage);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmEmailChange"/> returns failure
    /// when the email change process was not initiated.
    /// </summary>
    [Fact]
    public void ConfirmEmailChange_Should_ReturnFailure_When_NoRequestWasMade()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();

        // Act
        Result<Unit> result = user.ConfirmEmailChange(TokenValue, this.Clock.UtcNow);

        // Assert
        AssertError<Unit>(result, ErrorCode.Timeout, TokenPolicy.InvalidEmailChangeTokenMessage);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmEmailChange"/> returns failure
    /// when an invalid token is provided for an existing request.
    /// </summary>
    [Fact]
    public void ConfirmEmailChange_Should_Fail_When_InvalidToken()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();

        user.RequestEmailChange(NewEmail, TokenValue, RevertToken, TimeSpan.FromMinutes(1), this.Clock.UtcNow);

        this.Clock.Advance(TimeSpan.FromMinutes(2));

        // Act
        Result<Unit> result = user.ConfirmEmailChange("invalidToken", this.Clock.UtcNow);

        // Assert
        AssertError<Unit>(result, ErrorCode.Timeout, TokenPolicy.InvalidEmailChangeTokenMessage);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.RevertEmailChange"/> successfully restores the original email
    /// and forces a password reset for security reasons.
    /// </summary>
    [Fact]
    public void RevertEmailChange_Should_RestoreEmail_And_ResetSecurity()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();

        user.RequestEmailChange(NewEmail, TokenValue, RevertToken, Duration, this.Clock.UtcNow);
        user.ConfirmEmailChange(TokenValue, this.Clock.UtcNow);

        SecurityStamp stampAfterConfirm = user.SecurityStamp;

        // Act
        Result<Unit> result = user.RevertEmailChange(RevertToken, this.Clock.UtcNow);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Email.Value.Should().Be(CurrentEmail);
        user.MustChangePassword.Should().BeTrue();
        user.RevertToken.Should().BeNull();
        user.CurrentToken.Should().BeNull();
        user.SecurityStamp.Should().NotBe(stampAfterConfirm);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.RevertEmailChange"/> returns failure
    /// when the revert token has expired.
    /// </summary>
    [Fact]
    public void RevertEmailChange_Should_ReturnFailure_When_TokenExpired()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();

        user.RequestEmailChange(NewEmail, TokenValue, RevertToken, TimeSpan.FromMinutes(1), this.Clock.UtcNow);

        this.Clock.Advance(TimeSpan.FromMinutes(2));

        // Act
        Result<Unit> result = user.RevertEmailChange(RevertToken, this.Clock.UtcNow);

        // Assert
        AssertError<Unit>(result, ErrorCode.Timeout, TokenPolicy.InvalidEmailRevertTokenMessage);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.RevertEmailChange"/> returns failure
    /// when no revert request is found for the user.
    /// </summary>
    [Fact]
    public void RevertEmailChange_Should_ReturnFailure_When_NoRevertRequestWasMade()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();

        // Act
        Result<Unit> result = user.RevertEmailChange(RevertToken, this.Clock.UtcNow);

        // Assert
        AssertError<Unit>(result, ErrorCode.Timeout, TokenPolicy.InvalidEmailRevertTokenMessage);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.RevertEmailChange"/> succeeds
    /// even when <see cref="UserEntity.ConfirmEmailChange"/> was never called
    /// (i.e. the email was never actually updated).
    /// This documents the intentional behaviour: the revert token is independent of confirmation.
    /// </summary>
    [Fact]
    public void RevertEmailChange_Should_Succeed_When_ConfirmWasNeverCalled()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();
        string originalEmail = user.Email.Value;

        user.RequestEmailChange(NewEmail, TokenValue, RevertToken, Duration, this.Clock.UtcNow);

        // Act
        Result<Unit> result = user.RevertEmailChange(RevertToken, this.Clock.UtcNow);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Email.Value.Should().Be(originalEmail);
        user.MustChangePassword.Should().BeTrue();
        user.Status.Should().Be(UserStatus.Active);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.RequestPasswordReset"/> generates a valid reset token
    /// and raises a password reset requested domain event.
    /// </summary>
    [Fact]
    public void RequestPasswordReset_Should_SetToken_And_Event()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();

        // Act
        user.RequestPasswordReset(TokenValue, TimeSpan.FromMinutes(10), this.Clock.UtcNow);

        // Assert
        user.CurrentToken.Should().NotBeNull();
        user.CurrentToken.Value.Should().Be(TokenValue);

        PasswordResetRequestedDomainEvent emailEvent = user.DomainEvents
            .OfType<PasswordResetRequestedDomainEvent>()
            .Should().ContainSingle()
            .Subject;

        emailEvent.ResetToken.Should().Be(user.CurrentToken);

        user.ClearDomainEvents();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmPasswordReset"/> updates the password hash and security stamp
    /// when a valid reset token is provided.
    /// </summary>
    [Fact]
    public void RequestResetPassword_Should_UpdatePassword_And_SecurityStamp_When_ValidToken()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();
        SecurityStamp initialStamp = user.SecurityStamp;

        user.RequestPasswordReset(TokenValue, Duration, this.Clock.UtcNow);

        PasswordHash newPasswordHash = PasswordHash.Create(NewPasswordHashString);

        // Act
        Result<Unit> result = user.ConfirmPasswordReset(newPasswordHash, TokenValue, this.Clock.UtcNow);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.PasswordHash.Value.Should().Be(NewPasswordHashString);
        user.Status.Should().Be(UserStatus.Active);
        user.SecurityStamp.Should().NotBe(initialStamp);
        user.CurrentToken.Should().BeNull();
        user.MustChangePassword.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.RequestPasswordReset"/> succeeds
    /// for an unconfirmed user, documenting that this is intentional behaviour
    /// (EnsureNotPendingDeletion allows both Active and Unconfirmed states).
    /// </summary>
    [Fact]
    public void RequestPasswordReset_Should_Succeed_When_UserIsUnconfirmed()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();

        // Act
        Result<Unit> result = user.RequestPasswordReset(TokenValue, Duration, this.Clock.UtcNow);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.CurrentToken.Should().NotBeNull();
        user.CurrentToken.Type.Should().Be(UserTokenType.PasswordReset);
        user.Status.Should().Be(UserStatus.Unconfirmed);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmPasswordReset"/> returns failure
    /// when the password reset token has expired.
    /// </summary>
    [Fact]
    public void ConfirmPasswordReset_Should_ReturnFailure_When_TokenExpired()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();

        user.RequestPasswordReset(TokenValue, TimeSpan.FromMinutes(1), this.Clock.UtcNow);
        this.Clock.Advance(TimeSpan.FromMinutes(2));

        PasswordHash newPasswordHash = PasswordHash.Create(NewPasswordHashString);

        // Act
        Result<Unit> result = user.ConfirmPasswordReset(newPasswordHash, TokenValue, this.Clock.UtcNow);

        // Assert
        AssertError<Unit>(result, ErrorCode.Timeout, TokenPolicy.InvalidPasswordResetTokenMessage);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmPasswordReset"/> returns failure
    /// when no password reset was requested for the user.
    /// </summary>
    [Fact]
    public void ConfirmPasswordReset_Should_ReturnFailure_When_NoRequestWasMade()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();

        PasswordHash newPasswordHash = PasswordHash.Create(NewPasswordHashString);

        // Act
        Result<Unit> result = user.ConfirmPasswordReset(newPasswordHash, TokenValue, this.Clock.UtcNow);

        // Assert
        AssertError<Unit>(result, ErrorCode.Timeout, TokenPolicy.InvalidPasswordResetTokenMessage);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmPasswordReset"/> returns failure
    /// when the provided token is invalid.
    /// </summary>
    [Fact]
    public void ConfirmPasswordReset_Should_ReturnFailure_When_TokenIsInvalid()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();

        PasswordHash newPasswordHash = PasswordHash.Create(NewPasswordHashString);

        // Act
        Result<Unit> result = user.ConfirmPasswordReset(newPasswordHash, "invalidToken", this.Clock.UtcNow);

        // Assert
        AssertError<Unit>(result, ErrorCode.Timeout, TokenPolicy.InvalidPasswordResetTokenMessage);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmPasswordReset"/> returns failure
    /// when the new password is identical to the current one.
    /// </summary>
    [Fact]
    public void ConfirmPasswordReset_Should_ReturnFailure_When_PasswordIsSame()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();
        user.RequestPasswordReset(TokenValue, TimeSpan.FromHours(1), this.Clock.UtcNow);

        // Act
        Result<Unit> result = user.ConfirmPasswordReset(user.PasswordHash, TokenValue, this.Clock.UtcNow);

        // Assert
        AssertError<Unit>(result, ErrorCode.ValidationError, PasswordPolicy.SameAsOldMessage);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ChangeFirstName"/> and <see cref="UserEntity.ChangeLastName"/>
    /// successfully update the user's name properties.
    /// </summary>
    [Fact]
    public void ChangeFirstName_And_ChangeLastName_Should_UpdateNames()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();
        FirstName newFirstName = FirstName.Create("Jane");
        LastName newLastName = LastName.Create("Jackson");

        // Act
        Result<bool> changeFirstNameResult = user.ChangeFirstName(newFirstName);
        Result<bool> changeLastNameResult = user.ChangeLastName(newLastName);

        // Assert
        changeFirstNameResult.IsSuccess.Should().BeTrue();
        changeLastNameResult.IsSuccess.Should().BeTrue();
        user.FirstName.Value.Should().Be(newFirstName.Value);
        user.LastName!.Value.Should().Be(newLastName!.Value);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ChangeFirstName"/> returns failure
    /// when the user account is not active.
    /// </summary>
    [Fact]
    public void ChangeFirstName_Should_ReturnFailure_When_UserNotActive()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();
        FirstName newFirstName = FirstName.Create("Jane");

        // Act
        Result<bool> result = user.ChangeFirstName(newFirstName);

        // Assert
        AssertError<bool>(result, ErrorCode.ValidationError, UserPolicy.EmailIsNotConfirmedMessage);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ChangeLastName"/> sets the last name to <see langword="null"/>
    /// when provided with an empty or whitespace string.
    /// </summary>
    [Fact]
    public void ChangeLastName_Should_SetToNull_When_EmptyOrWhitespace()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();
        LastName? newLastName = LastName.CreateOptional(" ");

        // Act
        Result<bool> result = user.ChangeLastName(newLastName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.LastName.Should().BeNull();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ChangeLastName"/> returns failure
    /// when the user account is not active.
    /// </summary>
    [Fact]
    public void ChangeLastName_Should_ReturnFailure_When_UserNotActive()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();
        LastName newLastName = LastName.Create("Jackson");

        // Act
        Result<bool> result = user.ChangeLastName(newLastName);

        // Assert
        AssertError<bool>(result, ErrorCode.ValidationError, UserPolicy.EmailIsNotConfirmedMessage);
    }

    /// <summary>
    /// Verifies that name change methods return <see langword="false"/> in the result value
    /// when the provided names are identical to the current ones.
    /// </summary>
    [Fact]
    public void ChangeLastAndFirstName_Should_False_When_FirstOrLastNameIsTheSame()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();
        FirstName currentFirstName = FirstName.Create(CurrentFirstName);
        LastName currentLastName = LastName.Create(CurrentLastName);

        // Act
        Result<bool> changeFirstNameResult = user.ChangeFirstName(currentFirstName);
        Result<bool> changeLastNameResult = user.ChangeLastName(currentLastName);

        // Assert
        changeFirstNameResult.IsSuccess.Should().BeTrue();
        changeFirstNameResult.Value.Should().BeFalse();
        changeLastNameResult.IsSuccess.Should().BeTrue();
        changeLastNameResult.Value.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ChangePassword"/> successfully updates the password hash
    /// for an active user.
    /// </summary>
    [Fact]
    public void ChangePassword_Should_UpdatePassword_When_CurrentPasswordCorrect()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();
        PasswordHash newPasswordHash = PasswordHash.Create(NewPasswordHashString);

        // Act
        Result<Unit> result = user.ChangePassword(newPasswordHash);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.PasswordHash.Should().Be(newPasswordHash);
    }

    /// <summary>
    /// Verifies that <see cref="UserEntity.ChangePassword"/> returns failure
    /// when the new password hash is the same as the current one.
    /// </summary>
    [Fact]
    public void ChangePassword_Should_ReturnFailure_When_NewPasswordIsSameAsOld()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();
        PasswordHash samePasswordHash = PasswordHash.Create(PasswordHashString);

        // Act
        Result<Unit> result = user.ChangePassword(samePasswordHash);

        // Assert
        AssertError<Unit>(result, ErrorCode.ValidationError, PasswordPolicy.SameAsOldMessage);
    }

    /// <summary>
    /// Tests that changing the password also updates the user's security stamp.
    /// </summary>
    [Fact]
    public void ChangePassword_Should_UpdatePassword_And_SecurityStamp()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();
        SecurityStamp initialStamp = user.SecurityStamp;
        PasswordHash newPasswordHash = PasswordHash.Create(NewPasswordHashString);

        // Act
        user.ChangePassword(newPasswordHash);

        // Assert
        user.PasswordHash.Should().Be(newPasswordHash);
        user.SecurityStamp.Should().NotBe(initialStamp);
        user.MustChangePassword.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ChangePassword"/> returns failure
    /// and does not update the security stamp when the user is not active.
    /// </summary>
    [Fact]
    public void ChangePassword_Should_ReturnFailure_When_UserNotActive()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();
        SecurityStamp initialStamp = user.SecurityStamp;
        PasswordHash newPasswordHash = PasswordHash.Create(NewPasswordHashString);

        // Act
        Result<Unit> result = user.ChangePassword(newPasswordHash);

        // Assert
        AssertError<Unit>(result, ErrorCode.ValidationError, UserPolicy.EmailIsNotConfirmedMessage);
        user.SecurityStamp.Should().Be(initialStamp);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ChangePassword"/> resets the
    /// <see cref="UserEntity.MustChangePassword"/> flag that was set by
    /// <see cref="UserEntity.RevertEmailChange"/>.
    /// </summary>
    [Fact]
    public void ChangePassword_Should_ClearMustChangePassword_After_EmailRevert()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();

        user.RequestEmailChange(NewEmail, TokenValue, RevertToken, Duration, this.Clock.UtcNow);
        user.ConfirmEmailChange(TokenValue, this.Clock.UtcNow);
        user.RevertEmailChange(RevertToken, this.Clock.UtcNow);

        user.MustChangePassword.Should().BeTrue();

        PasswordHash newPasswordHash = PasswordHash.Create(NewPasswordHashString);

        // Act
        Result<Unit> result = user.ChangePassword(newPasswordHash);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.MustChangePassword.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ChangeUserName"/> successfully updates the username.
    /// </summary>
    [Fact]
    public void ChangeUserName_Should_Update_When_ValidVOProvided()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();
        UserName newUserName = UserName.Create("new_unique_name");

        // Act
        user.ChangeUserName(newUserName);

        // Assert
        user.UserName.Should().Be(newUserName);
        user.UserName.Value.Should().Be("new_unique_name");
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ChangeUserName"/> returns failure
    /// when the provided username is identical to the current one.
    /// </summary>
    [Fact]
    public void ChangeUserName_Should_ReturnFailure_When_SameUserNameProvided()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();
        UserName oldUserName = UserName.Create(CurrentUserName);

        // Act
        Result<Unit> result = user.ChangeUserName(oldUserName);

        // Assert
        AssertError<Unit>(result, ErrorCode.ValidationError, UserNamePolicy.SameAsCurrentMessage);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ChangeUserName"/> returns failure
    /// when the user account is not active.
    /// </summary>
    [Fact]
    public void ChangeUserName_Should_ReturnFailure_When_UserNotActive()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();
        UserName oldUserName = UserName.Create(CurrentUserName);

        // Act
        Result<Unit> result = user.ChangeUserName(oldUserName);

        // Assert
        AssertError<Unit>(result, ErrorCode.ValidationError, UserPolicy.EmailIsNotConfirmedMessage);
    }

    /// <summary>
    /// Verifies that <see cref="UserEntity.UpdateUnconfirmedRegistration"/> correctly updates user properties
    /// during the unconfirmed registration phase.
    /// </summary>
    [Fact]
    public void UpdateUnconfirmedRegistration_Should_UpdateProperties_When_EmailNotConfirmed()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();
        FirstName newFirst = FirstName.Create("NewFirst");
        LastName newLast = LastName.Create("NewLast");
        UserName newUsername = UserName.Create("newusername");
        PasswordHash newPasswordHash = PasswordHash.Create(NewPasswordHashString);

        // Act
        user.UpdateUnconfirmedRegistration(
            newFirst,
            newUsername,
            newPasswordHash,
            TokenValue,
            Duration,
            this.Clock.UtcNow,
            newLast);

        // Assert
        user.FirstName.Value.Should().Be(newFirst.Value);
        user.LastName!.Value.Should().Be(newLast!.Value);
        user.UserName.Value.Should().Be(newUsername.Value);
        user.PasswordHash.Value.Should().Be(newPasswordHash.Value);
        user.CurrentToken.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that <see cref="UserEntity.UpdateUnconfirmedRegistration"/> updates the security stamp.
    /// </summary>
    [Fact]
    public void UpdateUnconfirmedRegistration_Should_UpdateSecurityStamp()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();
        SecurityStamp initialStamp = user.SecurityStamp;
        PasswordHash newPasswordHash = PasswordHash.Create(NewPasswordHashString);

        // Act
        user.UpdateUnconfirmedRegistration(
            FirstName.Create("NewFirst"),
            UserName.Create("newusername"),
            newPasswordHash,
            TokenValue,
            Duration,
            this.Clock.UtcNow);

        // Assert
        user.SecurityStamp.Should().NotBe(initialStamp);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.UpdateUnconfirmedRegistration"/> returns failure
    /// if the user's email has already been confirmed.
    /// </summary>
    [Fact]
    public void UpdateUnconfirmedRegistration_Should_ReturnFailure_When_EmailAlreadyConfirmed()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();

        // Act
        Result<Unit> result = user.UpdateUnconfirmedRegistration(
            FirstName.Create("NewFirst"),
            UserName.Create("newusername"),
            PasswordHash.Create(NewPasswordHashString),
            TokenValue,
            Duration,
            this.Clock.UtcNow);

        AssertError<Unit>(result, ErrorCode.ValidationError, UserPolicy.EmailAlreadyConfirmedMessage);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.RequestAccountDeletion"/> marks the account as pending deletion
    /// and raises the appropriate domain event.
    /// </summary>
    [Fact]
    public void RequestAccountDeletion_Should_MarkPendingDeletion_And_RaiseEvent()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();

        // Act
        Result<Unit> result = user.RequestAccountDeletion(this.Clock.UtcNow);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Status.Should().Be(UserStatus.PendingDeletion);
        user.DeletionScheduledAt.Should().NotBeNull();
        user.DeletionScheduledAt.Should().BeAfter(this.Clock.UtcNow);

        user.DomainEvents
            .OfType<AccountDeletionRequestedDomainEvent>()
            .Should().ContainSingle();

        user.ClearDomainEvents();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.RequestAccountDeletion"/> returns failure
    /// when the user account is not active.
    /// </summary>
    [Fact]
    public void RequestAccountDeletion_Should_ReturnFailure_When_UserNotActive()
    {
        // Arrange
        UserEntity user = UserEntityFactory.Create();

        // Act
        Result<Unit> result = user.RequestAccountDeletion(this.Clock.UtcNow);

        // Assert
        AssertError<Unit>(result, ErrorCode.ValidationError, UserPolicy.EmailIsNotConfirmedMessage);
        user.Status.Should().Be(UserStatus.Unconfirmed);
        user.DeletionScheduledAt.Should().BeNull();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.RecoverAccount"/> successfully restores
    /// an account that is pending deletion.
    /// </summary>
    [Fact]
    public void RecoverAccount_Should_RestoreAccount_When_PendingDeletion()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();
        user.RequestAccountDeletion(this.Clock.UtcNow);

        // Act
        Result<Unit> result = user.RecoverAccount(this.Clock.UtcNow);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Status.Should().Be(UserStatus.Active);
        user.DeletionScheduledAt.Should().BeNull();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.RecoverAccount"/> returns failure
    /// when the deletion grace period has already expired.
    /// </summary>
    [Fact]
    public void RecoverAccount_Should_ReturnFailure_When_DeletionPeriodExpired()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();
        user.RequestAccountDeletion(this.Clock.UtcNow);

        this.Clock.Advance(TimeSpan.FromDays(UserPolicy.DeletionDelayInDays + 1));

        // Act
        Result<Unit> result = user.RecoverAccount(this.Clock.UtcNow);

        // Assert
        AssertError<Unit>(result, ErrorCode.InvalidOperation, UserPolicy.DeletionPeriodExpiredMessage);
        user.Status.Should().Be(UserStatus.PendingDeletion);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.RecoverAccount"/> returns failure
    /// when the account is active and not pending deletion.
    /// </summary>
    [Fact]
    public void RecoverAccount_Should_ReturnFailure_When_UserIsActive()
    {
        // Arrange
        UserEntity user = UserEntityFactory.CreateActive();

        // Act
        Result<Unit> result = user.RecoverAccount(this.Clock.UtcNow);

        // Assert
        AssertError<Unit>(result, ErrorCode.ValidationError, UserPolicy.IsActiveMessage);
    }

    private static void AssertError<T>(Result<T> result, ErrorCode code, string message)
    {
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be(code);
        result.Error.Message.Should().Be(message);
    }
}
