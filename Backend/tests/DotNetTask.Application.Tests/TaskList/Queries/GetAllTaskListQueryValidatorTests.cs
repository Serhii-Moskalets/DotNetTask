using DotNetTask.Application.TaskList.Queries.GetTaskLists;
using DotNetTask.Domain.Constants;

using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.TaskList.Queries;

/// <summary>
/// Unit tests for <see cref="GetTaskListsQueryValidator"/>.
/// Ensures that the validator correctly enforces rules for retrieving all task lists.
/// </summary>
public class GetAllTaskListQueryValidatorTests
{
    private readonly GetTaskListsQueryValidator _validator = new();

    /// <summary>
    /// Validation fails when <see cref="GetTaskListsQuery.UserId"/> is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        GetTaskListsQuery query = new(Guid.Empty);

        // Act & Assert
        TestValidationResult<GetTaskListsQuery> result = this._validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(q => q.UserId)
              .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Validation succeeds when <see cref="GetTaskListsQuery.UserId"/> is provided.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_UserId_Is_Provided()
    {
        // Arrange
        GetTaskListsQuery query = new(Guid.NewGuid());

        // Act & Assert
        TestValidationResult<GetTaskListsQuery> result = this._validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(q => q.UserId);
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
        GetTaskListsQuery query = new(Guid.NewGuid(), page, 10);

        TestValidationResult<GetTaskListsQuery> result = this._validator.TestValidate(query);

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
        GetTaskListsQuery query = new(Guid.NewGuid(), 1, pageSize);

        TestValidationResult<GetTaskListsQuery> result = this._validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage(CommonPolicy.PageSizeRangeMessage);
    }

    /// <summary>
    /// Verifies that <see cref="GetTaskListsQueryValidator"/> succeeds when
    /// valid pagination parameters and User ID are provided.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_Pagination_Is_Valid()
    {
        // Arrange
        GetTaskListsQuery query = new(Guid.NewGuid(), Page: 1, PageSize: 50);

        // Act
        TestValidationResult<GetTaskListsQuery> result = this._validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
