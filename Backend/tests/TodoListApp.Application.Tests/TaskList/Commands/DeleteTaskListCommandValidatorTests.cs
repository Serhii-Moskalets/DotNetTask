using FluentValidation.TestHelper;
using TodoListApp.Application.TaskList.Commands.DeleteTaskList;
using TodoListApp.Domain.Constants;

namespace TodoListApp.Application.Tests.TaskList.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteTaskListCommandValidator"/>.
/// Ensures that validation rules for deleting a task list are enforced correctly.
/// </summary>
public class DeleteTaskListCommandValidatorTests
{
    private readonly DeleteTaskListCommandValidator _validator = new();

    /// <summary>
    /// Returns a validation error if <see cref="DeleteTaskListCommand.UserId"/> is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        var command = new DeleteTaskListCommand(Guid.NewGuid(), Guid.Empty);

        var result = this._validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Returns a validation error if <see cref="DeleteTaskListCommand.TaskListId"/> is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_TaskListId_Is_Empty()
    {
        var command = new DeleteTaskListCommand(Guid.Empty, Guid.NewGuid());

        var result = this._validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.TaskListId)
              .WithErrorMessage(TaskListPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Should not return any validation errors when the command is valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_Valid_Command()
    {
        var command = new DeleteTaskListCommand(Guid.NewGuid(), Guid.NewGuid());

        var result = this._validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
