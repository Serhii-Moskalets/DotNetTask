using DotNetTask.Application.UserTaskAccess.Queries.GetSharedTaskById;
using DotNetTask.Domain.Constants;

using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.UserTaskAccess.Queries;

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
        GetSharedTaskByIdQuery query = new(Guid.Empty, Guid.NewGuid());
        TestValidationResult<GetSharedTaskByIdQuery> result = this._validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.TaskId)
              .WithErrorMessage(TaskPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Ensures validation fails when the user identifier is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        GetSharedTaskByIdQuery query = new(Guid.NewGuid(), Guid.Empty);
        TestValidationResult<GetSharedTaskByIdQuery> result = this._validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Ensures no validation errors are returned when all query fields are valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        GetSharedTaskByIdQuery query = new(Guid.NewGuid(), Guid.NewGuid());
        TestValidationResult<GetSharedTaskByIdQuery> result = this._validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
