using FluentValidation.TestHelper;
using TodoListApp.Application.Users.Commands.ResendEmailVerification;
using TodoListApp.Domain.Constants;

namespace TodoListApp.Application.Tests.Users.Commands.ResendEmailVerification;

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
        var command = new ResendEmailVerificationCommand(Guid.NewGuid());

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Verifies that the validator returns an error when the User ID is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        var command = new ResendEmailVerificationCommand(Guid.Empty);

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }
}
