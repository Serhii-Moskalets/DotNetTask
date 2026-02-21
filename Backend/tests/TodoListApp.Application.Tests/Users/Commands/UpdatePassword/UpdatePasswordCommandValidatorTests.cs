using FluentValidation.TestHelper;
using TodoListApp.Application.Users.Commands.UpdatePassword;

namespace TodoListApp.Application.Tests.Users.Commands.UpdatePassword;

/// <summary>
/// Contains unit tests for the <see cref="UpdatePasswordCommandValidator"/> class.
/// </summary>
public class UpdatePasswordCommandValidatorTests
{
    private readonly UpdatePasswordCommandValidator _validator = new();

    /// <summary>
    /// Verifies that the validator does not return any errors when all password requirements are met.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new UpdatePasswordCommand("OldPassword123!", "NewPassword456!", Guid.NewGuid());

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Verifies that the validator returns an error when the new password is identical to the current one.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_NewPassword_Is_Same_As_Current()
    {
        // Arrange
        var password = "SafePassword123!";
        var command = new UpdatePasswordCommand(password, password, Guid.NewGuid());

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("New password and current password cannot be the same.");
    }

    /// <summary>
    /// Verifies that empty or invalid mandatory fields trigger validation errors.
    /// </summary>
    /// <param name="currentPass">The current password to validate.</param>
    /// <param name="newPass">The new password to validate.</param>
    /// <param name="expectedMessage">The expected error message for the specific failure.</param>
    [Theory]
    [InlineData("", "NewPass123!", "User ID is required.")]
    [InlineData("Current123!", "", "Password is required.")]
    public void Should_Have_Error_When_Required_Fields_Are_Empty(string currentPass, string newPass, string expectedMessage)
    {
        // Arrange
        var command = new UpdatePasswordCommand(currentPass, newPass, Guid.Empty);

        // Act & Assert
        var result = this._validator.TestValidate(command);

        if (string.IsNullOrEmpty(currentPass))
        {
            result.ShouldHaveValidationErrorFor(x => x.CurrentPassword);
        }

        if (string.IsNullOrEmpty(newPass))
        {
            result.ShouldHaveValidationErrorFor(x => x.NewPassword).WithErrorMessage(expectedMessage);
        }

        result.ShouldHaveValidationErrorFor(x => x.UserId).WithErrorMessage("User ID is required.");
    }

    /// <summary>
    /// Verifies that various password complexity requirements are enforced for the new password.
    /// </summary>
    /// <param name="weakPassword">The password that fails complexity rules.</param>
    /// <param name="expectedMessage">The expected error message for the specific failure.</param>
    [Theory]
    [InlineData("short", "Password must be at least 8 characters long.")]
    [InlineData("nouppercase1!", "Password must contain at least one uppercase letter.")]
    [InlineData("NOLOWERCASE1!", "Password must contain at least one lowercase letter.")]
    [InlineData("NoNumber!", "Password must contain at least one number.")]
    [InlineData("NoSpecialChar1", "Password must contain at least one special character (!?*.).")]
    public void Should_Have_Error_When_Password_Complexity_Is_Not_Met(string weakPassword, string expectedMessage)
    {
        // Arrange
        var command = new UpdatePasswordCommand("ValidOldPass1!", weakPassword, Guid.NewGuid());

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage(expectedMessage);
    }
}
