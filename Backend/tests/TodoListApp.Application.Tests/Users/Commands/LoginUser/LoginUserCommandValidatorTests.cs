using FluentValidation.TestHelper;
using TodoListApp.Application.Users.Commands.LoginUser;

namespace TodoListApp.Application.Tests.Users.Commands.LoginUser;

/// <summary>
/// Contains unit tests for the <see cref="LoginUserCommandValidator"/> class.
/// </summary>
public class LoginUserCommandValidatorTests
{
    private readonly LoginUserCommandValidator _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginUserCommandValidatorTests"/> class.
    /// </summary>
    public LoginUserCommandValidatorTests()
    {
        this._validator = new LoginUserCommandValidator();
    }

    /// <summary>
    /// Verifies that valid data passes validation without errors.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new LoginUserCommand("test@example.com", "Password123!");

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
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
        var command = new LoginUserCommand(email!, "Password123!");

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email cannot be null or empty.");
    }

    /// <summary>
    /// Verifies that an incorrect email format triggers a validation error.
    /// </summary>
    /// <param name="email">The email value to validate (null or empty string).</param>
    [Theory]
    [InlineData("plainaddress")]
    [InlineData("#@%^%#$@#$@#.com")]
    [InlineData("@example.com")]
    [InlineData("Joe Smith <email@example.com>")]
    public void Should_Have_Error_When_Email_Format_Is_Incorrect(string email)
    {
        // Arrange
        var command = new LoginUserCommand(email, "Password123!");

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email address is incorrect.");
    }

    /// <summary>
    /// Verifies that an empty password triggers a validation error.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_Password_Is_Empty()
    {
        // Arrange
        var command = new LoginUserCommand("test@example.com", string.Empty);

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password is required.");
    }
}
