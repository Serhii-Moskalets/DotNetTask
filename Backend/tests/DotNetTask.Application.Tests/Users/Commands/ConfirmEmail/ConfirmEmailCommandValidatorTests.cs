using DotNetTask.Application.Users.Commands.ConfirmEmail;
using DotNetTask.Domain.Constants;

using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.Users.Commands.ConfirmEmail;

/// <summary>
/// Contains unit tests for the <see cref="ConfirmEmailCommandValidator"/> class.
/// </summary>
public class ConfirmEmailCommandValidatorTests
{
    private readonly ConfirmEmailCommandValidator _validator = new();

    /// <summary>
    /// Verifies that valid data passes validation without errors.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        ConfirmEmailCommand command = new ConfirmEmailCommand("secure-verification-token");

        // Act
        TestValidationResult<ConfirmEmailCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
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
        ConfirmEmailCommand command = new ConfirmEmailCommand(token!);

        // Act
        TestValidationResult<ConfirmEmailCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Token)
              .WithErrorMessage(TokenPolicy.RequiredMessage);
    }
}
