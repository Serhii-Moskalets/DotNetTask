using FluentValidation.TestHelper;
using TodoListApp.Application.Tasks.Commands.UpdateTask;
using TodoListApp.Application.Tasks.Dtos;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.ValueObjects;

namespace TodoListApp.Application.Tests.Tasks.Commands;

/// <summary>
/// Contains unit tests for <see cref="UpdateTaskCommandValidator"/>.
/// Ensures that the validator correctly enforces rules for updating a task.
/// </summary>
public class UpdateTaskCommandValidatorTests
{
    private readonly UpdateTaskCommandValidator _validator = new();

    /// <summary>
    /// Ensures validation fails when the task ID is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_TaskId_Is_Empty()
    {
        var command = new UpdateTaskCommand(
            new UpdateTaskDto
            {
                TaskId = Guid.Empty,
                Title = "Valid title",
            },
            Guid.NewGuid());

        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Dto.TaskId)
              .WithErrorMessage("Task ID is required.");
    }

    /// <summary>
    /// Ensures validation fails when the user ID is empty.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        var command = new UpdateTaskCommand(
            new UpdateTaskDto
            {
                TaskId = Guid.NewGuid(),
                Title = "Valid title",
            },
            Guid.Empty);

        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.UserId)
              .WithErrorMessage("User ID is required.");
    }

    /// <summary>
    /// Ensures validation fails when the title exceeds the maximum length.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_Title_TooLong()
    {
        var longTitle = new string('A', TaskTitle.MaxLength + 1);
        var command = new UpdateTaskCommand(
            new UpdateTaskDto
            {
                TaskId = Guid.NewGuid(),
                Title = longTitle,
            },
            Guid.NewGuid());

        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Dto.Title)
              .WithErrorMessage($"Title cannot exceed {TaskTitle.MaxLength} characters.");
    }

    /// <summary>
    /// Ensures validation fails when the description exceeds the maximum length.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_Description_TooLong()
    {
        var longDescription = new string('B', TaskDescription.MaxLength + 1);
        var command = new UpdateTaskCommand(
            new UpdateTaskDto
            {
                TaskId = Guid.NewGuid(),
                Title = "Valid title",
                Description = longDescription,
            },
            Guid.NewGuid());

        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Dto.Description)
              .WithErrorMessage($"Description cannot exceed {TaskDescription.MaxLength} characters.");
    }

    /// <summary>
    /// Ensures validation fails when the due date is in the past.
    /// </summary>
    [Fact]
    public void Should_Have_Error_When_DueDate_IsInPast()
    {
        var command = new UpdateTaskCommand(
            new UpdateTaskDto
            {
                TaskId = Guid.NewGuid(),
                Title = "Valid title",
                DueDate = DateTime.UtcNow.AddMinutes(-1),
            },
            Guid.NewGuid());

        var result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Dto.DueDate)
              .WithErrorMessage("Due date cannot be in the past.");
    }

    /// <summary>
    /// Ensures no validation errors occur when the command is valid.
    /// </summary>
    [Fact]
    public void Should_Not_Have_Error_When_CommandIsValid()
    {
        var command = new UpdateTaskCommand(
            new UpdateTaskDto
            {
                TaskId = Guid.NewGuid(),
                Title = "Valid title",
                Description = "Valid description",
                DueDate = DateTime.UtcNow.AddMinutes(5),
            },
            Guid.NewGuid());

        var result = this._validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
