using DotNetTask.Application.Users.Commands.UpdatePassword;
using DotNetTask.Domain.Constants;

using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.Users.Commands.UpdatePassword;

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
        UpdatePasswordCommand command = new("OldPassword123!", "NewPassword456!", Guid.NewGuid());

        // Act & Assert
        TestValidationResult<UpdatePasswordCommand> result = this._validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Verifies that the validator returns an error when the new password is identical to the current one.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_NewPassword_Is_Same_As_Current()
    {
        // Arrange
        string password = "SafePassword123!";
        UpdatePasswordCommand command = new(password, password, Guid.NewGuid());

        // Act & Assert
        TestValidationResult<UpdatePasswordCommand> result = this._validator.TestValidate(command);
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
        UpdatePasswordCommand command = new("OldPass123!", "NewPass456!", Guid.Empty);

        // Act & Assert
        TestValidationResult<UpdatePasswordCommand> result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Verifies that an empty current password triggers the appropriate validation error.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_CurrentPassword_Is_Empty()
    {
        // Arrange
        UpdatePasswordCommand command = new(string.Empty, "NewPass456!", Guid.NewGuid());

        // Act & Assert
        TestValidationResult<UpdatePasswordCommand> result = this._validator.TestValidate(command);
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
        UpdatePasswordCommand command = new("ValidOldPass1!", weakPassword, Guid.NewGuid());

        // Act & Assert
        TestValidationResult<UpdatePasswordCommand> result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage(expectedMessage);
    }
}
