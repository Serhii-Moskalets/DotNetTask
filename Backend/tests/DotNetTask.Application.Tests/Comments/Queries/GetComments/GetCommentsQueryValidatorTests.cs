using DotNetTask.Application.Comment.Queries.GetComments;
using DotNetTask.Domain.Constants;

using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.Comment.Queries.GetComments;

/// <summary>
/// Unit tests for <see cref="GetCommentsQueryValidator"/>.
/// Ensures that the validator correctly enforces rules for retrieving comments.
/// </summary>
public class GetCommentsQueryValidatorTests
{
    private readonly GetCommentsQueryValidator _validator = new();

    /// <summary>
    /// Fails validation when UserId is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        GetCommentsQuery query = new(Guid.NewGuid(), Guid.Empty);

        // Act
        TestValidationResult<GetCommentsQuery> result = this._validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(q => q.UserId)
              .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Fails validation when TaskId is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_TaskId_Is_Empty()
    {
        // Arrange
        GetCommentsQuery query = new(Guid.Empty, Guid.NewGuid());

        // Act
        TestValidationResult<GetCommentsQuery> result = this._validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(q => q.TaskId)
              .WithErrorMessage(TaskPolicy.IdRequiredMessage);
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
        // Arrange
        GetCommentsQuery query = new(Guid.NewGuid(), Guid.NewGuid(), page, 10);

        // Act
        TestValidationResult<GetCommentsQuery> result = this._validator.TestValidate(query);

        // Assert
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
        // Arrange
        GetCommentsQuery query = new(Guid.NewGuid(), Guid.NewGuid(), 1, pageSize);

        // Act
        TestValidationResult<GetCommentsQuery> result = this._validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage(CommonPolicy.PageSizeRangeMessage);
    }

    /// <summary>
    /// Passes validation when the query is valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_Query_Is_Valid()
    {
        // Arrange
        GetCommentsQuery query = new(Guid.NewGuid(), Guid.NewGuid());

        // Act
        TestValidationResult<GetCommentsQuery> result = this._validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
