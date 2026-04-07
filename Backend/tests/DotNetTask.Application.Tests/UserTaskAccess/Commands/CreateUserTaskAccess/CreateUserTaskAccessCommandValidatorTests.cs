using DotNetTask.Application.UserTaskAccess.Commands.CreateUserTaskAccess;
using DotNetTask.Domain.Constants;

using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.UserTaskAccess.Commands.CreateUserTaskAccess;

/// <summary>
/// Tests for <see cref="CreateUserTaskAccessCommandValidator"/>.
/// </summary>
public class CreateUserTaskAccessCommandValidatorTests
{
    private readonly CreateUserTaskAccessCommandValidator _validator = new();

    /// <summary>
    /// Ensures validation fails when the task identifier is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_TaskId_Is_Empty()
    {
        // Arrange
        CreateUserTaskAccessCommand command = new(Guid.Empty, Guid.NewGuid(), "test@test.com");

        // Act
        TestValidationResult<CreateUserTaskAccessCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TaskId)
              .WithErrorMessage(TaskPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Ensures validation fails when the owner identifier is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_OwnerId_Is_Empty()
    {
        // Arrange
        CreateUserTaskAccessCommand command = new(Guid.NewGuid(), Guid.Empty, "test@test.com");

        // Act
        TestValidationResult<CreateUserTaskAccessCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OwnerId)
              .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Ensures validation fails when the email address is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        // Arrange
        CreateUserTaskAccessCommand command = new(Guid.NewGuid(), Guid.NewGuid(), string.Empty);

        // Act
        TestValidationResult<CreateUserTaskAccessCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage(EmailPolicy.EmptyMessage);
    }

    /// <summary>
    /// Ensures validation fails for clearly invalid email formats.
    /// </summary>
    /// <param name="email">An email value that does not meet basic format requirements.</param>
    [Theory]
    [InlineData("example")]
    [InlineData("@example.com")]
    public void Should_Fail_For_Clearly_Invalid_Emails(string email)
    {
        // Arrange
        CreateUserTaskAccessCommand command = new(Guid.NewGuid(), Guid.NewGuid(), email);

        // Act
        TestValidationResult<CreateUserTaskAccessCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage(EmailPolicy.InvalidFormatMessage);
    }

    /// <summary>
    /// Ensures no validation errors are returned when all command fields are valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        // Arrange
        CreateUserTaskAccessCommand command = new(Guid.NewGuid(), Guid.NewGuid(), "test@example.com");

        // Act
        TestValidationResult<CreateUserTaskAccessCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
