using DotNetTask.Application.UserTaskAccess.Commands.DeleteTaskAccessById;
using DotNetTask.Domain.Constants;

using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.UserTaskAccess.Commands;

/// <summary>
/// Tests for <see cref="DeleteTaskAccessByIdCommandValidator"/>.
/// </summary>
public class DeleteTaskAccessByIdCommandValidatorTests
{
    private readonly DeleteTaskAccessByIdCommandValidator _validator = new();

    /// <summary>
    /// Ensures validation fails when the task identifier is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_TaskId_Is_Empty()
    {
        DeleteTaskAccessByIdCommand command = new DeleteTaskAccessByIdCommand(Guid.Empty, Guid.NewGuid(), Guid.NewGuid());
        TestValidationResult<DeleteTaskAccessByIdCommand> result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TaskId)
              .WithErrorMessage(TaskPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Ensures validation fails when the user identifier is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        DeleteTaskAccessByIdCommand command = new DeleteTaskAccessByIdCommand(Guid.NewGuid(), Guid.Empty, Guid.NewGuid());
        TestValidationResult<DeleteTaskAccessByIdCommand> result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Ensures validation fails when the owner identifier is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_OwnerId_Is_Empty()
    {
        DeleteTaskAccessByIdCommand command = new DeleteTaskAccessByIdCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty);
        TestValidationResult<DeleteTaskAccessByIdCommand> result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OwnerId)
              .WithErrorMessage(UserTaskAccessPolicy.OwnerIdRequired);
    }

    /// <summary>
    /// Ensures no validation errors are returned when all command fields are valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        DeleteTaskAccessByIdCommand command = new DeleteTaskAccessByIdCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        TestValidationResult<DeleteTaskAccessByIdCommand> result = this._validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
