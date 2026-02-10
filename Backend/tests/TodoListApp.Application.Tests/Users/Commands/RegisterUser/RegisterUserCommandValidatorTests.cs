using FluentValidation.TestHelper;
using TodoListApp.Application.Users.Commands.RegisterUser;

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
        result.ShouldHaveValidationErrorFor(x => x.FirstName).WithErrorMessage("First name is required.");
        result.ShouldHaveValidationErrorFor(x => x.UserName).WithErrorMessage("Username is required.");
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("Email is required.");
        result.ShouldHaveValidationErrorFor(x => x.Password).WithErrorMessage("Password is required.");
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
            .WithErrorMessage("Username must be between 3 and 20 characters.");
    }

    /// <summary>
    /// Verifies that various password complexity requirements are enforced.
    /// </summary>
    /// <param name="password">The password that fails complexity rules.</param>
    /// <param name="expectedErrorMessage">The expected error message for the specific failure.</param>
    [Theory]
    [InlineData("short", "Password must be at least 8 characters long.")]
    [InlineData("nocapital1!", "Password must contain at least one uppercase letter.")]
    [InlineData("NOLOWERCASE1!", "Password must contain at least one lowercase letter.")]
    [InlineData("NoDigit!!", "Password must contain at least one number.")]
    [InlineData("NoSpecialChar1", "Password must contain at least one special character (!?*.).")]
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
