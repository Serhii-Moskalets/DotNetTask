using DotNetTask.Application.Tasks.Commands.DeleteOverdueTasks;
using DotNetTask.Application.Tasks.Commands.DeleteRangeTasks;
using DotNetTask.Domain.Constants;
using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.Tasks.Commands.DeleteRangeTasks;

/// <summary>
/// Unit tests for <see cref="DeleteOverdueTasksCommandValidator"/>.
/// Ensures that the validator correctly checks task list ownership.
/// </summary>
public class DeleteRangeTasksCommandValidatorTests
{
    private readonly DeleteRangeTasksCommandValidator _validator = new();

    /// <summary>
    /// Verifies that a validation error occurs when the <c>TaskIds</c> collection is null.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_TaskIds_Is_Null()
    {
        // Arrange
        DeleteRangeTasksCommand command = new(TaskIds: null!, UserId: Guid.NewGuid());

        // Act
        TestValidationResult<DeleteRangeTasksCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.TaskIds)
            .WithErrorMessage(TaskPolicy.EmptyTaskIdsCollectionMessage);
    }

    /// <summary>
    /// Verifies that a validation error occurs when the <c>TaskIds</c> collection is null.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_TaskIds_Have_GuidEmpty()
    {
        // Arrange
        DeleteRangeTasksCommand command = new(TaskIds: [Guid.NewGuid(), Guid.Empty], UserId: Guid.NewGuid());

        // Act
        TestValidationResult<DeleteRangeTasksCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.TaskIds)
            .WithErrorMessage(TaskPolicy.EmptyTaskIdsCollectionMessage);
    }

    /// <summary>
    /// Verifies that a validation error occurs when the <c>TaskIds</c> collection contains duplicate identifiers.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_TaskIds_Contains_Duplicates()
    {
        // Arrange
        Guid taskId = Guid.NewGuid();
        DeleteRangeTasksCommand command = new(TaskIds: [taskId, taskId], UserId: Guid.NewGuid());

        // Act
        TestValidationResult<DeleteRangeTasksCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.TaskIds)
            .WithErrorMessage(TaskPolicy.DuplicateTaskIdsMessage);
    }

    /// <summary>
    /// Verifies that a validation error occurs when the <c>UserId</c> is an empty GUID.
    /// </summary>
    [Fact]
    public void Should_HaveError_When_UserId_Is_Empty()
    {
        // Arrange
        DeleteRangeTasksCommand command = new(TaskIds: [Guid.NewGuid()], UserId: Guid.Empty);

        // Act
        TestValidationResult<DeleteRangeTasksCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.UserId)
            .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Verifies that no validation errors occur when all command properties are valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_TaskIds_Are_Valid_And_UserId()
    {
        // Arrange
        DeleteRangeTasksCommand command = new(TaskIds: [Guid.NewGuid(), Guid.NewGuid()], UserId: Guid.NewGuid());

        // Act
        TestValidationResult<DeleteRangeTasksCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.TaskIds);
        result.ShouldNotHaveValidationErrorFor(c => c.UserId);
    }
}
