using FluentValidation.TestHelper;
using TodoListApp.Application.Tasks.Commands.AddTagToTask;
using TodoListApp.Domain.Constants;

namespace TodoListApp.Application.Tests.Tasks.Commands;

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
        var command = new AddTagToTaskCommand(Guid.Empty, Guid.NewGuid(), Guid.NewGuid());
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TaskId)
              .WithErrorMessage(TaskPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Ensures validation fails when the user ID is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        var command = new AddTagToTaskCommand(Guid.NewGuid(), Guid.Empty, Guid.NewGuid());
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Ensures validation fails when the tag ID is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_TagId_Is_Empty()
    {
        var command = new AddTagToTaskCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty);
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TagId)
              .WithErrorMessage(TagPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Ensures no validation errors when the command is valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        var command = new AddTagToTaskCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var result = this._validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
