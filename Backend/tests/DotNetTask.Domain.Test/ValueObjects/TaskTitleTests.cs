using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Exceptions;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

namespace DotNetTask.Domain.Test.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="TaskTitle"/> value object.
/// </summary>
public class TaskTitleTests
{
    /// <summary>
    /// Verifies that Create returns a valid instance when input is correct.
    /// </summary>
    [Fact]
    public void Create_ShouldReturnInstance_WhenValid()
    {
        // Arrange
        const string input = "Finish the project";

        // Act
        TaskTitle result = TaskTitle.Create(input);

        // Assert
        result.Value.Should().Be(input);
    }

    /// <summary>
    /// Verifies that Create trims leading and trailing whitespace.
    /// </summary>
    /// <param name="input">The raw input string that may contain surrounding whitespace.</param>
    /// <param name="expected">The expected trimmed value after validation.</param>
    [Theory]
    [InlineData("   Study .NET   ", "Study .NET")]
    [InlineData("\nClean house\t", "Clean house")]
    public void Create_ShouldTrimValue(string input, string expected)
    {
        TaskTitle result = TaskTitle.Create(input);

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
        Action act = () => TaskTitle.Create(invalidValue!);

        act.Should()
            .Throw<DomainException>()
            .WithMessage(TaskPolicy.EmptyTitleMessage);
    }

    /// <summary>
    /// Verifies that Create throws DomainException when value exceeds MaxLength.
    /// </summary>
    [Fact]
    public void Create_ShouldThrow_WhenValueExceedsMaxLength()
    {
        // Arrange
        string tooLong = new string('a', TaskTitle.MaxLength + 1);

        // Act
        Action act = () => TaskTitle.Create(tooLong);

        // Assert
        act.Should()
            .Throw<DomainException>()
            .WithMessage(TaskPolicy.TooLongTitleMessage);
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
        TaskTitle? result = TaskTitle.CreateOptional(input);

        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that CreateOptional returns valid instance when input is valid.
    /// </summary>
    [Fact]
    public void CreateOptional_ShouldReturnInstance_WhenValid()
    {
        const string input = "Optional Task";

        TaskTitle? result = TaskTitle.CreateOptional(input);

        result.Should().NotBeNull();
        result!.Value.Should().Be(input);
    }

    /// <summary>
    /// Verifies that ToString returns underlying value.
    /// </summary>
    [Fact]
    public void ToString_ShouldReturnValue()
    {
        const string text = "Task String";

        TaskTitle result = TaskTitle.Create(text);

        result.ToString().Should().Be(text);
    }

    /// <summary>
    /// Verifies that two instances with same value are equal (Value Object behavior).
    /// </summary>
    [Fact]
    public void ShouldBeEqual_WhenValuesAreSame()
    {
        TaskTitle first = TaskTitle.Create("Same Title");
        TaskTitle second = TaskTitle.Create("Same Title");

        first.Should().Be(second);
    }

    /// <summary>
    /// Verifies that two instances with different values are not equal.
    /// </summary>
    [Fact]
    public void ShouldNotBeEqual_WhenValuesAreDifferent()
    {
        TaskTitle first = TaskTitle.Create("First Title");
        TaskTitle second = TaskTitle.Create("Second Title");

        first.Should().NotBe(second);
    }

    /// <summary>
    /// Verifies that a task title instance can be created successfully when the input string is at the maximum allowed.
    /// </summary>
    [Fact]
    public void Create_Should_Work_When_LengthIsExactlyMaxLength()
    {
        // Arrange
        string value = new string('a', TaskTitle.MaxLength);

        // Act
        TaskTitle result = TaskTitle.Create(value);

        // Assert
        result.Value.Length.Should().Be(TaskTitle.MaxLength);
    }
}
