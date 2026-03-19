using FluentValidation.TestHelper;
using TodoListApp.Application.Tag.Commands.DeleteTag;
using TodoListApp.Domain.Constants;

namespace TodoListApp.Application.Tests.Tag.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteTagCommandValidator"/>.
/// Ensures that the validator correctly enforces rules for deleting a tag.
/// </summary>
public class DeleteTagCommandValidatorTests
{
    private readonly DeleteTagCommandValidator _validator = new();

    /// <summary>
    /// Fails validation when UserId is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        var command = new DeleteTagCommand(TagId: Guid.NewGuid(), UserId: Guid.Empty);

        // Act
        var result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.UserId)
              .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Fails validation when TagId is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_TagId_Is_Empty()
    {
        // Arrange
        var command = new DeleteTagCommand(TagId: Guid.Empty, UserId: Guid.NewGuid());

        // Act
        var result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.TagId)
              .WithErrorMessage(TagPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Passes validation when both UserId and TagId are valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_UserId_And_TagId_Are_Valid()
    {
        // Arrange
        var command = new DeleteTagCommand(TagId: Guid.NewGuid(), UserId: Guid.NewGuid());

        // Act
        var result = this._validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.UserId);
        result.ShouldNotHaveValidationErrorFor(c => c.TagId);
    }
}
