using DotNetTask.Application.UserTaskAccess.Queries.GetSharedTasksByUserId;
using DotNetTask.Domain.Constants;

using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.UserTaskAccess.Queries.GetSharedTasksByUserId;

/// <summary>
/// Tests for <see cref="GetSharedTasksByUserIdQueryValidator"/>.
/// </summary>
public class GetSharedTasksByUserIdQueryValidatorTests
{
    private readonly GetSharedTasksByUserIdQueryValidator _validator = new();

    /// <summary>
    /// Ensures validation fails when the user identifier is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        GetSharedTasksByUserIdQuery query = new(Guid.Empty);

        // Act
        TestValidationResult<GetSharedTasksByUserIdQuery> result = this._validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Ensures no validation errors are returned when the user identifier is valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_UserId_Is_Valid()
    {
        // Arrange
        GetSharedTasksByUserIdQuery query = new(Guid.NewGuid());

        // Act
        TestValidationResult<GetSharedTasksByUserIdQuery> result = this._validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
