using DotNetTask.Application.Tasks.Commands.RemoveTagFromTask;
using DotNetTask.Domain.Constants;

using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.Tasks.Commands;

/// <summary>
/// Unit tests for <see cref="RemoveTagFromTaskCommandValidator"/>.
/// Verifies that the validator correctly enforces rules for removing a tag from a task.
/// </summary>
public class RemoveTagFromTaskCommandTests
{
    private readonly RemoveTagFromTaskCommandValidator _validator = new();

    /// <summary>
    /// Ensures validation fails when the task ID is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_TaskId_Is_Empty()
    {
        // Arrange
        RemoveTagFromTaskCommand command = new RemoveTagFromTaskCommand(TaskId: Guid.Empty, UserId: Guid.NewGuid());

        // Act
        TestValidationResult<RemoveTagFromTaskCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.TaskId)
              .WithErrorMessage(TaskPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Ensures validation fails when the user ID is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        RemoveTagFromTaskCommand command = new RemoveTagFromTaskCommand(TaskId: Guid.NewGuid(), UserId: Guid.Empty);

        // Act
        TestValidationResult<RemoveTagFromTaskCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.UserId)
              .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Ensures no validation errors occur when both task ID and user ID are valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_TaskId_And_UserId_Are_Valid()
    {
        // Arrange
        RemoveTagFromTaskCommand command = new RemoveTagFromTaskCommand(TaskId: Guid.NewGuid(), UserId: Guid.NewGuid());

        // Act
        TestValidationResult<RemoveTagFromTaskCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.TaskId);
        result.ShouldNotHaveValidationErrorFor(c => c.UserId);
    }
}
