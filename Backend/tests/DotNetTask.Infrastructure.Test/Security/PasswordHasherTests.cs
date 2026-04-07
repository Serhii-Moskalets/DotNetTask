using DotNetTask.Infrastructure.Security;
using FluentAssertions;

namespace DotNetTask.Infrastructure.Test.Security;

/// <summary>
/// Unit tests for the <see cref="PasswordHasher"/> class to ensure secure password hashing and verification.
/// </summary>
public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    /// <summary>
    /// Verifies that <see cref="PasswordHasher.HashPassword"/> generates a valid 60-character
    /// BCrypt hash and that the password can be successfully verified against it.
    /// </summary>
    /// <param name="password">The plain-text password to hash and verify.</param>
    [Theory]
    [InlineData("MyPassword123#")]
    [InlineData("Very_Long_Password_With_Spaces_And_Symbols_#1234567890")]
    public void HashPassword_ShouldGenerateValidHash(string password)
    {
        // Act
        string hash = this._hasher.HashPassword(password);

        // Assert
        hash.Should().NotBeNull();
        hash.Length.Should().Be(60);
        this._hasher.VerifyPassword(password, hash).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="PasswordHasher.HashPassword"/> throws an <see cref="ArgumentException"/>
    /// when the provided password is null, empty, or whitespace.
    /// </summary>
    /// <param name="invalidPassword">The invalid password string to test.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void HashPassword_ShouldThrowException_WhenPasswordIsInvalid(string? invalidPassword)
    {
        // Act
        Action act = () => this._hasher.HashPassword(invalidPassword!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that <see cref="PasswordHasher.VerifyPassword"/> returns <c>false</c>
    /// when a wrong password is provided for a given hash.
    /// </summary>
    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenPasswordIsIncorrect()
    {
        // Arrange
        const string password = "CommectPassword";
        string hash = this._hasher.HashPassword(password);

        // Act
        bool result = this._hasher.VerifyPassword("WrongPasswrod", hash);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="PasswordHasher.VerifyPassword"/> throws an <see cref="ArgumentException"/>
    /// when the hash provided for verification is invalid (whitespace).
    /// </summary>
    /// <param name="invalidHash">The invalid hash string to test.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void VerifyPassword_ShouldThrowException_WhenHashIsInvalid(string? invalidHash)
    {
        // Act
        Action act = () => this._hasher.VerifyPassword("password", invalidHash!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that <see cref="PasswordHasher.VerifyPassword"/> throws an <see cref="ArgumentException"/>
    /// when the password provided for verification is invalid (whitespace).
    /// </summary>
    /// <param name="invalidPassword">The invalid password string to test.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void VerifyPassword_ShouldThrowException_WhenPasswordIsInvalid(string? invalidPassword)
    {
        // Act
        Action act = () => this._hasher.VerifyPassword(invalidPassword!, "hash");

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
