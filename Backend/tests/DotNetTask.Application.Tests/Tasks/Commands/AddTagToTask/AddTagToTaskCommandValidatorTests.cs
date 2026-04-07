using DotNetTask.Application.Tasks.Commands.AddTagToTask;
using DotNetTask.Domain.Constants;

using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.Tasks.Commands.AddTagToTask;

/// <summary>
/// Tests for <see cref="AddTagToTaskCommandValidator"/>.
/// </summary>
public class AddTagToTaskCommandValidatorTests
{
    private readonly AddTagToTaskCommandValidator _validator = new();

    /// <summary>
    /// Ensures validation fails when the task ID is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_TaskId_Is_Empty()
    {
        // Arrange
        AddTagToTaskCommand command = new(Guid.Empty, Guid.NewGuid(), Guid.NewGuid());

        // Act
        TestValidationResult<AddTagToTaskCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TaskId)
              .WithErrorMessage(TaskPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Ensures validation fails when the user ID is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        AddTagToTaskCommand command = new(Guid.NewGuid(), Guid.Empty, Guid.NewGuid());

        // Act
        TestValidationResult<AddTagToTaskCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Ensures validation fails when the tag ID is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_TagId_Is_Empty()
    {
        // Arrange
        AddTagToTaskCommand command = new(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty);

        // Act
        TestValidationResult<AddTagToTaskCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TagId)
              .WithErrorMessage(TagPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Ensures no validation errors when the command is valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        // Arrange
        AddTagToTaskCommand command = new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Act
        TestValidationResult<AddTagToTaskCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
