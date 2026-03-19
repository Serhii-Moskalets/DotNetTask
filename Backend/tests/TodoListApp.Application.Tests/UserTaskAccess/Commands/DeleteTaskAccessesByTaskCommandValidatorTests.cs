using FluentValidation.TestHelper;
using TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessesByTask;
using TodoListApp.Domain.Constants;

namespace TodoListApp.Application.Tests.UserTaskAccess.Commands;

/// <summary>
/// Tests for <see cref="DeleteTaskAccessesByTaskCommandValidator"/>.
/// </summary>
public class DeleteTaskAccessesByTaskCommandValidatorTests
{
    private readonly DeleteTaskAccessesByTaskCommandValidator _validator = new();

    /// <summary>
    /// Ensures validation fails when the task identifier is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_TaskId_Is_Empty()
    {
        var command = new DeleteTaskAccessesByTaskCommand(Guid.Empty, Guid.NewGuid());
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TaskId)
              .WithErrorMessage(TaskPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Ensures validation fails when the user identifier is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        var command = new DeleteTaskAccessesByTaskCommand(Guid.NewGuid(), Guid.Empty);
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Ensures no validation errors are returned when all command fields are valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        var command = new DeleteTaskAccessesByTaskCommand(Guid.NewGuid(), Guid.NewGuid());
        var result = this._validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
