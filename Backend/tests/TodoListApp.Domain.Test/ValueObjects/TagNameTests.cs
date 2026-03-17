using FluentAssertions;
using TodoListApp.Domain.Exceptions;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Domain.Test.ValueObjects;

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
        var result = TagName.Create(input);

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
        var result = TagName.Create(input);

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
        Action act = () => TagName.Create(invalidValue!);

        act.Should()
            .Throw<DomainException>()
            .WithMessage("Tag name cannot be empty.");
    }

    /// <summary>
    /// Verifies that Create throws DomainException when value exceeds MaxLength.
    /// </summary>
    [Fact]
    public void Create_ShouldThrow_WhenValueExceedsMaxLength()
    {
        // Arrange
        var tooLong = new string('a', TagName.MaxLength + 1);

        // Act
        Action act = () => TagName.Create(tooLong);

        // Assert
        act.Should()
            .Throw<DomainException>()
            .WithMessage($"Tag name cannot be longer than {TagName.MaxLength} characters.");
    }

    /// <summary>
    /// Verifies that ToString returns underlying value.
    /// </summary>
    [Fact]
    public void ToString_ShouldReturnValue()
    {
        const string text = "SampleTag";

        var result = TagName.Create(text);

        result.ToString().Should().Be(text);
    }

    /// <summary>
    /// Verifies that two instances with same value are equal.
    /// </summary>
    [Fact]
    public void ShouldBeEqual_WhenValuesAreSame()
    {
        var first = TagName.Create("SameTag");
        var second = TagName.Create("SameTag");

        first.Should().Be(second);
    }

    /// <summary>
    /// Verifies that two instances with different values are not equal.
    /// </summary>
    [Fact]
    public void ShouldNotBeEqual_WhenValuesAreDifferent()
    {
        var first = TagName.Create("FirstTag");
        var second = TagName.Create("SecondTag");

        first.Should().NotBe(second);
    }

    /// <summary>
    /// Verifies that a tag name instance can be created successfully when the input string is at the maximum allowed.
    /// </summary>
    [Fact]
    public void Create_Should_Work_When_LengthIsExactlyMaxLength()
    {
        // Arrange
        var value = new string('a', TagName.MaxLength);

        // Act
        var result = TagName.Create(value);

        // Assert
        result.Value.Length.Should().Be(TagName.MaxLength);
    }
}
