using DotNetTask.Application.TaskList.Commands.UpdateTaskList;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.ValueObjects;

using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.TaskList.Commands;

/// <summary>
/// Unit tests for <see cref="UpdateTaskListCommandValidator"/>.
/// Ensures that the validator correctly enforces rules for updating a task list.
/// </summary>
public class UpdateTaskListCommandValidatorTests
{
    private readonly UpdateTaskListCommandValidator _validator = new();

    /// <summary>
    /// Should have a validation error when the new title is null or empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_NewTitle_Is_Null_Or_Empty()
    {
        UpdateTaskListCommand command = new UpdateTaskListCommand(Guid.NewGuid(), Guid.NewGuid(), string.Empty);

        TestValidationResult<UpdateTaskListCommand> result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.NewTitle)
            .WithErrorMessage(TaskListPolicy.EmptyMessage);
    }

    /// <summary>
    /// Should have a validation error when the new title exceeds the maximum length.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_NewTitle_Exceeds_MaxLength()
    {
        string longTitle = new string('A', TaskListTitle.MaxLength + 1);
        UpdateTaskListCommand command = new UpdateTaskListCommand(Guid.NewGuid(), Guid.NewGuid(), longTitle);

        TestValidationResult<UpdateTaskListCommand> result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.NewTitle)
            .WithErrorMessage(TaskListPolicy.TooLongMessage);
    }

    /// <summary>
    /// Should have a validation error when the user ID is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        UpdateTaskListCommand command = new UpdateTaskListCommand(Guid.NewGuid(), Guid.Empty, "Valid Title");

        TestValidationResult<UpdateTaskListCommand> result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.UserId)
            .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Should have a validation error when the task list ID is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_TaskListId_Is_Empty()
    {
        UpdateTaskListCommand command = new UpdateTaskListCommand(Guid.Empty, Guid.NewGuid(), "Valid Title");

        TestValidationResult<UpdateTaskListCommand> result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.TaskListId)
            .WithErrorMessage(TaskListPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Should not have any validation errors for a valid command.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_For_Valid_Command()
    {
        UpdateTaskListCommand command = new UpdateTaskListCommand(Guid.NewGuid(), Guid.NewGuid(), "Valid Title");

        TestValidationResult<UpdateTaskListCommand> result = this._validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}