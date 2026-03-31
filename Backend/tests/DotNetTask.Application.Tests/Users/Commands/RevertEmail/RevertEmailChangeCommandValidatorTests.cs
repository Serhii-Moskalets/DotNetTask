using DotNetTask.Application.Users.Commands.RevertEmailChange;
using DotNetTask.Domain.Constants;

using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.Users.Commands.RevertEmail;

/// <summary>
/// Unit tests for the <see cref="RevertEmailChangeCommandValidator"/> class.
/// </summary>
public class RevertEmailChangeCommandValidatorTests
{
    private const string IpAddress = "192.168.0.1";
    private readonly RevertEmailChangeCommandValidator _validator = new();

    /// <summary>
    /// Verifies that the validator passes without any errors when all command properties
    /// are populated with valid data.
    /// </summary>
    [Fact]
    public void Should_NotHaveError_When_CommandIsValid()
    {
        // Arrange
        RevertEmailChangeCommand command = new("valid-token", IpAddress);

        // Act
        TestValidationResult<RevertEmailChangeCommand> result = this._validator.TestValidate(command);

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
        RevertEmailChangeCommand command = new(token!, IpAddress);

        // Act
        TestValidationResult<RevertEmailChangeCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Token)
              .WithErrorMessage(TokenPolicy.RequiredMessage);
    }
}
