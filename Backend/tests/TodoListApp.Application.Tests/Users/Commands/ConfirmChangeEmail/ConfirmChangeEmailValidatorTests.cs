using FluentValidation.TestHelper;
using TodoListApp.Application.Users.Commands.ConfirmEmailChange;
using TodoListApp.Domain.Constants;

namespace TodoListApp.Application.Tests.Users.Commands.ConfirmChangeEmail;

/// <summary>
/// Unit tests for the <see cref="ConfirmEmailChangeCommandValidator"/> class.
/// </summary>
public class ConfirmChangeEmailValidatorTests
{
    private readonly ConfirmEmailChangeCommandValidator _validator = new();

    /// <summary>
    /// Verifies that an empty user id triggers a validation error.
    /// </summary>
    [Fact]
    public void Should_HaveError_When_TokenIsEmpty()
    {
        // Arrange
        var command = new ConfirmEmailChangeCommand(string.Empty);

        // Act
        var result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Token)
              .WithErrorMessage(TokenPolicy.RequiredMessage);
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
        var command = new ConfirmEmailChangeCommand(token!);

        // Act
        var result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Token)
              .WithErrorMessage(TokenPolicy.RequiredMessage);
    }
}
