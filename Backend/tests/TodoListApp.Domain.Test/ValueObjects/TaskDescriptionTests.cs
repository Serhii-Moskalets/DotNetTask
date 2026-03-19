using FluentAssertions;
using TodoListApp.Domain.Constants;
using TodoListApp.Domain.Exceptions;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Domain.Test.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="TaskDescription"/> value object.
/// </summary>
public class TaskDescriptionTests
{
    /// <summary>
    /// Verifies that Create returns a valid instance when input is correct.
    /// </summary>
    [Fact]
    public void Create_ShouldReturnInstance_WhenValid()
    {
        // Arrange
        const string input = "This is a valid task description.";

        // Act
        var result = TaskDescription.Create(input);

        // Assert
        result.Value.Should().Be(input);
    }

    /// <summary>
    /// Verifies that Create trims leading and trailing whitespace.
    /// </summary>
    /// <param name="input">The raw input string that may contain surrounding whitespace.</param>
    /// <param name="expected">The expected trimmed value after validation.</param>
    [Theory]
    [InlineData("   Detailed description   ", "Detailed description")]
    [InlineData("\nMulti-line\t", "Multi-line")]
    public void Create_ShouldTrimValue(string input, string expected)
    {
        var result = TaskDescription.Create(input);

        result.Value.Should().Be(expected);
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
        Action act = () => TaskDescription.Create(invalidValue!);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(TaskPolicy.EmptyDescriptionMessage);
    }

    /// <summary>
    /// Verifies that Create throws DomainException when value exceeds MaxLength.
    /// </summary>
    [Fact]
    public void Create_ShouldThrow_WhenValueExceedsMaxLength()
    {
        // Arrange
        var tooLong = new string('a', TaskDescription.MaxLength + 1);

        // Act
        Action act = () => TaskDescription.Create(tooLong);

        // Assert
        act.Should()
            .Throw<DomainException>()
            .WithMessage(TaskPolicy.TooLongDescriptionMessage);
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
        var result = TaskDescription.CreateOptional(input);

        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that CreateOptional returns a valid instance when input is valid.
    /// </summary>
    [Fact]
    public void CreateOptional_ShouldReturnInstance_WhenValid()
    {
        const string input = "Optional description content.";

        var result = TaskDescription.CreateOptional(input);

        result.Should().NotBeNull();
        result!.Value.Should().Be(input);
    }

    /// <summary>
    /// Verifies that ToString returns the underlying value.
    /// </summary>
    [Fact]
    public void ToString_ShouldReturnValue()
    {
        const string text = "ToString content";

        var result = TaskDescription.Create(text);

        result.ToString().Should().Be(text);
    }

    /// <summary>
    /// Verifies that two instances with same value are equal.
    /// </summary>
    [Fact]
    public void ShouldBeEqual_WhenValuesAreSame()
    {
        const string content = "Same content";
        var first = TaskDescription.Create(content);
        var second = TaskDescription.Create(content);

        first.Should().Be(second);
    }

    /// <summary>
    /// Verifies that two instances with different values are not equal.
    /// </summary>
    [Fact]
    public void ShouldNotBeEqual_WhenValuesAreDifferent()
    {
        var first = TaskDescription.Create("Description A");
        var second = TaskDescription.Create("Description B");

        first.Should().NotBe(second);
    }

    /// <summary>
    /// Verifies that a task description instance can be created successfully when the input string is at the maximum allowed.
    /// </summary>
    [Fact]
    public void Create_Should_Work_When_LengthIsExactlyMaxLength()
    {
        // Arrange
        var value = new string('a', TaskDescription.MaxLength);

        // Act
        var result = TaskDescription.Create(value);

        // Assert
        result.Value.Length.Should().Be(TaskDescription.MaxLength);
    }
}
