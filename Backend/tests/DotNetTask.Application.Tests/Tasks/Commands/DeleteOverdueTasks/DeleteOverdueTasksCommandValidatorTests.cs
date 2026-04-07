using DotNetTask.Application.Tasks.Commands.DeleteOverdueTasks;
using DotNetTask.Domain.Constants;

using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.Tasks.Commands.DeleteOverdueTasks;

/// <summary>
/// Unit tests for <see cref="DeleteOverdueTasksCommandValidator"/>.
/// Ensures that the validator correctly checks task list ownership.
/// </summary>
public class DeleteOverdueTasksCommandValidatorTests
{
    private readonly DeleteOverdueTasksCommandValidator _validator = new();

    /// <summary>
    /// Fails validation when TaskListId is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_TaskListId_Is_Empty()
    {
        // Arrange
        DeleteOverdueTasksCommand command = new(TaskListId: Guid.Empty, UserId: Guid.NewGuid());

        // Act
        TestValidationResult<DeleteOverdueTasksCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.TaskListId)
              .WithErrorMessage(TaskListPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Fails validation when UserId is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        DeleteOverdueTasksCommand command = new(TaskListId: Guid.NewGuid(), UserId: Guid.Empty);

        // Act
        TestValidationResult<DeleteOverdueTasksCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.UserId)
              .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Passes validation when both TaskListId and UserId are valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_TaskListId_And_UserId_Are_Valid()
    {
        // Arrange
        DeleteOverdueTasksCommand command = new(TaskListId: Guid.NewGuid(), UserId: Guid.NewGuid());

        // Act
        TestValidationResult<DeleteOverdueTasksCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.TaskListId);
        result.ShouldNotHaveValidationErrorFor(c => c.UserId);
    }
}
