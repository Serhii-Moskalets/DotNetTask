using FluentValidation.TestHelper;
using TodoListApp.Application.Tag.Queries.GetTags;
using TodoListApp.Domain.Constants;

namespace TodoListApp.Application.Tests.Tag.Queries;

/// <summary>
/// Contains unit tests for <see cref="GetTagsQueryValidator"/>.
/// Ensures that the validator correctly enforces rules for retrieving all tags.
/// </summary>
public class GetTagsQueryValidatorTests
{
    private readonly GetTagsQueryValidator _validator = new();

    /// <summary>
    /// Should return a validation error when the UserId is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        var query = new GetTagsQuery(Guid.Empty);

        // Act
        var result = this._validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(q => q.UserId)
              .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Should not return any validation errors when the UserId is valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_UserId_Is_Valid()
    {
        // Arrange
        var query = new GetTagsQuery(Guid.NewGuid());

        // Act
        var result = this._validator.TestValidate(query);

        // Assert
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
        var query = new GetTagsQuery(Guid.NewGuid(), page, 10);

        var result = this._validator.TestValidate(query);

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
        var query = new GetTagsQuery(Guid.NewGuid(), 1, pageSize);

        var result = this._validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage(CommonPolicy.PageSizeRangeMessage);
    }

    /// <summary>
    /// Verifies that <see cref="GetTagsQueryValidator"/> succeeds when
    /// valid pagination parameters and User ID are provided.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_Pagination_Is_Valid()
    {
        // Arrange
        var query = new GetTagsQuery(Guid.NewGuid(), Page: 1, PageSize: 50);

        // Act
        var result = this._validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
