using FluentValidation.TestHelper;
using TodoListApp.Application.Users.Queries.GetUserProfile;
using TodoListApp.Domain.Constants;

namespace TodoListApp.Application.Tests.Users.Queries.GetUserProfile;

/// <summary>
/// Contains unit tests for <see cref="GetUserProfileQueryValidator"/> to verify
/// that the validator behaves correctly for valid and invalid queries.
/// </summary>
public class GetUserProfileQueryValidatorTests
{
    private readonly GetUserProfileQueryValidator _validator = new();

    /// <summary>
    /// Verifies that the validator does not produce any errors for a valid query.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_Query_IsValid()
    {
        // Arrange
        var command = new GetUserProfileQuery(Guid.NewGuid());

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Verifies that the validator produces an error when the <see cref="GetUserProfileQuery.UserId"/> is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_IsEmpty()
    {
        // Arrange
        var command = new GetUserProfileQuery(Guid.Empty);

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage(UserPolicy.UserIdRequiredMessage);
    }
}
