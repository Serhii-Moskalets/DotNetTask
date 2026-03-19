using FluentValidation.TestHelper;
using TodoListApp.Application.Tasks.Queries.GetTaskById;
using TodoListApp.Domain.Constants;

namespace TodoListApp.Application.Tests.Tasks.Queries;

/// <summary>
/// Unit tests for <see cref="GetTaskByIdQueryValidator"/>.
/// Ensures that the validator correctly enforces rules for retrieving a task by ID.
/// </summary>
public class GetTaskByIdQueryValidatorTests
{
    private readonly GetTaskByIdQueryValidator _validator = new();

    /// <summary>
    /// Validation fails when <see cref="GetTaskByIdQuery.UserId"/> is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        var query = new GetTaskByIdQuery(Guid.Empty, Guid.NewGuid());

        // Act
        var result = this._validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(q => q.UserId)
              .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Validation fails when <see cref="GetTaskByIdQuery.TaskId"/> is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_TaskId_Is_Empty()
    {
        // Arrange
        var query = new GetTaskByIdQuery(Guid.NewGuid(), Guid.Empty);

        // Act
        var result = this._validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(q => q.TaskId)
              .WithErrorMessage(TaskPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Validation succeeds when both <see cref="GetTaskByIdQuery.UserId"/> and <see cref="GetTaskByIdQuery.TaskId"/> are provided.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_UserId_And_TaskId_Are_Provided()
    {
        // Arrange
        var query = new GetTaskByIdQuery(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = this._validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(q => q.UserId);
        result.ShouldNotHaveValidationErrorFor(q => q.TaskId);
    }
}
