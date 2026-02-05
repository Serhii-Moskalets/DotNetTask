using FluentAssertions;
using TodoListApp.Domain.Enums;
using TodoListApp.Domain.Exceptions;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Domain.Test.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="SecurityToken"/> value object.
/// </summary>
public class SecurityTokenTests
{
    private const string ValidTokenValue = "correct-token-123";
    private readonly TimeSpan _validDuration = TimeSpan.FromHours(1);

    /// <summary>
    /// Tests that <see cref="SecurityToken.Create"/> initializes properties correctly.
    /// </summary>
    [Fact]
    public void Create_Should_InitializeProperties_When_DataIsValid()
    {
        // Arrange
        const UserTokenType type = UserTokenType.EmailVerification;
        const string metadata = "additional-info";

        // Act
        var token = SecurityToken.Create(ValidTokenValue, this._validDuration, type, metadata);

        // Assert
        token.Value.Should().Be(ValidTokenValue);
        token.Type.Should().Be(type);
        token.Metadata.Should().Be(metadata);
        token.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    /// <summary>
    /// Tests that <see cref="SecurityToken.Create"/> throws <see cref="DomainException"/>
    /// when the token value is null or empty.
    /// </summary>
    /// <param name="invalidValue">The invalid token string to test.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Create_Should_ThrowException_When_ValueIsNullOrEmpty(string? invalidValue)
    {
        // Act
        Action act = () => SecurityToken.Create(invalidValue!, this._validDuration, UserTokenType.PasswordReset);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Token value cannot be null or empty.");
    }

    /// <summary>
    /// Tests that <see cref="SecurityToken.Create"/> throws <see cref="DomainException"/>
    /// when the duration is zero or negative.
    /// </summary>
    /// <param name="seconds">The non-positive number of seconds for duration.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Create_Should_ThrowException_When_DurationIsNotPositive(int seconds)
    {
        // Arrange
        var invalidDuration = TimeSpan.FromSeconds(seconds);

        // Act
        Action act = () => SecurityToken.Create(ValidTokenValue, invalidDuration, UserTokenType.EmailChange);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Token duration must be positive.");
    }

    /// <summary>
    /// Tests that <see cref="SecurityToken.IsValid"/> returns true when all conditions are met.
    /// </summary>
    [Fact]
    public void IsValid_Should_ReturnTrue_When_TokenMatchesAndNotExpired()
    {
        // Arrange
        var token = SecurityToken.Create(ValidTokenValue, this._validDuration, UserTokenType.PasswordReset);
        var currentTime = DateTime.UtcNow;

        // Act
        bool result = token.IsValid(ValidTokenValue, UserTokenType.PasswordReset, currentTime);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="SecurityToken.IsValid"/> returns false for various invalid conditions.
    /// </summary>
    /// <param name="providedValue">The token string provided for verification.</param>
    /// <param name="expectedType">The type expected for verification.</param>
    /// <param name="minutesToAdd">Minutes to add to UtcNow to simulate current time (for expiration check).</param>
    [Theory]
    [InlineData("wrong-value", UserTokenType.PasswordReset, 0)]
    [InlineData(ValidTokenValue, UserTokenType.EmailVerification, 0)]
    [InlineData(ValidTokenValue, UserTokenType.PasswordReset, 120)]
    public void IsValid_Should_ReturnFalse_When_ConditionsNotMet(string providedValue, UserTokenType expectedType, int minutesToAdd)
    {
        // Arrange
        var token = SecurityToken.Create(ValidTokenValue, this._validDuration, UserTokenType.PasswordReset);
        var testTime = DateTime.UtcNow.AddMinutes(minutesToAdd);

        // Act
        bool result = token.IsValid(providedValue, expectedType, testTime);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="SecurityToken.IsValid"/> returns false when the provided value is null or empty.
    /// </summary>
    /// <param name="emptyValue">The null or empty string to test.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsValid_Should_ReturnFalse_When_ProvidedValueIsNullOrEmpty(string? emptyValue)
    {
        // Arrange
        var token = SecurityToken.Create(ValidTokenValue, this._validDuration, UserTokenType.PasswordReset);

        // Act
        bool result = token.IsValid(emptyValue!, UserTokenType.PasswordReset, DateTime.UtcNow);

        // Assert
        result.Should().BeFalse();
    }
}
