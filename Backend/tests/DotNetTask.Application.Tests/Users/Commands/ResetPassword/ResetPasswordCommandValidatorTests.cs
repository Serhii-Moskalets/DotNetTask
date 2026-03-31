using DotNetTask.Application.Users.Commands.ResetPassword;
using DotNetTask.Domain.Constants;

using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.Users.Commands.ResetPassword;

/// <summary>
/// Contains unit tests for the <see cref="ResetPasswordCommandValidator"/> class.
/// </summary>
public class ResetPasswordCommandValidatorTests
{
    private const string IpAddress = "192.168.0.1";
    private const string Email = "test@example.com";
    private readonly ResetPasswordCommandValidator _validator = new();

    /// <summary>
    /// Verifies that the validator does not have any errors when the email is valid.
    /// </summary>
    [Fact]
    public void Should_NotHaveError_When_EmailIsValid()
    {
        // Arrange
        ResetPasswordCommand command = new(Email, IpAddress);

        // Act
        TestValidationResult<ResetPasswordCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    /// <summary>
    /// Verifies that validation fails when the email is empty or null.
    /// </summary>
    /// <param name="email">The invalid email value.</param>
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Should_HaveError_When_EmailIsEmpty(string? email)
    {
        // Arrange
        ResetPasswordCommand command = new(email!, IpAddress);

        // Act
        TestValidationResult<ResetPasswordCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage(EmailPolicy.EmptyMessage);
    }

    /// <summary>
    /// Verifies that validation fails when the email format is incorrect.
    /// </summary>
    /// <param name="invalidEmail">The malformed email address.</param>
    [Theory]
    [InlineData("plainaddress")]
    [InlineData("#@%^%#$@#$@#.com")]
    [InlineData("@example.com")]
    [InlineData("email.example.com")]
    public void Should_HaveError_When_EmailFormatIsInvalid(string invalidEmail)
    {
        // Arrange
        ResetPasswordCommand command = new(invalidEmail, IpAddress);

        // Act
        TestValidationResult<ResetPasswordCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage(EmailPolicy.InvalidFormatMessage);
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
        ResetPasswordCommand command = new(Email, invalidIp);

        // Act
        TestValidationResult<ResetPasswordCommand> result = this._validator.TestValidate(command);

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
        ResetPasswordCommand command = new(Email, string.Empty);

        // Act
        TestValidationResult<ResetPasswordCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.IpAddress)
              .WithErrorMessage(CommonPolicy.InvalidIpAddressMessage);
    }
}
