using DotNetTask.Application.Users.Queries.GetUserProfile;
using DotNetTask.Domain.Constants;

using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.Users.Queries.GetUserProfile;

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
        GetUserProfileQuery command = new(Guid.NewGuid());

        // Act
        TestValidationResult<GetUserProfileQuery> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Verifies that the validator produces an error when the <see cref="GetUserProfileQuery.UserId"/> is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_IsEmpty()
    {
        // Arrange
        GetUserProfileQuery command = new(Guid.Empty);

        // Act
        TestValidationResult<GetUserProfileQuery> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }
}
