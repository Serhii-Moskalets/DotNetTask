using DotNetTask.Application.Users.Commands.RecoverAccount;
using DotNetTask.Domain.Constants;
using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.Users.Commands.RecoverAccount;

/// <summary>
/// Contains unit tests for the <see cref="RecoverAccountCommandValidator"/> class.
/// </summary>
public class RecoverAccountCommandValidatorTests
{
    private readonly RecoverAccountCommandValidator _validator = new();

    /// <summary>
    /// Verifies that the validator does not return any errors when user id is valid.
    /// </summary>
    [Fact]
    public void Hould_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        RecoverAccountCommand command = new(Guid.NewGuid());

        // Act
        TestValidationResult<RecoverAccountCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Verifies that an empty User ID triggers the appropriate validation error.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        RecoverAccountCommand command = new(Guid.Empty);

        // Act
        TestValidationResult<RecoverAccountCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }

}
