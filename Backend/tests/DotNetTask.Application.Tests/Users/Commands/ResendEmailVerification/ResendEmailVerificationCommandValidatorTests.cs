using DotNetTask.Application.Users.Commands.ResendEmailVerification;
using DotNetTask.Domain.Constants;

using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.Users.Commands.ResendEmailVerification;

/// <summary>
/// Contains unit tests for the <see cref="ResendEmailVerificationCommandValidator"/> class.
/// </summary>
public class ResendEmailVerificationCommandValidatorTests
{
    private const string IpAddress = "192.168.0.1";
    private static readonly Guid UserId = Guid.NewGuid();
    private readonly ResendEmailVerificationCommandValidator _validator = new();

    /// <summary>
    /// Verifies that the validator does not return any errors when user id is valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrage
        ResendEmailVerificationCommand command = new(UserId, IpAddress);

        // Act
        TestValidationResult<ResendEmailVerificationCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Verifies that the validator returns an error when the User ID is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        ResendEmailVerificationCommand command = new(Guid.Empty, IpAddress);

        // Act
        TestValidationResult<ResendEmailVerificationCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(UserPolicy.IdRequiredMessage);
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
        ResendEmailVerificationCommand command = new(UserId, invalidIp);

        // Act
        TestValidationResult<ResendEmailVerificationCommand> result = this._validator.TestValidate(command);

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
        ResendEmailVerificationCommand command = new(UserId, string.Empty);

        // Act
        TestValidationResult<ResendEmailVerificationCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.IpAddress)
              .WithErrorMessage(CommonPolicy.InvalidIpAddressMessage);
    }
}
