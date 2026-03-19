using FluentAssertions;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Enums;
using TodoListApp.Domain.Exceptions;
using TodoListApp.Domain.Test.Common;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Domain.Test.Entities;

/// <summary>
/// Unit tests for the <see cref="UserEntity"/> class.
/// </summary>
public class UserEntityTests
{
    private const string CurrentFirstName = "John";
    private const string CurrentLastName = "Doe";
    private const string CurrentUserName = "jdoe";
    private const string CurrentEmail = "john@example.com";

    private const string TokenValue = "token123";
    private const string RevertToken = "revert_secret";
    private const string ResetToken = "reset_secret";

    private static readonly Email NewEmail = Email.Create("newEmail@example.com");
    private static readonly string PasswordHashString = new('a', 64);
    private static readonly string NewPasswordHashString = new('b', 64);

    private static readonly TimeSpan Duration = TimeSpan.FromHours(1);
    private static readonly DateTime CurrentTime = DateTime.UtcNow;

    /// <summary>
    /// Tests that the constructor creates a user with valid data.
    /// </summary>
    [Fact]
    public void Constructor_Should_CreateUser_When_ValidData()
    {
        // Arrange
        var user = CreateUser();

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
    /// Tests that <see cref="UserEntity.RequestEmailVerification"/> sets token value, expiry, and type correctly.
    /// </summary>
    [Fact]
    public void RequestEmailVerification_Should_SetToken_And_MarkEmailUnconfirmed()
    {
        // Arrange
        var user = CreateUser();

        user.RequestEmailVerification(TokenValue, Duration, CurrentTime);

        // Assert
        user.CurrentToken.Should().NotBeNull();
        user.CurrentToken.Type.Should().Be(UserTokenType.EmailVerification);
        user.EmailConfirmed.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmEmailVerification"/> sets token value, expiry, and type correctly.
    /// </summary>
    [Fact]
    public void ConfirmEmailVerification_Should_SetConfirmed_When_TokenValid()
    {
        // Arrange
        var fakeClock = new FakeClock(CurrentTime);
        var user = CreateUser();

        user.RequestEmailVerification(TokenValue, Duration, fakeClock.UtcNow);

        user.ConfirmEmailVerification(TokenValue, fakeClock.UtcNow);

        // Assert
        user.EmailConfirmed.Should().BeTrue();
        user.CurrentToken.Should().BeNull();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmEmailVerification"/> throws
    /// when the email verification token is expired.
    /// </summary>
    [Fact]
    public void ConfirmEmailVerification_ShoulReturnFailure_When_TokenExpired()
    {
        // Arrange
        var fakeClock = new FakeClock(CurrentTime);
        var user = CreateUser();

        user.RequestEmailVerification(TokenValue, TimeSpan.FromMinutes(1), fakeClock.UtcNow);

        fakeClock.Advance(TimeSpan.FromMinutes(2));

        // Act
        var result = user.ConfirmEmailVerification(TokenValue, fakeClock.UtcNow);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmEmailVerification"/> retuns failure
    /// when the email verification was not requested.
    /// </summary>
    [Fact]
    public void ConfirmEmailVerification_Should_ReturnFailure_When_NoRequestWasMade()
    {
        // Arrange
        var fakeClock = new FakeClock(CurrentTime);
        var user = CreateUser();

        // Act
        var result = user.ConfirmEmailVerification(TokenValue, fakeClock.UtcNow);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmEmailChange"/> updates the email
    /// and confirms it when a valid token is provided.
    /// </summary>
    [Fact]
    public void ConfirmEmailChange_Should_UpdateEmail_When_Valid()
    {
        // Arrange
        var fakeClock = new FakeClock(CurrentTime);
        var user = CreateUser();

        user.RequestEmailChange(NewEmail, TokenValue, RevertToken, Duration, fakeClock.UtcNow);

        user.ConfirmEmailChange(TokenValue, fakeClock.UtcNow);

        // Assert
        user.Email.Value.Should().Be(NewEmail.Value);
        user.EmailConfirmed.Should().BeTrue();
        user.CurrentToken.Should().BeNull();
        user.RevertToken.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmEmailChange"/> updates the email,
    /// confirms it, and updates the security stamp.
    /// </summary>
    [Fact]
    public void ConfirmEmailChange_Should_UpdateSecurityStamp_When_Valid()
    {
        // Arrange
        var fakeClock = new FakeClock(CurrentTime);
        var user = CreateUser();
        var initialStamp = user.SecurityStamp;

        user.RequestEmailChange(NewEmail, TokenValue, RevertToken, Duration, fakeClock.UtcNow);

        // Act
        user.ConfirmEmailChange(TokenValue, fakeClock.UtcNow);

        // Assert
        user.SecurityStamp.Should().NotBe(initialStamp);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmEmailChange"/> throws
    /// when the email change confirmation token is expired.
    /// </summary>
    [Fact]
    public void ConfirmEmailChange_Should_ReturnFailure_When_TokenExpired()
    {
        // Arrange
        var fakeClock = new FakeClock(CurrentTime);
        var user = CreateUser();

        user.RequestEmailChange(NewEmail, TokenValue, RevertToken, TimeSpan.FromMinutes(1), fakeClock.UtcNow);

        fakeClock.Advance(TimeSpan.FromMinutes(2));

        // Act
        var result = user.ConfirmEmailChange(TokenValue, fakeClock.UtcNow);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmEmailChange"/> throws
    /// when the email change was not requested.
    /// </summary>
    [Fact]
    public void ConfirmEmailChange_Should_ReturnFailure_When_NoRequestWasMade()
    {
        // Arrange
        var fakeClock = new FakeClock(CurrentTime);
        var user = CreateUser();

        // Act
        var result = user.ConfirmEmailChange(TokenValue, fakeClock.UtcNow);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="UserEntity.RevertEmailChange"/> successfully restores the
    /// original email address from the token's metadata.
    /// </summary>
    [Fact]
    public void RevertEmailChange_Should_RestoreOldEmail()
    {
        // Arrange
        var user = CreateUser();
        user.RequestEmailChange(NewEmail, TokenValue, RevertToken, Duration, CurrentTime);
        user.ConfirmEmailChange(TokenValue, DateTime.UtcNow);

        // Act
        user.RevertEmailChange(RevertToken, DateTime.UtcNow, ResetToken, TimeSpan.FromHours(1));

        // Assert
        user.Email.Value.Should().Be(CurrentEmail);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.RevertEmailChange"/> successfully restores the
    /// original email address and updates the security stamp.
    /// </summary>
    [Fact]
    public void RevertEmailChange_Should_RestoreOldEmail_And_UpdateSecurityStamp()
    {
        // Arrange
        var user = CreateUser();
        user.RequestEmailChange(NewEmail, TokenValue, RevertToken, Duration, CurrentTime);
        user.ConfirmEmailChange(TokenValue, DateTime.UtcNow);
        var stampAfterConfirm = user.SecurityStamp;

        // Act
        user.RevertEmailChange(RevertToken, DateTime.UtcNow, ResetToken, TimeSpan.FromHours(1));

        // Assert
        user.Email.Value.Should().Be(CurrentEmail);
        user.MustChangePassword.Should().BeTrue();
        user.SecurityStamp.Should().NotBe(stampAfterConfirm);
    }

    /// <summary>
    /// Verifies that <see cref="UserEntity.RevertEmailChange"/> generates a new security stamp
    /// that differs from the one generated during the (unauthorized) email confirmation,
    /// effectively kicking out the attacker.
    /// </summary>
    [Fact]
    public void RevertEmailChange_Should_InvalidatePreviousSecurityStamp()
    {
        // Arrange
        var user = CreateUser();
        user.RequestEmailChange(NewEmail, TokenValue, RevertToken, Duration, CurrentTime);
        user.ConfirmEmailChange(TokenValue, DateTime.UtcNow);
        var stampAfterHacker = user.SecurityStamp;

        // Act
        user.RevertEmailChange(RevertToken, DateTime.UtcNow, ResetToken, TimeSpan.FromHours(1));

        // Assert
        user.SecurityStamp.Should().NotBe(stampAfterHacker);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.RevertEmailChange"/> sets the <see cref="UserEntity.MustChangePassword"/>
    /// flag to true, forcing the user to secure their account upon next login.
    /// </summary>
    [Fact]
    public void RevertEmailChange_Should_SetMustChangePassword()
    {
        // Arrange
        var user = CreateUser();
        user.RequestEmailChange(NewEmail, TokenValue, RevertToken, Duration, CurrentTime);
        user.ConfirmEmailChange(TokenValue, DateTime.UtcNow);

        // Act
        user.RevertEmailChange(RevertToken, DateTime.UtcNow, ResetToken, TimeSpan.FromHours(1));

        // Assert
        user.MustChangePassword.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="UserEntity.RevertEmailChange"/> clears the revert token
    /// after a successful restoration of the original email.
    /// </summary>
    [Fact]
    public void RevertEmailChange_Should_ClearRevertToken()
    {
        // Arrange
        var user = CreateUser();
        user.RequestEmailChange(NewEmail, TokenValue, RevertToken, Duration, CurrentTime);
        user.ConfirmEmailChange(TokenValue, DateTime.UtcNow);

        // Act
        user.RevertEmailChange(RevertToken, DateTime.UtcNow, ResetToken, TimeSpan.FromHours(1));

        // Assert
        user.RevertToken.Should().BeNull();
    }

    /// <summary>
    /// Verifies that <see cref="UserEntity.RevertEmailChange"/> issues a mandatory password reset token
    /// as part of the account recovery process.
    /// </summary>
    [Fact]
    public void RevertEmailChange_Should_SetPasswordResetToken()
    {
        // Arrange
        var fakeClock = new FakeClock(CurrentTime);
        var user = CreateUser();
        user.RequestEmailChange(NewEmail, TokenValue, RevertToken, Duration, CurrentTime);
        user.ConfirmEmailChange(TokenValue, fakeClock.UtcNow);

        // Act
        user.RevertEmailChange(RevertToken, fakeClock.UtcNow, ResetToken, TimeSpan.FromHours(1));

        // Assert
        user.CurrentToken.Should().NotBeNull();
        user.CurrentToken.Type.Should().Be(UserTokenType.PasswordReset);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmEmailChange"/> returns failure
    /// when the email change confirmation token is expired.
    /// </summary>
    [Fact]
    public void RevertEmailChange_Should_ReturnFailure_When_TokenExpired()
    {
        // Arrange
        var fakeClock = new FakeClock(CurrentTime);
        var user = CreateUser();

        user.RequestEmailChange(NewEmail, TokenValue, RevertToken, TimeSpan.FromMinutes(1), fakeClock.UtcNow);

        fakeClock.Advance(TimeSpan.FromMinutes(2));

        // Act
        var result = user.RevertEmailChange(RevertToken, fakeClock.UtcNow, ResetToken, TimeSpan.FromHours(1));

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.RevertEmailChange"/> throws a <see cref="DomainException"/>
    /// when no revert token exists or the process was never initiated.
    /// </summary>
    [Fact]
    public void RevertEmailChange_Should_ReturnFailure_When_NoRevertRequestWasMade()
    {
        // Arrange
        var user = CreateUser();

        var expiredTime = DateTime.UtcNow;

        // Act
        var result = user.RevertEmailChange(RevertToken, expiredTime, ResetToken, TimeSpan.FromHours(1));

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmPasswordReset"/> updates the password hash
    /// when a valid password reset token is provided.
    /// </summary>
    [Fact]
    public void ResetPassword_Should_UpdatePassword_And_SecurityStamp_When_ValidToken()
    {
        // Arrange
        var fakeClock = new FakeClock(CurrentTime);
        var user = CreateUser();
        var initialStamp = user.SecurityStamp;

        user.RequestPasswordReset(TokenValue, Duration, fakeClock.UtcNow);

        var newPasswordHash = PasswordHash.Create(NewPasswordHashString);

        user.ConfirmPasswordReset(newPasswordHash, TokenValue, fakeClock.UtcNow);

        // Assert
        user.PasswordHash.Value.Should().Be(NewPasswordHashString);
        user.EmailConfirmed.Should().BeTrue();
        user.SecurityStamp.Should().NotBe(initialStamp);
        user.CurrentToken.Should().BeNull();
        user.MustChangePassword.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmPasswordReset"/> throws
    /// when the password reset confirmation token is expired.
    /// </summary>
    [Fact]
    public void ConfirmPasswordReset_ShoulReturnFailure_When_TokenExpired()
    {
        // Arrange
        var fakeClock = new FakeClock(CurrentTime);
        var user = CreateUser();

        user.RequestPasswordReset(TokenValue, TimeSpan.FromMinutes(1), fakeClock.UtcNow);
        fakeClock.Advance(TimeSpan.FromMinutes(2));

        var newPasswordHash = PasswordHash.Create(NewPasswordHashString);

        // Act
        var result = user.ConfirmPasswordReset(newPasswordHash, TokenValue, fakeClock.UtcNow);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmPasswordReset"/> throws a <see cref="DomainException"/>
    /// when no password reset request exists for the user.
    /// </summary>
    [Fact]
    public void ConfirmPasswordReset_Should_ReturnFailure_When_NoRequestWasMade()
    {
        // Arrange
        var user = CreateUser();
        var expiredTime = DateTime.UtcNow;

        var newPasswordHash = PasswordHash.Create(NewPasswordHashString);

        // Act
        var result = user.ConfirmPasswordReset(newPasswordHash, TokenValue, expiredTime);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ChangeFirstName"/> and <see cref="UserEntity.ChangeLastName"/>
    /// successfully update the user's names and return <see langword="true"/> when different values are provided.
    /// </summary>
    [Fact]
    public void ChangeFirstName_And_ChangeLastName_Should_UpdateNames()
    {
        // Arrange
        var user = CreateUser();
        var newFirstName = FirstName.Create("Jane");
        var newLastName = LastName.Create("Jane");

        // Act
        var changeFirstNameResult = user.ChangeFirstName(newFirstName);
        var changeLastNameResult = user.ChangeLastName(newLastName);

        // Assert
        user.FirstName.Value.Should().Be(newFirstName.Value);
        user.LastName!.Value.Should().Be(newLastName!.Value);
        changeFirstNameResult.Should().BeTrue();
        changeLastNameResult.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ChangeLastName"/> sets last name to null
    /// when the new last name is empty or whitespace.
    /// </summary>
    [Fact]
    public void ChangeLastName_Should_SetToNull_When_EmptyOrWhitespace()
    {
        // Arrange
        var user = CreateUser();
        var newLastName = LastName.CreateOptional(" ");

        // Act
        user.ChangeLastName(newLastName);

        // Assert
        user.LastName.Should().BeNull();
    }

    /// <summary>
    /// Verifies that <see cref="UserEntity.ChangeFirstName"/> and <see cref="UserEntity.ChangeLastName"/>
    /// return <see langword="false"/> when the provided names are identical to the current ones.
    /// </summary>
    [Fact]
    public void ChangeLastAndFirstName_Should_ReturnFalse_When_FirstOrLastNameIsTheSame()
    {
        // Arrange
        var user = CreateUser();
        var currentFirstName = FirstName.Create(CurrentFirstName);
        var currentLastName = LastName.Create(CurrentLastName);

        // Act
        var changeFirstNameResult = user.ChangeFirstName(currentFirstName);
        var changeLastNameResult = user.ChangeLastName(currentLastName);

        // Assert
        changeFirstNameResult.Should().BeFalse();
        changeLastNameResult.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ChangePassword"/> updates the password hash
    /// when the current password hash is provided correctly.
    /// </summary>
    [Fact]
    public void ChangePassword_Should_UpdatePassword_When_CurrentPasswordCorrect()
    {
        // Arrange
        var user = CreateUser();
        var newPasswordHash = PasswordHash.Create(NewPasswordHashString);

        // Act
        user.ChangePassword(newPasswordHash);

        // Assert
        user.PasswordHash.Should().Be(newPasswordHash);
    }

    /// <summary>
    /// Verifies that <see cref="UserEntity.ChangePassword"/> throws a <see cref="DomainException"/>
    /// when the new password hash is the same as the current one.
    /// </summary>>
    [Fact]
    public void ChangePassword_Should_Throw_When_NewPasswordIsSameAsOld()
    {
        // Arrange
        var user = CreateUser();
        var samePasswordHash = PasswordHash.Create(PasswordHashString);

        // Act & Assert
        user.Invoking(u => u.ChangePassword(samePasswordHash))
            .Should().Throw<DomainException>();
    }

    /// <summary>
    /// Tests that the security stamp is initialized and changes after password update.
    /// </summary>
    [Fact]
    public void ChangePassword_Should_UpdatePassword_And_SecurityStamp()
    {
        // Arrange
        var user = CreateUser();
        var initialStamp = user.SecurityStamp;
        var newPasswordHash = PasswordHash.Create(NewPasswordHashString);

        // Act
        user.ChangePassword(newPasswordHash);

        // Assert
        user.PasswordHash.Should().Be(newPasswordHash);
        user.SecurityStamp.Should().NotBe(initialStamp);
        user.MustChangePassword.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ChangeUserName"/> updates the username
    /// when a valid and different new username is provided.
    /// </summary>
    [Fact]
    public void ChangeUserName_Should_Update_When_ValidVOProvided()
    {
        // Arrange
        var user = CreateUser();
        var newUserName = UserName.Create("new_unique_name");

        // Act
        user.ChangeUserName(newUserName);

        // Assert
        user.UserName.Should().Be(newUserName);
        user.UserName.Value.Should().Be("new_unique_name");
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ChangeUserName"/> throws a <see cref="DomainException"/>
    /// when the provided username is identical to the current one.
    /// </summary>
    [Fact]
    public void ChangeUserName_Should_ReturnFailure_When_SameUserNameProvided()
    {
        // Arrange
        var user = CreateUser();
        var oldUserName = UserName.Create(CurrentUserName);

        // Act
        var result = user.ChangeUserName(oldUserName);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmEmailVerification"/> throws a <see cref="DomainException"/>
    /// when an invalid or incorrect token is provided.
    /// </summary>
    [Fact]
    public void ConfirmEmailVerification_Should_Throw_When_TokenInvalid()
    {
        // Arrange
        var user = CreateUser();
        user.RequestEmailVerification(TokenValue, Duration, CurrentTime);

        // Act
        var result = user.ConfirmEmailVerification("invalidtoken", DateTime.UtcNow);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="UserEntity.UpdateUnconfirmedRegistration"/> correctly updates all user properties
    /// and generates a new security token when the user's email has not yet been confirmed.
    /// </summary>
    [Fact]
    public void UpdateUnconfirmedRegistration_Should_UpdateProperties_When_EmailNotConfirmed()
    {
        var user = CreateUser();
        var newFirst = FirstName.Create("NewFirst");
        var newLast = LastName.Create("NewLast");
        var newUsername = UserName.Create("newusername");
        var newPasswordHash = PasswordHash.Create(NewPasswordHashString);

        user.UpdateUnconfirmedRegistration(
            newFirst,
            newUsername,
            newPasswordHash,
            TokenValue,
            Duration,
            CurrentTime,
            newLast);

        user.FirstName.Value.Should().Be(newFirst.Value);
        user.LastName!.Value.Should().Be(newLast!.Value);
        user.UserName.Value.Should().Be(newUsername.Value);
        user.PasswordHash.Value.Should().Be(newPasswordHash.Value);
        user.CurrentToken.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that <see cref="UserEntity.UpdateUnconfirmedRegistration"/> updates the security stamp,
    /// ensuring that any previous sessions or tokens are invalidated when registration details change.
    /// </summary>
    [Fact]
    public void UpdateUnconfirmedRegistration_Should_UpdateSecurityStamp()
    {
        var user = CreateUser();
        var initialStamp = user.SecurityStamp;
        var newPasswordHash = PasswordHash.Create(NewPasswordHashString);

        user.UpdateUnconfirmedRegistration(
            FirstName.Create("NewFirst"),
            UserName.Create("newusername"),
            newPasswordHash,
            TokenValue,
            Duration,
            CurrentTime);

        user.SecurityStamp.Should().NotBe(initialStamp);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.UpdateUnconfirmedRegistration"/> throws a <see cref="DomainException"/>
    /// if an attempt is made to update registration details after the email has already been confirmed.
    /// </summary>
    [Fact]
    public void UpdateUnconfirmedRegistration_Should_Throw_When_EmailAlreadyConfirmed()
    {
        var user = CreateUser();
        user.RequestEmailVerification(TokenValue, Duration, CurrentTime);
        user.ConfirmEmailVerification(TokenValue, CurrentTime);

        var act = () => user.UpdateUnconfirmedRegistration(
            FirstName.Create("NewFirst"),
            UserName.Create("newusername"),
            PasswordHash.Create(NewPasswordHashString),
            TokenValue,
            Duration,
            CurrentTime);

        act.Should().Throw<DomainException>();
    }

    private static UserEntity CreateUser()
        => new(
            FirstName.Create(CurrentFirstName),
            UserName.Create(CurrentUserName),
            Email.Create(CurrentEmail),
            PasswordHash.Create(PasswordHashString),
            LastName.Create(CurrentLastName));
}
