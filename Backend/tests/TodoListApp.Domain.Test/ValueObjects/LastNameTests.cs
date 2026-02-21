using FluentAssertions;
using TodoListApp.Domain.Exceptions;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Domain.Test.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="LastName"/> value object.
/// </summary>
public class LastNameTests
{
    private const string Name = "Smith";

    /// <summary>
    /// Tests that a valid last name is correctly created and trimmed.
    /// </summary>
    /// <param name="input">The raw name string input.</param>
    /// <param name="expected">The expected trimmed name string.</param>
    [Theory]
    [InlineData("Smith", Name)]
    [InlineData("  Smith  ", Name)]
    [InlineData("O'Smith", "O'Smith")]
    [InlineData(" ", null)]
    [InlineData(null, null)]
    public void Create_Should_ReturnLastName_When_ValueIsValid(string? input, string? expected)
    {
        // Act
        var result = LastName.Create(input);

        // Assert
        result?.Value.Should().Be(expected);
    }

    /// <summary>
    /// Tests that <see cref="LastName.Create"/> throws <see cref="DomainException"/>
    /// when the last name exceeds 30 characters.
    /// </summary>
    [Fact]
    public void Create_Should_ThrowDomainException_When_NameTooLong()
    {
        // Arrange
        var longName = new string('A', 31);

        // Act
        Action act = () => LastName.Create(longName);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Last name cannot contain more than 30 characters.");
    }

    /// <summary>
    /// Tests that <see cref="LastName.ToString"/> returns the underlying string value.
    /// </summary>
    [Fact]
    public void ToString_Should_ReturnRawValue()
    {
        // Arrange
        var lastName = LastName.Create(Name);

        // Act
        var result = lastName!.ToString();

        // Assert
        result.Should().Be(Name);
    }

    /// <summary>
    /// Tests that two <see cref="LastName"/> instances with the same value are considered equal.
    /// </summary>
    [Fact]
    public void FirstNames_WithSameValue_Should_BeEqual()
    {
        // Arrange
        var name1 = LastName.Create(Name);
        var name2 = LastName.Create(Name);

        // Assert
        name1.Should().Be(name2);
        (name1 == name2).Should().BeTrue();
    }
}
