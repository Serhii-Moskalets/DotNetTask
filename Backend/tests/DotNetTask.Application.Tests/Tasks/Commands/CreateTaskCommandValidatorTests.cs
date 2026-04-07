using DotNetTask.Application.Tasks.Commands.CreateTask;
using DotNetTask.Application.Tasks.Dtos;
using DotNetTask.Domain.Constants;
using DotNetTask.Domain.ValueObjects;

using FluentValidation.TestHelper;

namespace DotNetTask.Application.Tests.Tasks.Commands;

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
        CreateTaskCommand command = new(
            new CreateTaskDto
            {
                TaskListId = Guid.Empty,
                Title = "Valid title",
            }, Guid.NewGuid());

        // Act & Assert
        TestValidationResult<CreateTaskCommand> result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Dto.TaskListId)
            .WithErrorMessage(TaskListPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Ensures validation fails when the user ID is empty.
    /// </summary>
    [Fact]
    public void Should_HaveError_When_UserId_IsEmpty()
    {
        // Arrange
        CreateTaskCommand command = new(
            new CreateTaskDto
            {
                TaskListId = Guid.NewGuid(),
                Title = "Valid title",
            }, Guid.Empty);

        // Act & Assert
        TestValidationResult<CreateTaskCommand> result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.UserId)
            .WithErrorMessage(UserPolicy.IdRequiredMessage);
    }

    /// <summary>
    /// Ensures validation fails when the task title is empty.
    /// </summary>
    [Fact]
    public void Should_HaveError_When_Title_IsEmpty()
    {
        // Arrange
        CreateTaskCommand command = new(
            new CreateTaskDto
            {
                TaskListId = Guid.NewGuid(),
                Title = string.Empty,
            }, Guid.NewGuid());

        // Act & Assert
        TestValidationResult<CreateTaskCommand> result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Dto.Title)
            .WithErrorMessage(TaskPolicy.EmptyTitleMessage);
    }

    /// <summary>
    /// Ensures validation fails when the task title exceeds the maximum length.
    /// </summary>
    [Fact]
    public void Should_HaveError_When_Title_TooLong()
    {
        // Arrange
        string longTitle = new('A', TaskTitle.MaxLength + 1);
        CreateTaskCommand command = new(
            new CreateTaskDto
            {
                TaskListId = Guid.NewGuid(),
                Title = longTitle,
            }, Guid.NewGuid());

        // Act & Assert
        TestValidationResult<CreateTaskCommand> result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Dto.Title)
            .WithErrorMessage(TaskPolicy.TooLongTitleMessage);
    }

    /// <summary>
    /// Ensures validation fails when the due date is in the past.
    /// </summary>
    [Fact]
    public void Should_HaveError_When_DueDate_IsInPast()
    {
        // Arrange
        CreateTaskCommand command = new(
            new CreateTaskDto
            {
                TaskListId = Guid.NewGuid(),
                Title = "Valid title",
                DueDate = DateTime.UtcNow.AddMinutes(-1),
            }, Guid.NewGuid());

        // Act & Assert
        TestValidationResult<CreateTaskCommand> result = this._validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Dto.DueDate)
            .WithErrorMessage(TaskPolicy.InvalidDueDateMessage);
    }

    /// <summary>
    /// Ensures no validation errors occur when the command is valid.
    /// </summary>
    [Fact]
    public void Should_NotHaveError_When_CommandIsValid()
    {
        // Arrange
        CreateTaskCommand command = new(
            new CreateTaskDto
            {
                TaskListId = Guid.NewGuid(),
                Title = "Valid title",
                DueDate = DateTime.UtcNow.AddMinutes(5),
            }, Guid.NewGuid());

        // Act & Assert
        TestValidationResult<CreateTaskCommand> result = this._validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
