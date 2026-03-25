using DotNetTask.Application.UserTaskAccess.Commands.DeleteTaskAccessByUserEmail;
using DotNetTask.Domain.Constants;

using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.UserTaskAccess.Commands;

/// <summary>
/// Tests for <see cref="DeleteTaskAccessByUserEmailCommandValidator"/>.
/// </summary>
public class DeleteTaskAccessByUserEmailCommandValidatorTests
{
    private readonly DeleteTaskAccessByUserEmailCommandValidator _validator = new();

    /// <summary>
    /// Ensures validation fails when the task identifier is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_TaskId_Is_Empty()
    {
        DeleteTaskAccessByUserEmailCommand command = new DeleteTaskAccessByUserEmailCommand(Guid.Empty, Guid.NewGuid(), "test@test.com");
        TestValidationResult<DeleteTaskAccessByUserEmailCommand> result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TaskId)
              .WithErrorMessage(TaskPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Ensures validation fails when the owner identifier is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_OwnerId_Is_Empty()
    {
        DeleteTaskAccessByUserEmailCommand command = new DeleteTaskAccessByUserEmailCommand(Guid.NewGuid(), Guid.Empty, "test@test.com");
        TestValidationResult<DeleteTaskAccessByUserEmailCommand> result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OwnerId)
              .WithErrorMessage(UserTaskAccessPolicy.OwnerIdRequired);
    }

    /// <summary>
    /// Ensures validation fails when the email address is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        DeleteTaskAccessByUserEmailCommand command = new DeleteTaskAccessByUserEmailCommand(Guid.NewGuid(), Guid.NewGuid(), string.Empty);
        TestValidationResult<DeleteTaskAccessByUserEmailCommand> result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage(EmailPolicy.EmptyMessage);
    }

    /// <summary>
    /// Ensures validation fails for clearly invalid email formats.
    /// </summary>
    /// <param name="email">
    /// An email value that does not meet basic format requirements.
    /// </param>
    [Theory]
    [InlineData("example")]
    [InlineData("@example.com")]
    public void Should_Have_Error_When_Email_Is_Invalid(string email)
    {
        DeleteTaskAccessByUserEmailCommand command = new DeleteTaskAccessByUserEmailCommand(Guid.NewGuid(), Guid.NewGuid(), email);
        TestValidationResult<DeleteTaskAccessByUserEmailCommand> result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage(EmailPolicy.InvalidFormatMessage);
    }

    /// <summary>
    /// Ensures no validation errors are returned when all command fields are valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        DeleteTaskAccessByUserEmailCommand command = new DeleteTaskAccessByUserEmailCommand(Guid.NewGuid(), Guid.NewGuid(), "test@example.com");
        TestValidationResult<DeleteTaskAccessByUserEmailCommand> result = this._validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
