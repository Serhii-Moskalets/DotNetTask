using FluentValidation.TestHelper;
using TodoListApp.Application.Users.Commands.RegisterUser;
using TodoListApp.Domain.Constants;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tests.Users.Commands.RegisterUser;

/// <summary>
/// Contains unit tests for the <see cref="RegisterUserCommandValidator"/> class.
/// </summary>
public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterUserCommandValidatorTests"/> class.
    /// </summary>
    public RegisterUserCommandValidatorTests()
    {
        this._validator = new RegisterUserCommandValidator();
    }

    /// <summary>
    /// Provides invalid password samples and their corresponding expected error messages from <see cref="PasswordPolicy"/>.
    /// </summary>
    /// <returns>An enumeration of test data arrays.</returns>
    public static TheoryData<string, string> GetInvalidPasswords()
    {
        return new TheoryData<string, string>
        {
            { "short", PasswordPolicy.TooShortMessage },
            { "nocapital1!", PasswordPolicy.UppercaseMessage },
            { "NOLOWERCASE1!", PasswordPolicy.LowercaseMessage },
            { "NoDigit!!", PasswordPolicy.NumberMessage },
            { "NoSpecialChar1", PasswordPolicy.SpecialCharMessage },
        };
    }

    /// <summary>
    /// Verifies that valid registration data passes all validation rules.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new RegisterUserCommand("John", "Doe", "johndoe", "john@test.com", "Password123!");

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Verifies that empty mandatory fields trigger validation errors.
    /// </summary>
    /// <param name="firstName">The first name to validate.</param>
    /// <param name="userName">The username to validate.</param>
    /// <param name="email">The email to validate.</param>
    /// <param name="password">The password to validate.</param>
    [Theory]
    [InlineData("", "", "", "")]
    public void Should_Have_Errors_When_Fields_Are_Empty(string firstName, string userName, string email, string password)
    {
        // Arrange
        var command = new RegisterUserCommand(firstName, "Doe", userName, email, password);

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FirstName).WithErrorMessage(FirstNamePolicy.EmptyMessage);
        result.ShouldHaveValidationErrorFor(x => x.UserName).WithErrorMessage(UserNamePolicy.EmptyMessage);
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage(EmailPolicy.EmptyMessage);
        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorMessage(PasswordPolicy.EmptyMessage);
    }

    /// <summary>
    /// Verifies that validation errors are triggered when mandatory fields exceed their maximum allowed length.
    /// </summary>
    [Fact]
    public void Should_Have_Errors_When_Names_Exceed_Maximum_Length()
    {
        // Arrange
        var longFirstName = new string('a', FirstName.MaxLength + 1);
        var longLastName = new string('a', LastName.MaxLength + 1);
        var command = new RegisterUserCommand(longFirstName, longLastName, "UserName", "email@example.com", "paSsword!2");

        // Act
        var result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName).WithErrorMessage(FirstNamePolicy.TooLongMessage);
        result.ShouldHaveValidationErrorFor(x => x.LastName).WithErrorMessage(LastNamePolicy.TooLongMessage);
    }

    /// <summary>
    /// Verifies that UserName length restrictions are enforced.
    /// </summary>
    /// <param name="userName">The username with invalid length.</param>
    [Theory]
    [InlineData("jo")]
    [InlineData("thisusernameiswaytoolongforoursystem")]
    public void Should_Have_Error_When_UserName_Length_Is_Invalid(string userName)
    {
        // Arrange
        var command = new RegisterUserCommand("John", "Doe", userName, "john@test.com", "Password123!");

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserName)
            .WithErrorMessage(UserNamePolicy.LengthMessage);
    }

    /// <summary>
    /// Verifies that <see cref="UserName"/> triggers a format error message when it contains invalid characters.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserName_Format_Is_Invalid()
    {
        // Arrange
        var command = new RegisterUserCommand("John", "Doe", "user@name!", "john@test.com", "Password123!");

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserName)
            .WithErrorMessage(UserNamePolicy.InvalidCharactersMessage);
    }

    /// <summary>
    /// Verifies that each password complexity rule (length, casing, digits, special characters) is correctly enforced.
    /// </summary>
    /// <param name="password">The password that fails complexity rules.</param>
    /// <param name="expectedErrorMessage">The policy-defined error message expected for the failure.</param>
    [Theory]
    [MemberData(nameof(GetInvalidPasswords))]
    public void Should_Have_Error_When_Password_Complexity_Is_Not_Met(string password, string expectedErrorMessage)
    {
        // Arrange
        var command = new RegisterUserCommand("John", "Doe", "johndoe", "john@test.com", password);

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage(expectedErrorMessage);
    }
}
