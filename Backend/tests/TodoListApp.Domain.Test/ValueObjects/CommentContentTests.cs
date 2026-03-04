using FluentAssertions;
using TodoListApp.Domain.Exceptions;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Domain.Test.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="CommentContent"/> value object.
/// </summary>
public class CommentContentTests
{
    /// <summary>
    /// Verifies that Create returns a valid instance when input is correct.
    /// </summary>
    [Fact]
    public void Create_ShouldReturnInstance_WhenValid()
    {
        // Arrange
        const string input = "Valid comment";

        // Act
        var result = CommentContent.Create(input);

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
    [InlineData("   Comment   ", "Comment")]
    [InlineData("\nComment\t", "Comment")]
    public void Create_ShouldTrimValue(string input, string expected)
    {
        var result = CommentContent.Create(input);

        result.Value.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that Create throws DomainException when value is null, empty or whitespace.
    /// </summary>
    /// /// <param name="invalidValue">
    /// The invalid input value that should cause validation failure.
    /// </param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n\t")]
    public void Create_ShouldThrow_WhenValueIsNullOrWhiteSpace(string? invalidValue)
    {
        Action act = () => CommentContent.Create(invalidValue!);

        act.Should()
            .Throw<DomainException>()
            .WithMessage("Comment content cannot be empty.");
    }

    /// <summary>
    /// Verifies that Create throws DomainException when value exceeds MaxLength.
    /// </summary>
    [Fact]
    public void Create_ShouldThrow_WhenValueExceedsMaxLength()
    {
        // Arrange
        var tooLong = new string('a', CommentContent.MaxLength + 1);

        // Act
        Action act = () => CommentContent.Create(tooLong);

        // Assert
        act.Should()
            .Throw<DomainException>()
            .WithMessage($"Comment content cannot be longer than {CommentContent.MaxLength} characters.");
    }

    /// <summary>
    /// Verifies that ToString returns underlying value.
    /// </summary>
    [Fact]
    public void ToString_ShouldReturnValue()
    {
        const string text = "Sample";

        var result = CommentContent.Create(text);

        result.ToString().Should().Be(text);
    }

    /// <summary>
    /// Verifies that two instances with same value are equal.
    /// </summary>
    [Fact]
    public void ShouldBeEqual_WhenValuesAreSame()
    {
        var first = CommentContent.Create("Same");
        var second = CommentContent.Create("Same");

        first.Should().Be(second);
    }

    /// <summary>
    /// Verifies that two instances with different values are not equal.
    /// </summary>
    [Fact]
    public void ShouldNotBeEqual_WhenValuesAreDifferent()
    {
        var first = CommentContent.Create("First");
        var second = CommentContent.Create("Second");

        first.Should().NotBe(second);
    }
}