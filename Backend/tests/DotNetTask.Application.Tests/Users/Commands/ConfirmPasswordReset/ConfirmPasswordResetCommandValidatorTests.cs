using DotNetTask.Application.Users.Commands.ConfirmPasswordReset;
using DotNetTask.Domain.Constants;

using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.Users.Commands.ConfirmPasswordReset;

/// <summary>
/// Contains unit tests for the <see cref="ConfirmPasswordResetCommandValidator"/> class.
/// </summary>
public class ConfirmPasswordResetCommandValidatorTests
{
    private readonly ConfirmPasswordResetCommandValidator _validator;

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
        ConfirmPasswordResetCommand command = new ConfirmPasswordResetCommand(
            "SecurePass123!",
            "ValidToken123");

        // Act & Assert
        TestValidationResult<ConfirmPasswordResetCommand> result = this._validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Verifies that an empty token triggers a validation error.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_Token_Is_Empty()
    {
        // Arrange
        ConfirmPasswordResetCommand command = new ConfirmPasswordResetCommand("SecurePass123!", string.Empty);

        // Act & Assert
        TestValidationResult<ConfirmPasswordResetCommand> result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Token)
              .WithErrorMessage(TokenPolicy.RequiredMessage);
    }

    /// <summary>
    /// Verifies that a password shorter than 8 characters triggers a validation error.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_Password_Is_Too_Short()
    {
        // Arrange
        ConfirmPasswordResetCommand command = new ConfirmPasswordResetCommand("token", "Short1!");

        // Act & Assert
        TestValidationResult<ConfirmPasswordResetCommand> result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
              .WithErrorMessage(PasswordPolicy.TooShortMessage);
    }

    /// <summary>
    /// Verifies that a password missing an uppercase letter triggers a validation error.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_Password_Missing_Uppercase()
    {
        // Arrange
        ConfirmPasswordResetCommand command = new ConfirmPasswordResetCommand("lowercase123!", "token");

        // Act & Assert
        TestValidationResult<ConfirmPasswordResetCommand> result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
              .WithErrorMessage(PasswordPolicy.UppercaseMessage);
    }

    /// <summary>
    /// Verifies that a password missing a lowercase letter triggers a validation error.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_Password_Missing_Lowercase()
    {
        // Arrange
        ConfirmPasswordResetCommand command = new ConfirmPasswordResetCommand("UPPERCASE123!", "token");

        // Act & Assert
        TestValidationResult<ConfirmPasswordResetCommand> result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
              .WithErrorMessage(PasswordPolicy.LowercaseMessage);
    }

    /// <summary>
    /// Verifies that a password missing a digit triggers a validation error.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_Password_Missing_Number()
    {
        // Arrange
        ConfirmPasswordResetCommand command = new ConfirmPasswordResetCommand("NoNumbers!", "token");

        // Act & Assert
        TestValidationResult<ConfirmPasswordResetCommand> result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
              .WithErrorMessage(PasswordPolicy.NumberMessage);
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
        ConfirmPasswordResetCommand command = new ConfirmPasswordResetCommand("token", password);

        // Act & Assert
        TestValidationResult<ConfirmPasswordResetCommand> result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
              .WithErrorMessage(PasswordPolicy.SpecialCharMessage);
    }
}
