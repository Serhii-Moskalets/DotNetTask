using DotNetTask.Application.Users.Commands.ResendEmailVerification;
using DotNetTask.Domain.Constants;

using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.Users.Commands.ResendEmailVerification;

/// <summary>
/// Contains unit tests for the <see cref="ResendEmailVerificationCommandValidator"/> class.
/// </summary>
public class ResendEmailVerificationCommandValidatorTests
{
    private readonly ResendEmailVerificationCommandValidator _validator = new();

    /// <summary>
    /// Verifies that the validator does not return any errors when user id is valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrage
        ResendEmailVerificationCommand command = new ResendEmailVerificationCommand(Guid.NewGuid());

        // Act & Assert
        TestValidationResult<ResendEmailVerificationCommand> result = this._validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Verifies that the validator returns an error when the User ID is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        ResendEmailVerificationCommand command = new ResendEmailVerificationCommand(Guid.Empty);

        // Act & Assert
        TestValidationResult<ResendEmailVerificationCommand> result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }
}
