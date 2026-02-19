using FluentValidation.TestHelper;
using TodoListApp.Application.Users.Commands.ConfirmPasswordReset;

namespace TodoListApp.Application.Tests.Users.Commands.ConfirmPasswordReset;

/// <summary>
/// Contains unit tests for the <see cref="ConfirmPasswordResetCommandValidator"/> class.
/// </summary>
public class ConfirmPasswordResetCommandValidatorTests
{
    private readonly ConfirmPasswordResetCommandValidator _validator;
    private readonly Guid userId = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfirmPasswordResetCommandValidatorTests"/> class.
    /// </summary>
    public ConfirmPasswordResetCommandValidatorTests()
    {
        this._validator = new ConfirmPasswordResetCommandValidator();
    }

    /// <summary>
    /// Verifies that valid data passes validation without errors.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new ConfirmPasswordResetCommand(
            this.userId,
            "SecurePass123!",
            "ValidToken123");

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Verifies that an empty token triggers a validation error.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_Token_Is_Empty()
    {
        // Arrange
        var command = new ConfirmPasswordResetCommand(this.userId, "SecurePass123!", string.Empty);

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Token)
              .WithErrorMessage("Token is required.");
    }

    /// <summary>
    /// Verifies that an empty userId triggers a validation error.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        var command = new ConfirmPasswordResetCommand(Guid.Empty, "SecurePass123!", "token");

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage("UserId is required.");
    }

    /// <summary>
    /// Verifies that a password shorter than 8 characters triggers a validation error.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_Password_Is_Too_Short()
    {
        // Arrange
        var command = new ConfirmPasswordResetCommand(this.userId, "token", "Short1!");

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
              .WithErrorMessage("Password must be at least 8 characters long.");
    }

    /// <summary>
    /// Verifies that a password missing an uppercase letter triggers a validation error.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_Password_Missing_Uppercase()
    {
        // Arrange
        var command = new ConfirmPasswordResetCommand(this.userId, "lowercase123!", "token");

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
              .WithErrorMessage("Password must contain at least one uppercase letter.");
    }

    /// <summary>
    /// Verifies that a password missing a lowercase letter triggers a validation error.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_Password_Missing_Lowercase()
    {
        // Arrange
        var command = new ConfirmPasswordResetCommand(this.userId, "UPPERCASE123!", "token");

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
              .WithErrorMessage("Password must contain at least one lowercase letter.");
    }

    /// <summary>
    /// Verifies that a password missing a digit triggers a validation error.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_Password_Missing_Number()
    {
        // Arrange
        var command = new ConfirmPasswordResetCommand(this.userId, "NoNumbers!", "token");

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
              .WithErrorMessage("Password must contain at least one number.");
    }

    /// <summary>
    /// Verifies that a password missing allowed special characters (!?*.) triggers a validation error.
    /// </summary>
    /// <param name="password">The password with missing or incorrect special characters.</param>
    [Theory]
    [InlineData("NoSpecialChar123")]
    [InlineData("OnlyOtherSpecial#")]
    public void Should_Have_Error_When_Password_Missing_Allowed_Special_Character(string password)
    {
        // Arrange
        var command = new ConfirmPasswordResetCommand(this.userId, "token", password);

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
              .WithErrorMessage("Password must contain at least one special character (!?*.).");
    }
}
