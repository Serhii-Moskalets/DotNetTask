using DotNetTask.Application.Users.Commands.InitiateAccountDeletion;
using DotNetTask.Domain.Constants;
using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.Users.Commands.InitiateAccountDeletion;

/// <summary>
/// Contains unit tests for the <see cref="InitiateAccountDeletionCommandValidator"/> class.
/// </summary>
public class InitiateAccountDeletionCommandValidatorTests
{
    private readonly InitiateAccountDeletionCommandValidator _validator = new();

    /// <summary>
    /// Verifies that the validator does not return any errors when user id is valid.
    /// </summary>
    [Fact]
    public void Hould_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        InitiateAccountDeletionCommand command = new(Guid.NewGuid());

        // Act
        TestValidationResult<InitiateAccountDeletionCommand> result = this._validator.TestValidate(command);

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
        InitiateAccountDeletionCommand command = new(Guid.Empty);

        // Act
        TestValidationResult<InitiateAccountDeletionCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }
}
