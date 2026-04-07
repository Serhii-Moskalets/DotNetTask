using DotNetTask.Application.Tasks.Queries.GetTaskByTitle;
using DotNetTask.Domain.Constants;

using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.Tasks.Queries;

/// <summary>
/// Unit tests for <see cref="GetTaskByTitleQueryValidator"/>.
/// Verifies validation rules for searching tasks by title.
/// </summary>
public class GetTaskByTitleQueryValidatorTests
{
    private readonly GetTaskByTitleQueryValidator _validator = new();

    /// <summary>
    /// Returns a validation error when UserId is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        GetTaskByTitleQuery query = new(
            UserId: Guid.Empty,
            Text: "test");

        // Act
        TestValidationResult<GetTaskByTitleQuery> result = this._validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Does not return a validation error when UserId is valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_UserId_Is_Valid()
    {
        GetTaskByTitleQuery query = new(
            UserId: Guid.NewGuid(),
            Text: "test");

        TestValidationResult<GetTaskByTitleQuery> result = this._validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }

    /// <summary>
    /// Allows Text to be null.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_Text_Is_Null()
    {
        GetTaskByTitleQuery query = new(
            UserId: Guid.NewGuid(),
            Text: null);

        TestValidationResult<GetTaskByTitleQuery> result = this._validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(x => x.Text);
    }

    /// <summary>
    /// Allows Text to be empty or whitespace.
    /// </summary>
    /// <param name="text">
    /// Input search text that may be empty or contain only whitespace.
    /// </param>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Not_Have_Error_When_Text_Is_Empty_Or_Whitespace(string text)
    {
        GetTaskByTitleQuery query = new(
            UserId: Guid.NewGuid(),
            Text: text);

        TestValidationResult<GetTaskByTitleQuery> result = this._validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(x => x.Text);
    }

    /// <summary>
    /// Returns a validation error when Text exceeds 100 characters.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_Text_Exceeds_100_Characters()
    {
        GetTaskByTitleQuery query = new(
            UserId: Guid.NewGuid(),
            Text: new string('a', CommonPolicy.MaxSearchTextLength + 1));

        TestValidationResult<GetTaskByTitleQuery> result = this._validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Text)
              .WithErrorMessage(CommonPolicy.SearchTextTooLongMessage);
    }

    /// <summary>
    /// Does not return a validation error when Text length is exactly 100 characters.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_Text_Is_Exactly_100_Characters()
    {
        GetTaskByTitleQuery query = new(
            UserId: Guid.NewGuid(),
            Text: new string('a', CommonPolicy.MaxSearchTextLength));

        TestValidationResult<GetTaskByTitleQuery> result = this._validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(x => x.Text);
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
        GetTaskByTitleQuery query = new(Guid.NewGuid(), "test", page, 10);

        TestValidationResult<GetTaskByTitleQuery> result = this._validator.TestValidate(query);

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
        GetTaskByTitleQuery query = new(Guid.NewGuid(), "test", 1, pageSize);

        TestValidationResult<GetTaskByTitleQuery> result = this._validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage(CommonPolicy.PageSizeRangeMessage);
    }
}
