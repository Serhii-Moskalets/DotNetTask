using DotNetTask.Application.Tasks.Commands.DeleteTask;
using DotNetTask.Domain.Constants;

using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.Tasks.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteTaskCommandValidator"/>.
/// Ensures that the validator correctly enforces rules for deleting a task.
/// </summary>
public class DeleteTaskCommandValidatorTests
{
    private readonly DeleteTaskCommandValidator _validator = new();

    /// <summary>
    /// Fails validation when TaskId is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_TaskId_Is_Empty()
    {
        // Arrange
        DeleteTaskCommand command = new(TaskId: Guid.Empty, UserId: Guid.NewGuid());

        // Act
        TestValidationResult<DeleteTaskCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.TaskId)
              .WithErrorMessage(TaskPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Fails validation when UserId is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        DeleteTaskCommand command = new(TaskId: Guid.NewGuid(), UserId: Guid.Empty);

        // Act
        TestValidationResult<DeleteTaskCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.UserId)
              .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Passes validation when both TaskId and UserId are valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_TaskId_And_UserId_Are_Valid()
    {
        // Arrange
        DeleteTaskCommand command = new(TaskId: Guid.NewGuid(), UserId: Guid.NewGuid());

        // Act
        TestValidationResult<DeleteTaskCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.TaskId);
        result.ShouldNotHaveValidationErrorFor(c => c.UserId);
    }
}
