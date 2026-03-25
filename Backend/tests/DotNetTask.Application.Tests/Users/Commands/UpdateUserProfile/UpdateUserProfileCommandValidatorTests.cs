using DotNetTask.Application.Users.Commands.UpdateUserProfile;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.ValueObjects;

using FluentValidation.TestHelper;

using TinyResult.Enums;

namespace DotNetTask.Application.Tests.Users.Commands.UpdateUserProfile;

/// <summary>
/// Contains unit tests for the <see cref="UpdateUserProfileCommandValidator"/> class.
/// </summary>
public class UpdateUserProfileCommandValidatorTests
{
    private readonly UpdateUserProfileCommandValidator _validator = new();

    /// <summary>
    /// Verifies that the validator does not return any errors when all command properties
    /// are valid and meet the length requirements.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        UpdateUserProfileCommand command = new UpdateUserProfileCommand("John", "Johnson", Guid.NewGuid());

        // Act & Assert
        TestValidationResult<UpdateUserProfileCommand> result = this._validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Verifies that the validator returns an error when the first and last name is null or empty.
    /// </summary>
    /// <param name="firstName">The empty or null first name string.</param>
    /// <param name="lastName">The empty or null last name string.</param>
    [Theory]
    [InlineData("", "")]
    [InlineData(null, null)]
    public void Should_Have_Error_When_Names_Is_Empty(string? firstName, string? lastName)
    {
        // Arrange
        UpdateUserProfileCommand command = new UpdateUserProfileCommand(firstName, lastName, Guid.NewGuid());

        // Act
        TestValidationResult<UpdateUserProfileCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage(UserPolicy.AtLeastOneFieldRequiredMessage)
            .WithErrorCode(nameof(ErrorCode.ValidationError));
    }

    /// <summary>
    /// Verifies that validation errors are triggered when mandatory fields exceed their maximum allowed length.
    /// </summary>
    [Fact]
    public void Should_Have_Errors_When_Names_Exceed_Maximum_Length()
    {
        // Arrange
        string longFirstName = new string('a', FirstName.MaxLength + 1);
        string longLastName = new string('a', LastName.MaxLength + 1);
        UpdateUserProfileCommand command = new UpdateUserProfileCommand(longFirstName, longLastName, Guid.NewGuid());

        // Act
        TestValidationResult<UpdateUserProfileCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName).WithErrorMessage(FirstNamePolicy.TooLongMessage);
        result.ShouldHaveValidationErrorFor(x => x.LastName).WithErrorMessage(LastNamePolicy.TooLongMessage);
    }

    /// <summary>
    /// Verifies that the validator returns an error when the User ID is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        UpdateUserProfileCommand command = new UpdateUserProfileCommand("FirstName", "LastName", Guid.Empty);

        // Act & Assert
        TestValidationResult<UpdateUserProfileCommand> result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }
}
