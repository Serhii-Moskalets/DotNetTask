using DotNetTask.Infrastructure.Security;

using FluentAssertions;

namespace DotNetTask.Infrastructure.Test.Security;

/// <summary>
/// Contains unit tests for the <see cref="TokenGenerator"/> class.
/// </summary>
public class TokenGeneratorTests
{
    private readonly TokenGenerator _sut = new();

    /// <summary>
    /// Verifies that <see cref="TokenGenerator.GenerateSecureToken"/> returns a non-empty string.
    /// </summary>
    [Fact]
    public void GenerateSecureToken_ShouldReturnNonEmptyString()
    {
        // Act
        string result = this._sut.GenerateSecureToken();

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Verifies that <see cref="TokenGenerator.GenerateSecureToken"/> produces unique tokens
    /// across multiple calls.
    /// </summary>
    [Fact]
    public void GenerateSecureToken_ShouldProduceUniqueTokens()
    {
        // Act
        string token1 = this._sut.GenerateSecureToken();
        string token2 = this._sut.GenerateSecureToken();

        // Assert
        token1.Should().NotBe(token2);
    }

    /// <summary>
    /// Verifies that the generated token has the expected length for 32-byte hex string.
    /// </summary>
    [Fact]
    public void GenerateSecureToken_ShouldHaveExpectedLength()
    {
        // Arrange
        const int expectedLength = 64;

        // Act
        string result = this._sut.GenerateSecureToken();

        // Assert
        result.Length.Should().Be(expectedLength);
    }
}
