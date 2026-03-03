using FluentValidation.TestHelper;
using TinyResult.Enums;
using TodoListApp.Application.Users.Commands.RegisterUser;
using TodoListApp.Application.Users.Commands.UpdateUsername;
using TodoListApp.Application.Users.Commands.UpdateUserProfile;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tests.Users.Commands.UpdateUserProfile;

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
        var command = new UpdateUserProfileCommand("John", "Johnson", Guid.NewGuid());

        // Act & Assert
        var result = this._validator.TestValidate(command);
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
        var command = new UpdateUserProfileCommand(firstName, lastName, Guid.NewGuid());

        // Act
        var result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("At least one field (FirstName or LastName) must be provided.")
            .WithErrorCode(nameof(ErrorCode.ValidationError));
    }

    /// <summary>
    /// Verifies that validation errors are triggered when mandatory fields exceed their maximum allowed length.
    /// </summary>
    /// <param name="firstName">The first name to validate (Max: 20).</param>
    /// <param name="lastName">The last name to validate (Max: 30).</param>
    [Theory]
    [InlineData("thisfirstnameiswaytoolong", "ValidLastName")]
    [InlineData("ValidFirstName", "thislastnameiswaytoolongforoursystem")]
    [InlineData("thisfirstnameiswaytoolong", "thislastnameiswaytoolongforoursystem")]
    public void Should_Have_Errors_When_First_And_Last_Name_Are_Too_Long(string firstName, string lastName)
    {
        // Arrange
        var command = new UpdateUserProfileCommand(firstName, lastName, Guid.NewGuid());

        // Act
        var result = this._validator.TestValidate(command);

        // Assert
        if (firstName.Length > 20)
        {
            result.ShouldHaveValidationErrorFor(x => x.FirstName)
                .WithErrorMessage("First name cannot be longer than 20 characters.");
        }

        if (lastName.Length > 30)
        {
            result.ShouldHaveValidationErrorFor(x => x.LastName)
                .WithErrorMessage("Last name cannot be longer than 30 characters.");
        }
    }

    /// <summary>
    /// Verifies that the validator returns an error when the User ID is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        var command = new UpdateUserProfileCommand("FirstName", "LastName", Guid.Empty);

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage("User ID is required.");
    }
}
