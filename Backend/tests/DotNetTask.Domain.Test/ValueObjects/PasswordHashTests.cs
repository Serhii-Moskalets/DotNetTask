using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Exceptions;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

namespace DotNetTask.Domain.Test.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="PasswordHash"/> value object.
/// </summary>
public class PasswordHashTests
{
    /// <summary>
    /// Tests that a valid password hash is correctly created and trimmed.
    /// </summary>
    /// <param name="input">The raw password hash string input.</param>
    [Theory]
    [InlineData("a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6")]
    [InlineData("  a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6  ")]
    public void Create_Should_ReturnPasswordHash_When_ValueIsValid(string input)
    {
        // Act
        PasswordHash result = PasswordHash.Create(input);

        // Assert
        result.Value.Should().Be(input.Trim());
    }

    /// <summary>
    /// Tests that <see cref="PasswordHash.Create"/> throws <see cref="DomainException"/>
    /// when the input is null, empty, or consists only of whitespace.
    /// </summary>
    /// <param name="invalidInput">The invalid string to be tested.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ThrowDomainException_When_ValueIsEmpty(string? invalidInput)
    {
        // Act
        Action act = () => PasswordHash.Create(invalidInput!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage(SecurityPolicy.HashEmptyMessage);
    }

    /// <summary>
    /// Tests that <see cref="PasswordHash.ToString"/> returns the underlying hash value.
    /// </summary>
    [Fact]
    public void ToString_Should_ReturnRawValue()
    {
        // Arrange
        string rawHash = new('a', 64);
        PasswordHash passwordHash = PasswordHash.Create(rawHash);

        // Act
        string result = passwordHash.ToString();

        // Assert
        result.Should().Be(rawHash);
    }

    /// <summary>
    /// Tests that two <see cref="PasswordHash"/> instances with the same value are considered equal.
    /// </summary>
    [Fact]
    public void PasswordHashes_WithSameValue_Should_BeEqual()
    {
        // Arrange
        string rawHash = new('b', 64);
        PasswordHash hash1 = PasswordHash.Create(rawHash);
        PasswordHash hash2 = PasswordHash.Create(rawHash);

        // Assert
        hash1.Should().Be(hash2);
        (hash1 == hash2).Should().BeTrue();
    }
}
