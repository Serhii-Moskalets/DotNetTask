using DotNetTask.Application.Users.Commands.ConfirmEmailChange;
using DotNetTask.Domain.Constants;

using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.Users.Commands.ConfirmChangeEmail;

/// <summary>
/// Unit tests for the <see cref="ConfirmEmailChangeCommandValidator"/> class.
/// </summary>
public class ConfirmChangeEmailValidatorTests
{
    private const string IpAddress = "192.168.0.1";
    private readonly ConfirmEmailChangeCommandValidator _validator = new();

    /// <summary>
    /// Verifies that an empty user id triggers a validation error.
    /// </summary>
    [Fact]
    public void Should_HaveError_When_TokenIsEmpty()
    {
        // Arrange
        ConfirmEmailChangeCommand command = new(string.Empty, IpAddress);

        // Act
        TestValidationResult<ConfirmEmailChangeCommand> result = this._validator.TestValidate(command);

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
        ConfirmEmailChangeCommand command = new(token!, IpAddress);

        // Act
        TestValidationResult<ConfirmEmailChangeCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Token)
              .WithErrorMessage(TokenPolicy.RequiredMessage);
    }

    /// <summary>
    /// Verifies that an invalid IP address format triggers a validation error.
    /// </summary>
    /// <param name="invalidIp">The malformed IP address string.</param>
    [Theory]
    [InlineData("not-an-ip")]
    [InlineData("256.256.256.256")]
    [InlineData("192.168.1")]
    [InlineData("...")]
    public void Should_Have_Error_When_IpAddress_Is_Invalid(string invalidIp)
    {
        // Arrange
        ConfirmEmailChangeCommand command = new("valid-token", invalidIp);

        // Act
        TestValidationResult<ConfirmEmailChangeCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.IpAddress)
              .WithErrorMessage(CommonPolicy.InvalidIpAddressMessage);
    }

    /// <summary>
    /// Verifies that an empty IP address triggers a validation error.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_IpAddress_Is_Empty()
    {
        // Arrange
        ConfirmEmailChangeCommand command = new("valid-token", string.Empty);

        // Act
        TestValidationResult<ConfirmEmailChangeCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.IpAddress)
              .WithErrorMessage(CommonPolicy.InvalidIpAddressMessage);
    }
}
