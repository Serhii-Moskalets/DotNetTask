using FluentAssertions;
using TodoListApp.Domain.Exceptions;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Domain.Test.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="SecurityStamp"/> value object.
/// </summary>
public class SecurityStampTests
{
    /// <summary>
    /// Tests that <see cref="SecurityStamp.New"/> generates a unique and non-empty stamp.
    /// </summary>
    [Fact]
    public void New_Should_GenerateUniqueAndValidStamp()
    {
        // Act
        var stamp1 = SecurityStamp.New();
        var stamp2 = SecurityStamp.New();

        // Assert
        stamp1.Value.Should().NotBeNullOrWhiteSpace();
        stamp2.Value.Should().NotBeNullOrWhiteSpace();
        stamp1.Value.Should().NotBe(stamp2.Value);

        Guid.TryParse(stamp1.Value, out _).Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="SecurityStamp.Create"/> initializes the property correctly
    /// when a valid string is provided.
    /// </summary>
    [Fact]
    public void Create_Should_InitializeValue_When_InputIsValid()
    {
        // Arrange
        const string rawValue = "custom-security-stamp-123";

        // Act
        var stamp = SecurityStamp.Create(rawValue);

        // Assert
        stamp.Value.Should().Be(rawValue);
    }

    /// <summary>
    /// Tests that <see cref="SecurityStamp"/> constructor (via Create) throws <see cref="DomainException"/>
    /// when the value is null or empty.
    /// </summary>
    /// <param name="invalidValue">The invalid string to test.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Create_Should_ThrowException_When_ValueIsNullOrEmpty(string? invalidValue)
    {
        // Act
        Action act = () => SecurityStamp.Create(invalidValue!);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("Invalid security identifier.");
    }

    /// <summary>
    /// Tests the equality of <see cref="SecurityStamp"/> records.
    /// </summary>
    [Fact]
    public void Equality_Should_BeBasedOnValue()
    {
        // Arrange
        const string rawValue = "same-value";
        var stamp1 = SecurityStamp.Create(rawValue);
        var stamp2 = SecurityStamp.Create(rawValue);
        var stamp3 = SecurityStamp.Create("different-value");

        // Assert
        stamp1.Should().Be(stamp2);
        stamp1.Should().NotBe(stamp3);
    }
}
