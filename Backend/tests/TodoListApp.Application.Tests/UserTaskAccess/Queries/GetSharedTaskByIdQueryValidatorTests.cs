using FluentValidation.TestHelper;
using TodoListApp.Application.UserTaskAccess.Queries.GetSharedTaskById;
using TodoListApp.Domain.Constants;

namespace TodoListApp.Application.Tests.UserTaskAccess.Queries;

/// <summary>
/// Tests for <see cref="GetSharedTaskByIdQueryValidator"/>.
/// </summary>
public class GetSharedTaskByIdQueryValidatorTests
{
    private readonly GetSharedTaskByIdQueryValidator _validator = new();

    /// <summary>
    /// Ensures validation fails when the task identifier is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_TaskId_Is_Empty()
    {
        var query = new GetSharedTaskByIdQuery(Guid.Empty, Guid.NewGuid());
        var result = this._validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.TaskId)
              .WithErrorMessage(TaskPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Ensures validation fails when the user identifier is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        var query = new GetSharedTaskByIdQuery(Guid.NewGuid(), Guid.Empty);
        var result = this._validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Ensures no validation errors are returned when all query fields are valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        var query = new GetSharedTaskByIdQuery(Guid.NewGuid(), Guid.NewGuid());
        var result = this._validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
