using FluentValidation.TestHelper;
using TodoListApp.Application.Tasks.Commands.DeleteOverdueTasks;
using TodoListApp.Domain.Constants;

namespace TodoListApp.Application.Tests.Tasks.Commands;

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
        var command = new DeleteOverdueTasksCommand(TaskListId: Guid.Empty, UserId: Guid.NewGuid());

        // Act
        var result = this._validator.TestValidate(command);

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
        var command = new DeleteOverdueTasksCommand(TaskListId: Guid.NewGuid(), UserId: Guid.Empty);

        // Act
        var result = this._validator.TestValidate(command);

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
        var command = new DeleteOverdueTasksCommand(TaskListId: Guid.NewGuid(), UserId: Guid.NewGuid());

        // Act
        var result = this._validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.TaskListId);
        result.ShouldNotHaveValidationErrorFor(c => c.UserId);
    }
}
