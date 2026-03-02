using FluentValidation.TestHelper;
using TodoListApp.Application.Users.Commands.RevertEmailChange;

namespace TodoListApp.Application.Tests.Users.Commands.RevertEmail;

/// <summary>
/// Unit tests for the <see cref="RevertEmailChangeCommandValidator"/> class.
/// </summary>
public class RevertEmailChangeCommandValidatorTests
{
    private readonly RevertEmailChangeCommandValidator _validator = new();

    /// <summary>
    /// Verifies that the validator passes without any errors when all command properties
    /// are populated with valid data.
    /// </summary>
    [Fact]
    public void Should_NotHaveError_When_CommandIsValid()
    {
        // Arrange
        var command = new RevertEmailChangeCommand("valid-token");

        // Act
        var result = this._validator.TestValidate(command);

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
    public void Should_HaveError_When_TokenIsNullOrEmpty(string? token)
    {
        // Arrange
        var command = new RevertEmailChangeCommand(token!);

        // Act
        var result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Token)
              .WithErrorMessage("Token is required.");
    }
}
