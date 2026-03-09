using FluentValidation.TestHelper;
using TodoListApp.Application.Tasks.Commands.CreateTask;
using TodoListApp.Application.Tasks.Dtos;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tests.Tasks.Commands;

/// <summary>
/// Unit tests for <see cref="CreateTaskCommandValidator"/>.
/// Tests validation rules for creating a task list.
/// </summary>
public class CreateTaskCommandValidatorTests
{
    private readonly CreateTaskCommandValidator _validator = new();

    /// <summary>
    /// Ensures validation fails when the task list ID is empty.
    /// </summary>
    [Fact]
    public void Should_HaveError_When_TaskListId_IsEmpty()
    {
        // Arrange
        var command = new CreateTaskCommand(
            new CreateTaskDto
            {
                TaskListId = Guid.Empty,
                Title = "Valid title",
            }, Guid.NewGuid());

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Dto.TaskListId)
            .WithErrorMessage("Task list ID is required.");
    }

    /// <summary>
    /// Ensures validation fails when the user ID is empty.
    /// </summary>
    [Fact]
    public void Should_HaveError_When_UserId_IsEmpty()
    {
        // Arrange
        var command = new CreateTaskCommand(
            new CreateTaskDto
            {
                TaskListId = Guid.NewGuid(),
                Title = "Valid title",
            }, Guid.Empty);

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.UserId)
            .WithErrorMessage("User ID is required.");
    }

    /// <summary>
    /// Ensures validation fails when the task title is empty.
    /// </summary>
    [Fact]
    public void Should_HaveError_When_Title_IsEmpty()
    {
        // Arrange
        var command = new CreateTaskCommand(
            new CreateTaskDto
            {
                TaskListId = Guid.NewGuid(),
                Title = string.Empty,
            }, Guid.NewGuid());

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Dto.Title)
            .WithErrorMessage("Task title cannot be empty.");
    }

    /// <summary>
    /// Ensures validation fails when the task title exceeds the maximum length.
    /// </summary>
    [Fact]
    public void Should_HaveError_When_Title_TooLong()
    {
        // Arrange
        var longTitle = new string('A', TaskTitle.MaxLength + 1);
        var command = new CreateTaskCommand(
            new CreateTaskDto
            {
                TaskListId = Guid.NewGuid(),
                Title = longTitle,
            }, Guid.NewGuid());

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Dto.Title)
            .WithErrorMessage($"Title cannot exceed {TaskTitle.MaxLength} characters.");
    }

    /// <summary>
    /// Ensures validation fails when the due date is in the past.
    /// </summary>
    [Fact]
    public void Should_HaveError_When_DueDate_IsInPast()
    {
        // Arrange
        var command = new CreateTaskCommand(
            new CreateTaskDto
            {
                TaskListId = Guid.NewGuid(),
                Title = "Valid title",
                DueDate = DateTime.UtcNow.AddMinutes(-1),
            }, Guid.NewGuid());

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Dto.DueDate)
            .WithErrorMessage("Due date cannot be in the past.");
    }

    /// <summary>
    /// Ensures no validation errors occur when the command is valid.
    /// </summary>
    [Fact]
    public void Should_NotHaveError_When_CommandIsValid()
    {
        // Arrange
        var command = new CreateTaskCommand(
            new CreateTaskDto
            {
                TaskListId = Guid.NewGuid(),
                Title = "Valid title",
                DueDate = DateTime.UtcNow.AddMinutes(5),
            }, Guid.NewGuid());

        // Act & Assert
        var result = this._validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
