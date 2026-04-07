using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Exceptions;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

namespace DotNetTask.Domain.Test.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="FirstName"/> value object.
/// </summary>
public class FirstNameTests
{
    /// <summary>
    /// Tests that a valid first name is correctly created and trimmed.
    /// </summary>
    /// <param name="input">The raw name string input.</param>
    /// <param name="expected">The expected trimmed name string.</param>
    [Theory]
    [InlineData("John", "John")]
    [InlineData("  Alice  ", "Alice")]
    [InlineData("O'Connor", "O'Connor")]
    public void Create_Should_ReturnFirstName_When_ValueIsValid(string input, string expected)
    {
        // Act
        FirstName result = FirstName.Create(input);

        // Assert
        result.Value.Should().Be(expected);
    }

    /// <summary>
    /// Tests that <see cref="FirstName.Create"/> throws <see cref="DomainException"/>
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
        Action act = () => FirstName.Create(invalidInput!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage(FirstNamePolicy.EmptyMessage);
    }

    /// <summary>
    /// Tests that <see cref="FirstName.Create"/> throws <see cref="DomainException"/>
    /// when the first name exceeds <see cref="FirstName.MaxLength"/> characters.
    /// </summary>
    [Fact]
    public void Create_Should_ThrowDomainException_When_NameTooLong()
    {
        // Arrange
        string longName = new('A', FirstName.MaxLength + 1);

        // Act
        Action act = () => FirstName.Create(longName);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage(FirstNamePolicy.TooLongMessage);
    }

    /// <summary>
    /// Verifies that a first name instance can be created successfully when the input string is at the maximum allowed.
    /// </summary>
    [Fact]
    public void Create_Should_Work_When_LengthIsExactlyMaxLength()
    {
        // Arrange
        string value = new('a', FirstName.MaxLength);

        // Act
        FirstName result = FirstName.Create(value);

        // Assert
        result.Value.Length.Should().Be(FirstName.MaxLength);
    }

    /// <summary>
    /// Tests that <see cref="FirstName.ToString"/> returns the underlying string value.
    /// </summary>
    [Fact]
    public void ToString_Should_ReturnRawValue()
    {
        // Arrange
        const string rawName = "Maxim";
        FirstName firstName = FirstName.Create(rawName);

        // Act
        string result = firstName.ToString();

        // Assert
        result.Should().Be(rawName);
    }

    /// <summary>
    /// Tests that two <see cref="FirstName"/> instances with the same value are considered equal.
    /// </summary>
    [Fact]
    public void FirstNames_WithSameValue_Should_BeEqual()
    {
        // Arrange
        FirstName name1 = FirstName.Create("John");
        FirstName name2 = FirstName.Create("John");

        // Assert
        name1.Should().Be(name2);
        (name1 == name2).Should().BeTrue();
    }
}
