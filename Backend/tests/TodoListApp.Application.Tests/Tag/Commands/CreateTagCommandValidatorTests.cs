using FluentValidation.TestHelper;
using TodoListApp.Application.Tag.Commands.CreateTag;
using TodoListApp.Domain.Constants;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tests.Tag.Commands;

/// <summary>
/// Unit tests for <see cref="CreateTagCommandValidator"/>.
/// Tests validation rules for creating a tag.
/// </summary>
public class CreateTagCommandValidatorTests
{
    private readonly CreateTagCommandValidator _validator = new();

    /// <summary>
    /// Tests that validation fails when the tag name is empty.
    /// </summary>
    [Fact]
    public void Validate_ShouldHaveError_WhenNameIsEmpty()
    {
        // Arrange
        var command = new CreateTagCommand(Guid.NewGuid(), Guid.NewGuid(), string.Empty);

        // Act
        var result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage(TagPolicy.EmptyMessage);
    }

    /// <summary>
    /// Tests that validation fails when the tag name exceeds the maximum length (50 characters).
    /// </summary>
    [Fact]
    public void Validate_ShouldHaveError_WhenNameIsTooLong()
    {
        // Arrange
        var longName = new string('a', TagName.MaxLength + 1);
        var command = new CreateTagCommand(Guid.NewGuid(), Guid.NewGuid(), longName);

        // Act
        var result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage(TagPolicy.TooLongMessage);
    }

    /// <summary>
    /// Tests that validation passes when the tag name is not empty and within length limit.
    /// </summary>
    [Fact]
    public void Validate_ShouldNotHaveError_WhenNameIsValid()
    {
        // Arrange
        var command = new CreateTagCommand(Guid.NewGuid(), Guid.NewGuid(), "Tag");

        // Act
        var result = this._validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.Name);
    }
}
