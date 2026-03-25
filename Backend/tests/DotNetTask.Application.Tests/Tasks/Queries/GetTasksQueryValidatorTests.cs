using DotNetTask.Application.Tasks.Queries.GetTasks;
using DotNetTask.Domain.Constants;

using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.Tasks.Queries;

/// <summary>
/// Unit tests for <see cref="GetTasksQueryValidator"/>.
/// Verifies validation rules for task list filtering queries.
/// </summary>
public class GetTasksQueryValidatorTests
{
    private readonly GetTasksQueryValidator _validator = new();

    /// <summary>
    /// Returns a validation error when UserId is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        GetTasksQuery query = new GetTasksQuery(
            UserId: Guid.Empty,
            TaskListId: Guid.NewGuid());

        TestValidationResult<GetTasksQuery> result = this._validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Does not return a validation error when UserId is valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_UserId_Is_Valid()
    {
        GetTasksQuery query = new GetTasksQuery(
            UserId: Guid.NewGuid(),
            TaskListId: Guid.NewGuid());

        TestValidationResult<GetTasksQuery> result = this._validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }

    /// <summary>
    /// Returns a validation error when TaskListId is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_TaskListId_Is_Empty()
    {
        GetTasksQuery query = new GetTasksQuery(
            UserId: Guid.NewGuid(),
            TaskListId: Guid.Empty);

        TestValidationResult<GetTasksQuery> result = this._validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.TaskListId)
              .WithErrorMessage(TaskListPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Does not return a validation error when TaskListId is valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_TaskListId_Is_Valid()
    {
        GetTasksQuery query = new GetTasksQuery(
            UserId: Guid.NewGuid(),
            TaskListId: Guid.NewGuid());

        TestValidationResult<GetTasksQuery> result = this._validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(x => x.TaskListId);
    }

    /// <summary>
    /// Allows both DueBefore and DueAfter to be null.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_DueDates_Are_Null()
    {
        GetTasksQuery query = new GetTasksQuery(
            UserId: Guid.NewGuid(),
            TaskListId: Guid.NewGuid(),
            DueBefore: null,
            DueAfter: null);

        TestValidationResult<GetTasksQuery> result = this._validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Allows DueAfter to be set when DueBefore is null.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_Only_DueAfter_Is_Set()
    {
        GetTasksQuery query = new GetTasksQuery(
            UserId: Guid.NewGuid(),
            TaskListId: Guid.NewGuid(),
            DueBefore: null,
            DueAfter: DateTime.UtcNow);

        TestValidationResult<GetTasksQuery> result = this._validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Allows DueBefore to be set when DueAfter is null.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_Only_DueBefore_Is_Set()
    {
        GetTasksQuery query = new GetTasksQuery(
            UserId: Guid.NewGuid(),
            TaskListId: Guid.NewGuid(),
            DueBefore: DateTime.UtcNow,
            DueAfter: null);

        TestValidationResult<GetTasksQuery> result = this._validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Allows DueAfter to be equal to DueBefore.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_DueAfter_Equals_DueBefore()
    {
        DateTime date = DateTime.UtcNow;

        GetTasksQuery query = new GetTasksQuery(
            UserId: Guid.NewGuid(),
            TaskListId: Guid.NewGuid(),
            DueBefore: date,
            DueAfter: date);

        TestValidationResult<GetTasksQuery> result = this._validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Returns a validation error when DueAfter is greater than DueBefore.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_DueAfter_Is_Greater_Than_DueBefore()
    {
        GetTasksQuery query = new GetTasksQuery(
            UserId: Guid.NewGuid(),
            TaskListId: Guid.NewGuid(),
            DueBefore: DateTime.UtcNow,
            DueAfter: DateTime.UtcNow.AddDays(1));

        TestValidationResult<GetTasksQuery> result = this._validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.DueAfter)
              .WithErrorMessage(TaskPolicy.InvalidDateRangeMessage);
    }

    /// <summary>
    /// Returns a validation error when Page is less than 1.
    /// </summary>
    /// <param name="page">The invalid page number to validate.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_Have_Error_When_Page_Is_Invalid(int page)
    {
        GetTasksQuery query = new GetTasksQuery(Guid.NewGuid(), Guid.NewGuid(), page, 10);

        TestValidationResult<GetTasksQuery> result = this._validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Page)
              .WithErrorMessage(CommonPolicy.PageMinMessage);
    }

    /// <summary>
    /// Returns a validation error when PageSize is out of range.
    /// </summary>
    /// <param name="pageSize">The invalid page size to validate.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Should_Have_Error_When_PageSize_Is_Invalid(int pageSize)
    {
        GetTasksQuery query = new GetTasksQuery(Guid.NewGuid(), Guid.NewGuid(), 1, pageSize);

        TestValidationResult<GetTasksQuery> result = this._validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage(CommonPolicy.PageSizeRangeMessage);
    }
}
