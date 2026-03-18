using FluentValidation.TestHelper;
using TodoListApp.Application.Users.Commands.UpdatePassword;
using TodoListApp.Domain.Constants;

namespace TodoListApp.Application.Tests.Users.Commands.UpdatePassword;

/// <summary>
/// Contains unit tests for the <see cref="UpdatePasswordCommandValidator"/> class.
/// </summary>
public class UpdatePasswordCommandValidatorTests
{
    private readonly UpdatePasswordCommandValidator _validator = new();

    /// <summary>
    /// Provides invalid password samples and their corresponding expected error messages from <see cref="PasswordPolicy"/>.
    /// </summary>
    /// <returns>An enumeration of test data arrays.</returns>
    public static TheoryData<string, string> GetInvalidPasswordData()
    {
        return new TheoryData<string, string>
        {
            { string.Empty, PasswordPolicy.EmptyMessage },
            { "short", PasswordPolicy.TooShortMessage },
            { "nouppercase1!", PasswordPolicy.UppercaseMessage },
            { "NOLOWERCASE1!", PasswordPolicy.LowercaseMessage },
            { "NoNumber!", PasswordPolicy.NumberMessage },
            { "NoSpecialChar1", PasswordPolicy.SpecialCharMessage },
        };
    }

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
            .WithErrorMessage(PasswordPolicy.SameAsOldMessage);
    }

    /// <summary>
    /// Verifies that an empty User ID triggers the appropriate validation error.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        var command = new UpdatePasswordCommand("OldPass123!", "NewPass456!", Guid.Empty);

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(UserPolicy.UserIdRequiredMessage);
    }

    /// <summary>
    /// Verifies that an empty current password triggers the appropriate validation error.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_CurrentPassword_Is_Empty()
    {
        // Arrange
        var command = new UpdatePasswordCommand(string.Empty, "NewPass456!", Guid.NewGuid());

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CurrentPassword)
            .WithErrorMessage(PasswordPolicy.EmptyMessage);
    }

    /// <summary>
    /// Verifies that various password complexity requirements are enforced for the new password.
    /// </summary>
    /// <param name="weakPassword">The password that fails complexity rules.</param>
    /// <param name="expectedMessage">The policy-defined error message expected for the failure.</param>
    [Theory]
    [MemberData(nameof(GetInvalidPasswordData))]
    public void Should_Have_Error_When_NewPassword_Complexity_Is_Not_Met(string weakPassword, string expectedMessage)
    {
        // Arrange
        var command = new UpdatePasswordCommand("ValidOldPass1!", weakPassword, Guid.NewGuid());

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage(expectedMessage);
    }
}
