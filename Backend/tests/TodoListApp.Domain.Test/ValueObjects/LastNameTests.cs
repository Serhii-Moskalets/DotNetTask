using FluentAssertions;
using TodoListApp.Domain.Constants;
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
    public void Create_Should_ReturnLastName_When_ValueIsValid(string input, string expected)
    {
        // Act
        var result = LastName.Create(input);

        // Assert
        result.Value.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that a last name instance can be created successfully when the input string is at the maximum allowed.
    /// </summary>
    [Fact]
    public void Create_Should_Work_When_LengthIsExactlyMaxLength()
    {
        // Arrange
        var value = new string('a', LastName.MaxLength);

        // Act
        var result = LastName.Create(value);

        // Assert
        result.Value.Length.Should().Be(LastName.MaxLength);
    }

    /// <summary>
    /// Verifies that Create throws DomainException when value is null, empty or whitespace.
    /// </summary>
    /// <param name="invalidValue">The invalid input value that should cause validation failure.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n\t")]
    public void Create_ShouldThrow_WhenValueIsNullOrWhiteSpace(string? invalidValue)
    {
        // Act
        Action act = () => LastName.Create(invalidValue!);

        // Assert
        act.Should()
            .Throw<DomainException>()
            .WithMessage(LastNamePolicy.EmptyMessage);
    }

    /// <summary>
    /// Verifies that CreateOptional returns null when input is empty or whitespace.
    /// </summary>
    /// <param name="input">The raw input string which can be null, empty, or whitespace.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateOptional_ShouldReturnNull_WhenInputIsNullOrWhiteSpace(string? input)
    {
        var result = LastName.CreateOptional(input);

        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that CreateOptional returns a valid instance when input is valid.
    /// </summary>
    [Fact]
    public void CreateOptional_ShouldReturnInstance_WhenValid()
    {
        // Act
        var result = LastName.CreateOptional(Name);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Should().Be(Name);
    }

    /// <summary>
    /// Tests that <see cref="LastName.Create"/> throws <see cref="DomainException"/>
    /// when the last name exceeds <see cref="LastName.MaxLength"/> characters.
    /// </summary>
    [Fact]
    public void Create_Should_ThrowDomainException_When_NameTooLong()
    {
        // Arrange
        var longName = new string('A', LastName.MaxLength + 1);

        // Act
        Action act = () => LastName.Create(longName);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage(LastNamePolicy.TooLongMessage);
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
    public void LastNames_WithSameValue_Should_BeEqual()
    {
        // Arrange
        var name1 = LastName.Create(Name);
        var name2 = LastName.Create(Name);

        // Assert
        name1.Should().Be(name2);
        (name1 == name2).Should().BeTrue();
    }
}
