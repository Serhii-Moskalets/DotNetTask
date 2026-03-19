using FluentValidation.TestHelper;
using TodoListApp.Application.UserTaskAccess.Queries.GetSharedTasksByUserId;
using TodoListApp.Domain.Constants;

namespace TodoListApp.Application.Tests.UserTaskAccess.Queries;

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
        var query = new GetSharedTasksByUserIdQuery(Guid.Empty);
        var result = this._validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Ensures no validation errors are returned when the user identifier is valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_UserId_Is_Valid()
    {
        var query = new GetSharedTasksByUserIdQuery(Guid.NewGuid());
        var result = this._validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
