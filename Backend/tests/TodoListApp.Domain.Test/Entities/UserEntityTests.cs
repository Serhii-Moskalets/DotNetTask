using FluentAssertions;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Enums;
using TodoListApp.Domain.Exceptions;
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

    private readonly Email _newEmail = Email.Create("newEmail@example.com");
    private readonly string _passwordHashString = new('a', 64);
    private readonly string _newPasswordHashString = new('b', 64);

    private readonly TimeSpan _duration = TimeSpan.FromHours(1);

    /// <summary>
    /// Tests that the constructor creates a user with valid data.
    /// </summary>
    [Fact]
    public void Constructor_Should_CreateUser_When_ValidData()
    {
        // Arrange
        var user = this.CreateUser();

        // Assert
        user.FirstName.Value.Should().Be(CurrentFirstName);
        user.LastName!.Value.Should().Be(CurrentLastName);
        user.UserName.Value.Should().Be(CurrentUserName);
        user.Email.Value.Should().Be(CurrentEmail);
        user.PasswordHash.Value.Should().Be(this._passwordHashString);
        user.SecurityStamp.Should().NotBeNull();
        user.SecurityStamp.Value.Should().NotBeNullOrWhiteSpace();
        user.MustChangePassword.Should().BeFalse();
    }

    /// <summary>
    /// Tests that the <see cref="UserEntity"/> constructor throws an <see cref="DomainException"/>
    /// when the password hash is null, empty, or whitespace.
    /// </summary>
    /// <param name="passwordHash">The password hash to test.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_Should_Throw_When_PasswordHashIsInvalid(string? passwordHash)
    {
        // Act
        var act = () => new UserEntity(CurrentFirstName, CurrentUserName, CurrentEmail, passwordHash!);

        // Assert
        act.Should().Throw<DomainException>();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.RequestEmailVerification"/> sets token value, expiry, and type correctly.
    /// </summary>
    [Fact]
    public void RequestEmailVerification_Should_SetToken_And_MarkEmailUnconfirmed()
    {
        // Arrange
        var user = this.CreateUser();

        user.RequestEmailVerification(TokenValue, this._duration);

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
        var user = this.CreateUser();
        user.RequestEmailVerification(TokenValue, this._duration);
        var currentTime = DateTime.UtcNow;

        user.ConfirmEmailVerification(TokenValue, currentTime);

        // Assert
        user.EmailConfirmed.Should().BeTrue();
        user.CurrentToken.Should().BeNull();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmEmailVerification"/> throws
    /// when the email verification token is expired.
    /// </summary>
    [Fact]
    public void ConfirmEmailVerification_ShouldThrow_When_TokenExpired()
    {
        // Arrange
        var user = this.CreateUser();
        user.RequestEmailVerification(TokenValue, TimeSpan.FromMinutes(1));

        var expiredTime = DateTime.UtcNow.AddMinutes(2);

        // Act
        var result = () => user.ConfirmEmailVerification(TokenValue, expiredTime);

        // Assert
        result.Should().Throw<DomainException>();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmEmailVerification"/> throws
    /// when the email verification was not requested.
    /// </summary>
    [Fact]
    public void ConfirmEmailVerification_ShouldThrow_When_NoRequestWasMade()
    {
        // Arrange
        var user = this.CreateUser();
        var expiredTime = DateTime.UtcNow;

        // Act
        var result = () => user.ConfirmEmailVerification(TokenValue, expiredTime);

        // Assert
        result.Should().Throw<DomainException>();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmEmailChange"/> updates the email
    /// and confirms it when a valid token is provided.
    /// </summary>
    [Fact]
    public void ConfirmEmailChange_Should_UpdateEmail_When_Valid()
    {
        // Arrange
        var user = this.CreateUser();

        user.RequestEmailChange(this._newEmail, TokenValue, RevertToken, this._duration);

        user.ConfirmEmailChange(TokenValue, DateTime.UtcNow);

        // Assert
        user.Email.Value.Should().Be(this._newEmail.Value);
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
        var user = this.CreateUser();
        var initialStamp = user.SecurityStamp;

        user.RequestEmailChange(this._newEmail, TokenValue, RevertToken, this._duration);

        // Act
        user.ConfirmEmailChange(TokenValue, DateTime.UtcNow);

        // Assert
        user.SecurityStamp.Should().NotBe(initialStamp);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmEmailChange"/> throws
    /// when the email change confirmation token is expired.
    /// </summary>
    [Fact]
    public void ConfirmEmailChange_ShouldThrow_When_TokenExpired()
    {
        // Arrange
        var user = this.CreateUser();

        user.RequestEmailChange(this._newEmail, TokenValue, RevertToken, TimeSpan.FromMinutes(1));

        var expiredTime = DateTime.UtcNow.AddMinutes(2);

        // Act
        var result = () => user.ConfirmEmailChange(TokenValue, expiredTime);

        // Assert
        result.Should().Throw<DomainException>();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmEmailChange"/> throws
    /// when the email change was not requested.
    /// </summary>
    [Fact]
    public void ConfirmEmailChange_ShouldThrow_When_NoRequestWasMade()
    {
        // Arrange
        var user = this.CreateUser();
        var expiredTime = DateTime.UtcNow;

        // Act
        var result = () => user.ConfirmEmailChange(TokenValue, expiredTime);

        // Assert
        result.Should().Throw<DomainException>();
    }

    /// <summary>
    /// Verifies that <see cref="UserEntity.RevertEmailChange"/> successfully restores the
    /// original email address from the token's metadata.
    /// </summary>
    [Fact]
    public void RevertEmailChange_Should_RestoreOldEmail()
    {
        // Arrange
        var user = this.CreateUser();
        user.RequestEmailChange(this._newEmail, TokenValue, RevertToken, this._duration);
        user.ConfirmEmailChange(TokenValue, DateTime.UtcNow);

        // Act
        user.RevertEmailChange(RevertToken, DateTime.UtcNow);

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
        var user = this.CreateUser();
        user.RequestEmailChange(this._newEmail, TokenValue, RevertToken, this._duration);
        user.ConfirmEmailChange(TokenValue, DateTime.UtcNow);
        var stampAfterConfirm = user.SecurityStamp;

        // Act
        user.RevertEmailChange(RevertToken, DateTime.UtcNow);

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
        var user = this.CreateUser();
        user.RequestEmailChange(this._newEmail, TokenValue, RevertToken, this._duration);
        user.ConfirmEmailChange(TokenValue, DateTime.UtcNow);
        var stampAfterHacker = user.SecurityStamp;

        // Act
        user.RevertEmailChange(RevertToken, DateTime.UtcNow);

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
        var user = this.CreateUser();
        user.RequestEmailChange(this._newEmail, TokenValue, RevertToken, this._duration);
        user.ConfirmEmailChange(TokenValue, DateTime.UtcNow);

        // Act
        user.RevertEmailChange(RevertToken, DateTime.UtcNow);

        // Assert
        user.MustChangePassword.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="UserEntity.RevertEmailChange"/> clears both the current
    /// confirmation token and the revert token after a successful restoration.
    /// </summary>
    [Fact]
    public void RevertEmailChange_Should_ClearTokens()
    {
        // Arrange
        var user = this.CreateUser();
        user.RequestEmailChange(this._newEmail, TokenValue, RevertToken, this._duration);
        user.ConfirmEmailChange(TokenValue, DateTime.UtcNow);

        // Act
        user.RevertEmailChange(RevertToken, DateTime.UtcNow);

        // Assert
        user.CurrentToken.Should().BeNull();
        user.RevertToken.Should().BeNull();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmEmailChange"/> throws
    /// when the email change confirmation token is expired.
    /// </summary>
    [Fact]
    public void RevertEmailChange_ShouldThrow_When_TokenExpired()
    {
        // Arrange
        var user = this.CreateUser();

        user.RequestEmailChange(this._newEmail, TokenValue, RevertToken, TimeSpan.FromMinutes(1));

        var expiredTime = DateTime.UtcNow.AddMinutes(2);

        // Act
        var result = () => user.RevertEmailChange(RevertToken, expiredTime);

        // Assert
        result.Should().Throw<DomainException>();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.RevertEmailChange"/> throws a <see cref="DomainException"/>
    /// when no revert token exists or the process was never initiated.
    /// </summary>
    [Fact]
    public void RevertEmailChange_ShouldThrow_When_NoRevertRequestWasMade()
    {
        // Arrange
        var user = this.CreateUser();

        var expiredTime = DateTime.UtcNow;

        // Act
        var result = () => user.RevertEmailChange(RevertToken, expiredTime);

        // Assert
        result.Should().Throw<DomainException>();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmPasswordReset"/> updates the password hash
    /// when a valid password reset token is provided.
    /// </summary>
    [Fact]
    public void ResetPassword_Should_UpdatePassword_And_SecurityStamp_When_ValidToken()
    {
        // Arrange
        var user = this.CreateUser();
        var initialStamp = user.SecurityStamp;

        user.RequestPasswordReset(TokenValue, this._duration);

        var newPasswordHash = PasswordHash.Create(this._newPasswordHashString);

        user.ConfirmPasswordReset(newPasswordHash, TokenValue, DateTime.UtcNow);

        // Assert
        user.PasswordHash.Value.Should().Be(this._newPasswordHashString);
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
    public void ConfirmPasswordReset_ShouldThrow_When_TokenExpired()
    {
        // Arrange
        var user = this.CreateUser();

        user.RequestPasswordReset(TokenValue, TimeSpan.FromMinutes(1));
        var expiredTime = DateTime.UtcNow.AddMinutes(2);

        var newPasswordHash = PasswordHash.Create(this._newPasswordHashString);

        // Act
        var result = () => user.ConfirmPasswordReset(newPasswordHash, TokenValue, expiredTime);

        // Assert
        result.Should().Throw<DomainException>();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmPasswordReset"/> throws a <see cref="DomainException"/>
    /// when no password reset request exists for the user.
    /// </summary>
    [Fact]
    public void ConfirmPasswordReset_ShouldThrow_When_NoRequestWasMade()
    {
        // Arrange
        var user = this.CreateUser();
        var expiredTime = DateTime.UtcNow;

        var newPasswordHash = PasswordHash.Create(this._newPasswordHashString);

        // Act
        var result = () => user.ConfirmPasswordReset(newPasswordHash, TokenValue, expiredTime);

        // Assert
        result.Should().Throw<DomainException>();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ChangeFirstName"/> and <see cref="UserEntity.ChangeLastName"/>
    /// successfully update the user's names and return <see langword="true"/> when different values are provided.
    /// </summary>
    [Fact]
    public void ChangeFirstName_And_ChangeLastName_Should_UpdateNames()
    {
        // Arrange
        var user = this.CreateUser();
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
        var user = this.CreateUser();
        var newLastName = LastName.Create(" ");

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
    public void ChangeLastAndFirstName_ShouldReturnFalse_When_FirstOrLastNameIsTheSame()
    {
        // Arrange
        var user = this.CreateUser();
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
        var user = this.CreateUser();
        var newPasswordHash = PasswordHash.Create(this._newPasswordHashString);

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
        var user = this.CreateUser();
        var samePasswordHash = PasswordHash.Create(this._passwordHashString);

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
        var user = this.CreateUser();
        var initialStamp = user.SecurityStamp;
        var newPasswordHash = PasswordHash.Create(this._newPasswordHashString);

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
        var user = this.CreateUser();
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
    public void ChangeUserName_Should_Throw_When_SameUserNameProvided()
    {
        // Arrange
        var user = this.CreateUser();
        var oldUserName = UserName.Create(CurrentUserName);

        // Act
        var result = () => user.ChangeUserName(oldUserName);

        // Assert
        result.Should().Throw<DomainException>();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmEmailVerification"/> throws a <see cref="DomainException"/>
    /// when an invalid or incorrect token is provided.
    /// </summary>
    [Fact]
    public void ConfirmEmailVerification_Should_Throw_When_TokenInvalid()
    {
        // Arrange
        var user = this.CreateUser();
        user.RequestEmailVerification(TokenValue, this._duration);

        // Act & Assert
        user.Invoking(u => u.ConfirmEmailVerification("invalidtoken", DateTime.UtcNow))
            .Should().Throw<DomainException>();
    }

    private UserEntity CreateUser()
        => new(CurrentFirstName, CurrentUserName, CurrentEmail, this._passwordHashString, CurrentLastName);
}
