using DotNetTask.Application.Users.Commands.UpdateUsername;
using DotNetTask.Domain.Constants;

using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.Users.Commands.UpdateUsername;

/// <summary>
/// Contains unit tests for the <see cref="UpdateUsernameCommandValidator"/> class.
/// </summary>
public class UpdateUsernameCommandValidatorTests
{
    private readonly UpdateUsernameCommandValidator _validator = new();

    /// <summary>
    /// Verifies that the validator does not return any errors when all command properties
    /// are valid and meet the length requirements.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        UpdateUsernameCommand command = new UpdateUsernameCommand("ValidUsername", Guid.NewGuid());

        // Act & Assert
        TestValidationResult<UpdateUsernameCommand> result = this._validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Verifies that the validator returns an error when the username is null or empty.
    /// </summary>
    /// <param name="username">The empty or null username string.</param>
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Have_Error_When_Username_Is_Empty(string? username)
    {
        // Arrange
        UpdateUsernameCommand command = new UpdateUsernameCommand(username!, Guid.NewGuid());

        // Act & Assert
        TestValidationResult<UpdateUsernameCommand> result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserName)
            .WithErrorMessage(UserNamePolicy.EmptyMessage);
    }

    /// <summary>
    /// Verifies that the validator returns an error when the username length
    /// is outside the allowed range (3 to 20 characters).
    /// </summary>
    /// <param name="username">The invalid username to validate.</param>
    [Theory]
    [InlineData("ab")]
    [InlineData("thisusernameiswaytoolongforoursystem")]
    public void Should_Have_Error_When_Username_Length_Is_Invalid(string username)
    {
        // Arrange
        UpdateUsernameCommand command = new UpdateUsernameCommand(username, Guid.NewGuid());

        // Act & Assert
        TestValidationResult<UpdateUsernameCommand> result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserName)
              .WithErrorMessage(UserNamePolicy.LengthMessage);
    }

    /// <summary>
    /// Verifies that the validator returns an error when the User ID is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        UpdateUsernameCommand command = new UpdateUsernameCommand("ValidUser", Guid.Empty);

        // Act & Assert
        TestValidationResult<UpdateUsernameCommand> result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }
}
