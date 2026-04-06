using DotNetTask.Application.Users.Commands.ConfirmPasswordReset;
using DotNetTask.Domain.Constants;
using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.Users.Commands.ConfirmPasswordReset;

/// <summary>
/// Contains unit tests for the <see cref="ConfirmPasswordResetCommandValidator"/> class.
/// </summary>
public class ConfirmPasswordResetCommandValidatorTests
{
    private const string IpAddress = "192.168.0.1";
    private const string ValidPassword = "SecurePass123!";
    private const string ValidToken = "ValidToken";
    private readonly ConfirmPasswordResetCommandValidator _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfirmPasswordResetCommandValidatorTests"/> class.
    /// </summary>
    public ConfirmPasswordResetCommandValidatorTests() => this._validator = new ConfirmPasswordResetCommandValidator();

    /// <summary>
    /// Verifies that valid data passes validation without errors.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        ConfirmPasswordResetCommand command = new(ValidPassword, ValidToken, IpAddress);

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
        ConfirmPasswordResetCommand command = new(ValidPassword, string.Empty, IpAddress);

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
        ConfirmPasswordResetCommand command = new("Short1!", ValidToken, IpAddress);

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
        ConfirmPasswordResetCommand command = new("lowercase123!", ValidToken, IpAddress);

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
        ConfirmPasswordResetCommand command = new("UPPERCASE123!", ValidToken, IpAddress);

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
        ConfirmPasswordResetCommand command = new("NoNumbers!", ValidToken, IpAddress);

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
        ConfirmPasswordResetCommand command = new(password, ValidToken, IpAddress);

        // Act & Assert
        TestValidationResult<ConfirmPasswordResetCommand> result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
              .WithErrorMessage(PasswordPolicy.SpecialCharMessage);
    }

    /// <summary>
    /// Verifies that an invalid IP address format triggers a validation error.
    /// </summary>
    /// <param name="invalidIp">The malformed IP address string.</param>
    [Theory]
    [InlineData("not-an-ip")]
    [InlineData("256.256.256.256")]
    [InlineData("192.168.1")]
    [InlineData("...")]
    public void Should_Have_Error_When_IpAddress_Is_Invalid(string invalidIp)
    {
        // Arrange
        ConfirmPasswordResetCommand command = new(ValidPassword, ValidToken, invalidIp);

        // Act
        TestValidationResult<ConfirmPasswordResetCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.IpAddress)
              .WithErrorMessage(CommonPolicy.InvalidIpAddressMessage);
    }

    /// <summary>
    /// Verifies that an empty IP address triggers a validation error.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_IpAddress_Is_Empty()
    {
        // Arrange
        ConfirmPasswordResetCommand command = new(ValidPassword, ValidToken, string.Empty);

        // Act
        TestValidationResult<ConfirmPasswordResetCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.IpAddress)
              .WithErrorMessage(CommonPolicy.InvalidIpAddressMessage);
    }
}
