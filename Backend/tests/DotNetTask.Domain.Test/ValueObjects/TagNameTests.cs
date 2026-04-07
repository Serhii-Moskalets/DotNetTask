using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Exceptions;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

namespace DotNetTask.Domain.Test.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="TagName"/> value object.
/// </summary>
public class TagNameTests
{
    /// <summary>
    /// Verifies that Create returns a valid instance when input is correct.
    /// </summary>
    [Fact]
    public void Create_ShouldReturnInstance_WhenValid()
    {
        // Arrange
        const string input = "ValidTag";

        // Act
        TagName result = TagName.Create(input);

        // Assert
        result.Value.Should().Be(input);
    }

    /// <summary>
    /// Verifies that Create trims leading and trailing whitespace.
    /// </summary>
    /// <param name="input">
    /// The raw input string that may contain surrounding whitespace.
    /// </param>
    /// <param name="expected">
    /// The expected trimmed value after validation.
    /// </param>
    [Theory]
    [InlineData("   TagName   ", "TagName")]
    [InlineData("\nTagName\t", "TagName")]
    public void Create_ShouldTrimValue(string input, string expected)
    {
        // Arrange
        TagName result = TagName.Create(input);

        // Act & Assert
        result.Value.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that Create throws DomainException when value is null, empty or whitespace.
    /// </summary>
    /// <param name="invalidValue">
    /// The invalid input value that should cause validation failure.
    /// </param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n\t")]
    public void Create_ShouldThrow_WhenValueIsNullOrWhiteSpace(string? invalidValue)
    {
        // Arrange
        Action act = () => TagName.Create(invalidValue!);

        // Act & Assert
        act.Should()
            .Throw<DomainException>()
            .WithMessage(TagPolicy.EmptyMessage);
    }

    /// <summary>
    /// Verifies that Create throws DomainException when value exceeds MaxLength.
    /// </summary>
    [Fact]
    public void Create_ShouldThrow_WhenValueExceedsMaxLength()
    {
        // Arrange
        string tooLong = new('a', TagName.MaxLength + 1);

        // Act
        Action act = () => TagName.Create(tooLong);

        // Assert
        act.Should()
            .Throw<DomainException>()
            .WithMessage(TagPolicy.TooLongMessage);
    }

    /// <summary>
    /// Verifies that ToString returns underlying value.
    /// </summary>
    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        const string text = "SampleTag";

        // Act
        TagName result = TagName.Create(text);

        // Assert
        result.ToString().Should().Be(text);
    }

    /// <summary>
    /// Verifies that two instances with same value are equal.
    /// </summary>
    [Fact]
    public void ShouldBeEqual_WhenValuesAreSame()
    {
        // Arrange
        TagName first = TagName.Create("SameTag");
        TagName second = TagName.Create("SameTag");

        // Act & Assert
        first.Should().Be(second);
    }

    /// <summary>
    /// Verifies that two instances with different values are not equal.
    /// </summary>
    [Fact]
    public void ShouldNotBeEqual_WhenValuesAreDifferent()
    {
        // Arrange
        TagName first = TagName.Create("FirstTag");
        TagName second = TagName.Create("SecondTag");

        // Act & Assert
        first.Should().NotBe(second);
    }

    /// <summary>
    /// Verifies that a tag name instance can be created successfully when the input string is at the maximum allowed.
    /// </summary>
    [Fact]
    public void Create_Should_Work_When_LengthIsExactlyMaxLength()
    {
        // Arrange
        string value = new('a', TagName.MaxLength);

        // Act
        TagName result = TagName.Create(value);

        // Assert
        result.Value.Length.Should().Be(TagName.MaxLength);
    }
}
