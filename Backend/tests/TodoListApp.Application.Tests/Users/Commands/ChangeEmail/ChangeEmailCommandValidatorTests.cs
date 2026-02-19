using FluentValidation.TestHelper;
using TodoListApp.Application.Users.Commands.ChangeEmail;

namespace TodoListApp.Application.Tests.Users.Commands.ChangeEmail;

/// <summary>
/// Contains unit tests for the <see cref="ChangeEmailCommandValidator"/> class.
/// </summary>
public class ChangeEmailCommandValidatorTests
{
    private const string ValidEmail = "example@example.com";

    private readonly ChangeEmailCommandValidator _validator = new();

    /// <summary>
    /// Verifies that valid data passes validation without errors.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new ChangeEmailCommand(ValidEmail, Guid.NewGuid());

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Verifies that an empty user id triggers a validation error.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        var command = new ChangeEmailCommand(ValidEmail, Guid.Empty);

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("User ID is required.");
    }

    /// <summary>
    /// Verifies that an empty or null email triggers a validation error.
    /// </summary>
    /// <param name="email">The email value to validate (null or empty string).</param>
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Have_Error_When_Email_Is_Empty(string? email)
    {
        // Arrange
        var command = new ChangeEmailCommand(email!, Guid.NewGuid());

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewEmail)
              .WithErrorMessage("Email is required.");
    }

    /// <summary>
    /// Verifies that an incorrect email format triggers a validation error.
    /// </summary>
    /// <param name="invalidEmail">The malformed email address.</param>
    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing-at-sign.com")]
    public void Should_Have_Error_When_Email_Format_Is_Incorrect(string invalidEmail)
    {
        // Arrange
        var command = new ChangeEmailCommand(invalidEmail, Guid.NewGuid());

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewEmail)
              .WithErrorMessage("Invalid email format.");
    }
}
