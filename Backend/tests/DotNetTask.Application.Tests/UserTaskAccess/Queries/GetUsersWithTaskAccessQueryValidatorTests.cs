using DotNetTask.Application.UserTaskAccess.Queries.GetUsersWithTaskAccess;
using DotNetTask.Domain.Constants;

using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.UserTaskAccess.Queries;

/// <summary>
/// Tests for <see cref="GetUsersWithTaskAccessQueryHandlerTests"/>.
/// </summary>
public class GetUsersWithTaskAccessQueryValidatorTests
{
    private readonly GetUsersWithTaskAccessQueryValidator _validator = new();

    /// <summary>
    /// Ensures validation fails when the task identifier is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_TaskId_Is_Empty()
    {
        GetUsersWithTaskAccessQuery query = new GetUsersWithTaskAccessQuery(Guid.Empty, Guid.NewGuid());
        TestValidationResult<GetUsersWithTaskAccessQuery> result = this._validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.TaskId)
              .WithErrorMessage(TaskPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Ensures validation fails when the user identifier is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        GetUsersWithTaskAccessQuery query = new GetUsersWithTaskAccessQuery(Guid.NewGuid(), Guid.Empty);
        TestValidationResult<GetUsersWithTaskAccessQuery> result = this._validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Ensures no validation errors are returned when all query fields are valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        GetUsersWithTaskAccessQuery query = new GetUsersWithTaskAccessQuery(Guid.NewGuid(), Guid.NewGuid());
        TestValidationResult<GetUsersWithTaskAccessQuery> result = this._validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
