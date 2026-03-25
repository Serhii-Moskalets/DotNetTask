using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Exceptions;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

namespace DotNetTask.Domain.Test.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="TaskListTitle"/> value object.
/// </summary>
public class TaskListTitleTests
{
    /// <summary>
    /// Verifies that Create returns a valid instance when input is correct.
    /// </summary>
    [Fact]
    public void Create_ShouldReturnInstance_WhenValid()
    {
        // Arrange
        const string input = "Shopping List";

        // Act
        TaskListTitle result = TaskListTitle.Create(input);

        // Assert
        result.Value.Should().Be(input);
    }

    /// <summary>
    /// Verifies that Create trims leading and trailing whitespace.
    /// </summary>
    /// <param name="input">The raw input string that may contain surrounding whitespace.</param>
    /// <param name="expected">The expected trimmed value after validation.</param>
    [Theory]
    [InlineData("   Work Tasks   ", "Work Tasks")]
    [InlineData("\nDaily Routine\t", "Daily Routine")]
    public void Create_ShouldTrimValue(string input, string expected)
    {
        // Act
        TaskListTitle result = TaskListTitle.Create(input);

        // Assert
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
        // Act
        Action act = () => TaskListTitle.Create(invalidValue!);

        // Assert
        act.Should()
            .Throw<DomainException>()
            .WithMessage(TaskListPolicy.EmptyMessage);
    }

    /// <summary>
    /// Verifies that Create throws DomainException when value exceeds MaxLength.
    /// </summary>
    [Fact]
    public void Create_ShouldThrow_WhenValueExceedsMaxLength()
    {
        // Arrange
        string tooLong = new string('z', TaskListTitle.MaxLength + 1);

        // Act
        Action act = () => TaskListTitle.Create(tooLong);

        // Assert
        act.Should()
            .Throw<DomainException>()
            .WithMessage(TaskListPolicy.TooLongMessage);
    }

    /// <summary>
    /// Verifies that ToString returns underlying value.
    /// </summary>
    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        const string text = "My List";

        // Act
        TaskListTitle result = TaskListTitle.Create(text);

        // Assert
        result.ToString().Should().Be(text);
    }

    /// <summary>
    /// Verifies that two instances with same value are equal (Value Object behavior).
    /// </summary>
    [Fact]
    public void ShouldBeEqual_WhenValuesAreSame()
    {
        // Arrange & Act
        const string sharedTitle = "Consistent Title";
        TaskListTitle first = TaskListTitle.Create(sharedTitle);
        TaskListTitle second = TaskListTitle.Create(sharedTitle);

        // Assert
        first.Should().Be(second);
    }

    /// <summary>
    /// Verifies that two instances with different values are not equal.
    /// </summary>
    [Fact]
    public void ShouldNotBeEqual_WhenValuesAreDifferent()
    {
        // Arrange & Act
        TaskListTitle first = TaskListTitle.Create("First Title");
        TaskListTitle second = TaskListTitle.Create("Second Title");

        // Assert
        first.Should().NotBe(second);
    }

    /// <summary>
    /// Verifies that a task list title instance can be created successfully when the input string is at the maximum allowed.
    /// </summary>
    [Fact]
    public void Create_Should_Work_When_LengthIsExactlyMaxLength()
    {
        // Arrange
        string value = new string('a', TaskListTitle.MaxLength);

        // Act
        TaskListTitle result = TaskListTitle.Create(value);

        // Assert
        result.Value.Length.Should().Be(TaskListTitle.MaxLength);
    }
}
