using FluentValidation.TestHelper;
using TodoListApp.Application.TaskList.Commands.UpdateTaskList;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tests.TaskList.Commands;

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
        var command = new UpdateTaskListCommand(Guid.NewGuid(), Guid.NewGuid(), string.Empty);

        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.NewTitle)
            .WithErrorMessage("New title cannot be null or empty.");
    }

    /// <summary>
    /// Should have a validation error when the new title exceeds the maximum length.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_NewTitle_Exceeds_MaxLength()
    {
        var longTitle = new string('A', TaskListTitle.MaxLength + 1);
        var command = new UpdateTaskListCommand(Guid.NewGuid(), Guid.NewGuid(), longTitle);

        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.NewTitle)
            .WithErrorMessage($"Title cannot exceed {TaskListTitle.MaxLength} characters.");
    }

    /// <summary>
    /// Should have a validation error when the user ID is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        var command = new UpdateTaskListCommand(Guid.NewGuid(), Guid.Empty, "Valid Title");

        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.UserId)
            .WithErrorMessage("User ID is required.");
    }

    /// <summary>
    /// Should have a validation error when the task list ID is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_TaskListId_Is_Empty()
    {
        var command = new UpdateTaskListCommand(Guid.Empty, Guid.NewGuid(), "Valid Title");

        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.TaskListId)
            .WithErrorMessage("Task list ID is required.");
    }

    /// <summary>
    /// Should not have any validation errors for a valid command.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_For_Valid_Command()
    {
        var command = new UpdateTaskListCommand(Guid.NewGuid(), Guid.NewGuid(), "Valid Title");

        var result = this._validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}