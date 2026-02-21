using FluentValidation.TestHelper;
using TodoListApp.Application.Users.Commands.ConfirmChangeEmail;

namespace TodoListApp.Application.Tests.Users.Commands.ConfirmChangeEmail;

/// <summary>
/// Unit tests for the <see cref="ConfirmChangeEmailCommandValidator"/> class.
/// </summary>
public class ConfirmChangeEmailValidatorTests
{
    private readonly ConfirmChangeEmailCommandValidator _validator = new();

    /// <summary>
    /// Verifies that valid data passes validation without errors.
    /// </summary>
    [Fact]
    public void Should_HaveError_When_UserIdIsEmpty()
    {
        // Arrange
        var command = new ConfirmChangeEmailCommand(Guid.Empty, "valid-token");

        // Act
        var result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage("User ID is required.");
    }

    /// <summary>
    /// Verifies that an empty user id triggers a validation error.
    /// </summary>
    [Fact]
    public void Should_HaveError_When_TokenIsEmpty()
    {
        // Arrange
        var command = new ConfirmChangeEmailCommand(Guid.NewGuid(), string.Empty);

        // Act
        var result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Token)
              .WithErrorMessage("Token is required.");
    }

    /// <summary>
    /// Verifies that an empty or null token triggers a validation error.
    /// </summary>
    /// <param name="token">The token value to validate (null or empty string).</param>
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Have_Error_When_Token_Is_Empty(string? token)
    {
        // Arrange
        var command = new ConfirmChangeEmailCommand(Guid.NewGuid(), token!);

        // Act
        var result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Token)
              .WithErrorMessage("Token is required.");
    }
}
