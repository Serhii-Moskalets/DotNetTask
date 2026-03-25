using DotNetTask.Application.Tasks.Commands.ChangeTaskStatus;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.Enums;

using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.Tasks.Commands;

/// <summary>
/// Unit tests for <see cref="ChangeTaskStatusCommandValidator"/>.
/// Verifies that the validator correctly enforces rules for changing task status.
/// </summary>
public class ChangeTaskStatusCommandValidatorTests
{
    private readonly ChangeTaskStatusCommandValidator _validator = new();

    /// <summary>
    /// Ensures validation fails when the task ID is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_TaskId_Is_Empty()
    {
        ChangeTaskStatusCommand command = new ChangeTaskStatusCommand(Guid.Empty, Guid.NewGuid(), StatusTask.Done);
        TestValidationResult<ChangeTaskStatusCommand> result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TaskId)
            .WithErrorMessage(TaskPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Ensures validation fails when the user ID is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        ChangeTaskStatusCommand command = new ChangeTaskStatusCommand(Guid.NewGuid(), Guid.Empty, StatusTask.Done);
        TestValidationResult<ChangeTaskStatusCommand> result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Ensures validation fails when the task status is invalid.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_Status_Is_Invalid()
    {
        ChangeTaskStatusCommand command = new ChangeTaskStatusCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            (StatusTask)999);

        TestValidationResult<ChangeTaskStatusCommand> result = this._validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Status)
              .WithErrorMessage(TaskPolicy.InvalidTaskSatusMessage);
    }

    /// <summary>
    /// Ensures no validation errors when the command is valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Errors_When_Command_Is_Valid()
    {
        ChangeTaskStatusCommand command = new ChangeTaskStatusCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            StatusTask.Done);

        TestValidationResult<ChangeTaskStatusCommand> result = this._validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
