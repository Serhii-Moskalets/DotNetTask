using DotNetTask.Application.TaskList.Commands.CreateTaskList;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.ValueObjects;

using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.TaskList.Commands;

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
        CreateTaskListCommand command = new(Guid.NewGuid(), string.Empty);

        // Act
        TestValidationResult<CreateTaskListCommand> result = this._validator.TestValidate(command);

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
        string longTitle = new('A', TaskListTitle.MaxLength + 1);
        CreateTaskListCommand command = new(Guid.NewGuid(), longTitle);

        // Act
        TestValidationResult<CreateTaskListCommand> result = this._validator.TestValidate(command);

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
        CreateTaskListCommand command = new(Guid.Empty, "Valid Title");

        // Act
        TestValidationResult<CreateTaskListCommand> result = this._validator.TestValidate(command);

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
        CreateTaskListCommand command = new(Guid.NewGuid(), "Valid Title");

        // Act
        TestValidationResult<CreateTaskListCommand> result = this._validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(c => c.Title);
        result.ShouldNotHaveValidationErrorFor(c => c.UserId);
    }
}
