using FluentAssertions;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Enums;
using TodoListApp.Domain.Exceptions;

namespace TodoListApp.Domain.Test.Entities;

/// <summary>
/// Unit tests for the <see cref="UserEntity"/> class.
/// </summary>
public class UserEntityTests
{
    private const string FirstName = "John";
    private const string LastName = "Doe";
    private const string UserName = "jdoe";
    private const string Email = "john@example.com";
    private const string TokenValue = "token123";
    private readonly string _passwordHash = new('a', 64);
    private readonly string _newPasswordHash = new('b', 64);
    private readonly TimeSpan _duration = TimeSpan.FromHours(1);

    /// <summary>
    /// Tests that the constructor creates a user with valid data.
    /// </summary>
    [Fact]
    public void Constructor_Should_CreateUser_When_ValidData()
    {
        // Arrange
        var user = new UserEntity(FirstName, UserName, Email, this._passwordHash, LastName);

        // Assert
        user.FirstName.Value.Should().Be(FirstName);
        user.LastName!.Value.Should().Be(LastName);
        user.UserName.Value.Should().Be(UserName);
        user.Email.Value.Should().Be(Email);
        user.PasswordHash.Value.Should().Be(this._passwordHash);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.RequestEmailVerification"/> sets token value, expiry, and type correctly.
    /// </summary>
    [Fact]
    public void RequestEmailVerification_Should_SetToken_And_MarkEmailUnconfirmed()
    {
        // Arrange
        var user = new UserEntity(FirstName, UserName, Email, this._passwordHash);

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
        var user = new UserEntity(FirstName, UserName, Email, this._passwordHash);
        user.RequestEmailVerification(TokenValue, this._duration);
        var currentTime = DateTime.UtcNow;

        user.ConfirmEmailVerification(TokenValue, currentTime);

        // Assert
        user.EmailConfirmed.Should().BeTrue();
        user.CurrentToken.Should().BeNull();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmEmailChange"/> updates the email
    /// and confirms it when a valid token is provided.
    /// </summary>
    [Fact]
    public void ConfirmEmailChange_Should_UpdateEmail_When_Valid()
    {
        // Arrange
        var user = new UserEntity(FirstName, UserName, Email, this._passwordHash);
        const string newEmail = "new@example.com";
        user.RequestEmailChange(newEmail, TokenValue, this._duration);

        user.ConfirmEmailChange(TokenValue, DateTime.UtcNow);

        // Assert
        user.Email.Value.Should().Be(newEmail);
        user.EmailConfirmed.Should().BeTrue();
        user.CurrentToken.Should().BeNull();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.RequestPasswordReset"/> throws a
    /// <see cref="DomainException"/> when the user's email is not confirmed.
    /// </summary>
    [Fact]
    public void RequestPasswordReset_Should_Throw_When_EmailNotConfirmed()
    {
        // Arrange
        var user = new UserEntity(FirstName, UserName, Email, this._passwordHash);

        // Act
        var act = () => user.RequestPasswordReset(TokenValue, this._duration);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Email must be confirmed.");
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmPasswordReset"/> updates the password hash
    /// when a valid password reset token is provided.
    /// </summary>
    [Fact]
    public void ResetPassword_Should_UpdatePassword_When_ValidToken()
    {
        // Arrange
        var user = new UserEntity(FirstName, UserName, Email, this._passwordHash);
        user.RequestEmailVerification(TokenValue, this._duration);
        user.ConfirmEmailVerification(TokenValue, DateTime.UtcNow);

        user.RequestPasswordReset(TokenValue, this._duration);

        user.ConfirmPasswordReset(this._newPasswordHash, TokenValue, DateTime.UtcNow);

        // Assert
        user.PasswordHash.Value.Should().Be(this._newPasswordHash);
        user.EmailConfirmed.Should().BeTrue();
        user.CurrentToken.Should().BeNull();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ChangeFirstName"/> and <see cref="UserEntity.ChangeLastName"/> updates first and last names.
    /// </summary>
    [Fact]
    public void ChangeFirstName_And_ChangeLastName_Should_UpdateNames()
    {
        // Arrange
        var user = new UserEntity(FirstName, UserName, Email, this._passwordHash);

        // Act
        user.ChangeFirstName("Jane");
        user.ChangeLastName("Smith");

        // Assert
        user.FirstName.Value.Should().Be("Jane");
        user.LastName!.Value.Should().Be("Smith");
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ChangeFirstName"/> throws a
    /// <see cref="DomainException"/> when the new first name is null, empty, or whitespace.
    /// </summary>
    /// <param name="invalidName">The new first name to test.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void UpdateFirst_ShouldThrow_When_Invalid(string? invalidName)
    {
        // Arrange
        var user = new UserEntity(FirstName, UserName, Email, this._passwordHash);

        // Act
        var act = () => user.ChangeFirstName(invalidName!);

        // Assert
        act.Should().Throw<DomainException>();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ChangeLastName"/> sets last name to null
    /// when the new last name is empty or whitespace.
    /// </summary>
    [Fact]
    public void ChangeLastName_Should_SetToNull_When_EmptyOrWhitespace()
    {
        // Arrange
        var user = new UserEntity(FirstName, UserName, Email, this._passwordHash, "OldName");

        // Act
        user.ChangeLastName(" ");

        // Assert
        user.LastName.Should().BeNull();
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
        var act = () => new UserEntity(FirstName, UserName, Email, passwordHash!);

        // Assert
        act.Should().Throw<DomainException>();
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ChangePassword"/> updates the password hash
    /// when the current password hash is provided correctly.
    /// </summary>
    [Fact]
    public void ChangePassword_Should_UpdatePassword_When_CurrentPasswordCorrect()
    {
        // Arrange
        var user = new UserEntity(FirstName, UserName, Email, this._passwordHash);

        // Act
        user.ChangePassword(this._passwordHash, this._newPasswordHash);

        // Assert
        user.PasswordHash.Value.Should().Be(this._newPasswordHash);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ChangePassword"/> throws a <see cref="DomainException"/>
    /// when the provided current password hash does not match the stored one.
    /// </summary>
    [Fact]
    public void ChangePassword_Should_Throw_When_CurrentPasswordIncorrect()
    {
        // Arrange
        var user = new UserEntity(FirstName, UserName, Email, this._passwordHash);

        // Act & Assert
        user.Invoking(u => u.ChangePassword("wronghash", this._newPasswordHash))
            .Should().Throw<DomainException>()
            .WithMessage("Current password is incorrect.");
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ChangeUserName"/> updates the username
    /// when a valid and different new username is provided.
    /// </summary>
    [Fact]
    public void ChangeUserName_Should_Update_When_ValidVOProvided()
    {
        // Arrange
        var user = new UserEntity(FirstName, UserName, Email, this._passwordHash);
        var newUserName = Domain.ValueObjects.UserName.Create("new_unique_name");

        // Act
        user.ChangeUserName(newUserName);

        // Assert
        user.UserName.Should().Be(newUserName);
        user.UserName.Value.Should().Be("new_unique_name");
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmEmailVerification"/> throws a <see cref="DomainException"/>
    /// when an invalid or incorrect token is provided.
    /// </summary>
    [Fact]
    public void ConfirmEmailVerification_Should_Throw_When_TokenInvalid()
    {
        // Arrange
        var user = new UserEntity(FirstName, UserName, Email, this._passwordHash);
        user.RequestEmailVerification(TokenValue, this._duration);

        // Act & Assert
        user.Invoking(u => u.ConfirmEmailVerification("invalidtoken", DateTime.UtcNow))
            .Should().Throw<DomainException>();
    }
}
