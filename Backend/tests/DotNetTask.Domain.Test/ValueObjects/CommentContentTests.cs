using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Exceptions;
using DotNetTask.Domain.ValueObjects;

using FluentAssertions;

namespace DotNetTask.Domain.Test.ValueObjects;

/// <summary>
/// Unit tests for the <see cref="CommentContent"/> value object.
/// </summary>
public class CommentContentTests
{
    /// <summary>
    /// Verifies that Create returns a valid instance when input is correct.
    /// </summary>
    [Fact]
    public void Create_Should_ReturnInstance_WhenValid()
    {
        // Arrange
        const string input = "Valid comment";

        // Act
        CommentContent result = CommentContent.Create(input);

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
    public void Create_Should_TrimValue(string input, string expected)
    {
        // Act
        CommentContent result = CommentContent.Create(input);

        // Assert
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
    public void Create_Should_Throw_WhenValueIsNullOrWhiteSpace(string? invalidValue)
    {
        // Act
        Action act = () => CommentContent.Create(invalidValue!);

        // Assert
        act.Should()
            .Throw<DomainException>()
            .WithMessage(CommentPolicy.EmptyMessage);
    }

    /// <summary>
    /// Verifies that Create throws DomainException when value exceeds MaxLength.
    /// </summary>
    [Fact]
    public void Create_ShouldThrow_WhenValueExceedsMaxLength()
    {
        // Arrange
        string tooLong = new string('a', CommentContent.MaxLength + 1);

        // Act
        Action act = () => CommentContent.Create(tooLong);

        // Assert
        act.Should()
            .Throw<DomainException>()
            .WithMessage(CommentPolicy.TooLongMessage);
    }

    /// <summary>
    /// Verifies that a comment content instance can be created successfully when the input string is at the maximum allowed.
    /// </summary>
    [Fact]
    public void Create_Should_Work_When_LengthIsExactlyMaxLength()
    {
        // Arrange
        string value = new string('a', CommentContent.MaxLength);

        // Act
        CommentContent result = CommentContent.Create(value);

        // Assert
        result.Value.Length.Should().Be(CommentContent.MaxLength);
    }

    /// <summary>
    /// Verifies that ToString returns underlying value.
    /// </summary>
    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        const string text = "Sample";

        // Act
        CommentContent result = CommentContent.Create(text);

        // Assert
        result.ToString().Should().Be(text);
    }

    /// <summary>
    /// Verifies that two instances with same value are equal.
    /// </summary>
    [Fact]
    public void CommentContent_Should_BeEqual_WhenValuesAreSame()
    {
        // Act
        CommentContent first = CommentContent.Create("Same");
        CommentContent second = CommentContent.Create("Same");

        // Assert
        first.Should().Be(second);
    }

    /// <summary>
    /// Verifies that two instances with different values are not equal.
    /// </summary>
    [Fact]
    public void CommentContent_Should_NotBeEqual_WhenValuesAreDifferent()
    {
        // Act
        CommentContent first = CommentContent.Create("First");
        CommentContent second = CommentContent.Create("Second");

        // Assert
        first.Should().NotBe(second);
    }
}