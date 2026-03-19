using FluentValidation.TestHelper;
using TodoListApp.Application.TaskList.Commands.CreateTaskList;
using TodoListApp.Domain.Constants;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tests.TaskList.Commands;

/// <summary>
/// Unit tests for <see cref="CreateTaskListCommandValidator"/>.
/// Verifies that the validator correctly enforces rules for creating a task list.
/// </summary>
public class CreateTaskListCommandValidatorTests
{
    private readonly CreateTaskListCommandValidator _validator = new();

    /// <summary>
    /// Ensures validation fails when the title is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_Title_Is_Empty()
    {
        // Arrange
        var command = new CreateTaskListCommand(Guid.NewGuid(), string.Empty);

        // Act
        var result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Title)
              .WithErrorMessage(TaskListPolicy.EmptyMessage);
    }

    /// <summary>
    /// Ensures validation fails when the title exceeds the maximum length.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_Title_Too_Long()
    {
        // Arrange
        var longTitle = new string('A', TaskListTitle.MaxLength + 1);
        var command = new CreateTaskListCommand(Guid.NewGuid(), longTitle);

        // Act
        var result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Title)
              .WithErrorMessage(TaskListPolicy.TooLongMessage);
    }

    /// <summary>
    /// Ensures validation fails when the user ID is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        var command = new CreateTaskListCommand(Guid.Empty, "Valid Title");

        // Act
        var result = this._validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.UserId)
              .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Ensures no validation errors when the command is valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_Valid()
    {
        // Arrange
        var command = new CreateTaskListCommand(Guid.NewGuid(), "Valid Title");

        // Act
        var result = this._validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.Title);
        result.ShouldNotHaveValidationErrorFor(c => c.UserId);
    }
}
