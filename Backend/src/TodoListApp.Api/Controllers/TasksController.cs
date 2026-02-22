using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Api.Requests.Task;
using TodoListApp.Application.Tasks.Commands.AddTagToTask;
using TodoListApp.Application.Tasks.Commands.ChangeTaskStatus;
using TodoListApp.Application.Tasks.Commands.CreateTask;
using TodoListApp.Application.Tasks.Commands.DeleteTask;
using TodoListApp.Application.Tasks.Commands.RemoveTagFromTask;
using TodoListApp.Application.Tasks.Commands.UpdateTask;
using TodoListApp.Application.Tasks.Dtos;
using TodoListApp.Application.Tasks.Queries.GetTaskById;
using TodoListApp.Application.Tasks.Queries.GetTaskByTitle;
using TodoListApp.Application.Tasks.Queries.GetTasks;
using TodoListApp.Domain.Enums;

namespace TodoListApp.Api.Controllers;

/// <summary>
/// Provides HTTP endpoints for managing tasks.
/// </summary>
/// <remarks>
/// This controller acts as an API layer and delegates
/// all business logic to application command and query handlers.
/// </remarks>
[Authorize]
public class TasksController : BaseController
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TasksController"/> class.
    /// </summary>
    /// <param name="mediator">The Mediatr sender for dispatching commands and queries.</param>
    public TasksController(ISender mediator)
        : base(mediator)
    {
    }

    /// <summary>
    /// Retrieves a task by its identifier.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <returns>The requested task if found.</returns>
    [HttpGet("{taskId:guid}")]
    public async Task<IActionResult> GetTaskById([FromRoute] Guid taskId)
    {
        var query = new GetTaskByIdQuery(this.CurrentUserId, taskId);
        var result = await this.Mediator.Send(query, this.HttpContext.RequestAborted);
        return this.HandleResult(result);
    }

    /// <summary>
    /// Retrieves all tasks for the current user with optional filters.
    /// </summary>
    /// <param name="request">Filtering and sorting parameters.</param>
    /// <returns>A list of tasks.</returns>
    [HttpGet]
    public async Task<IActionResult> GetTasks([FromQuery] GetTasksRequest request)
    {
        var query = new GetTasksQuery(
            this.CurrentUserId,
            request.TaskListId,
            request.Page,
            request.PageSize,
            request.TaskStatuses,
            request.DueBefore,
            request.DueAfter,
            request.TaskSortBy,
            request.Ascending);

        var result = await this.Mediator.Send(query, this.HttpContext.RequestAborted);
        return this.HandleResult(result);
    }

    /// <summary>
    /// Retrieves tasks that match the specified title.
    /// </summary>
    /// <param name="request">The request containing the task title.</param>
    /// <returns>A list of matching tasks.</returns>
    [HttpGet("by-title")]
    public async Task<IActionResult> GetTasksByTitle([FromQuery] GetTaskByTitleRequest request)
    {
        var query = new GetTaskByTitleQuery(this.CurrentUserId, request.Title);
        var result = await this.Mediator.Send(query, this.HttpContext.RequestAborted);
        return this.HandleResult(result);
    }

    /// <summary>
    /// Creates a new task.
    /// </summary>
    /// <param name="taskListId">The unique identifier of the task list.</param>
    /// <param name="request">The task creation request.</param>
    /// <returns>
    /// An <see cref="OkObjectResult"/> containing the created task identifier
    /// if the operation succeeds; otherwise, a <see cref="BadRequestObjectResult"/>
    /// containing error details.
    /// </returns>
    [HttpPost("task-lists/{taskListId:guid}")]
    public async Task<IActionResult> CreateTask([FromRoute] Guid taskListId, [FromBody] CreateTaskDtoRequest request)
    {
        var command = new CreateTaskCommand(
            new CreateTaskDto
            {
                Title = request.Title,
                DueDate = request.DueDate,
                TaskListId = taskListId,
            }, this.CurrentUserId);

        var result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);

        if (result.IsSuccess)
        {
            return this.CreatedAtAction(
                nameof(this.GetTaskById),
                new { taskId = result.Value },
                result.Value);
        }

        return this.HandleResult(result);
    }

    /// <summary>
    /// Updates an existing task.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="request">The task update request.</param>
    /// <returns>
    /// Returns <see cref="NoContentResult"/> if the task was successfully updated;
    /// otherwise, a <see cref="BadRequestObjectResult"/>.
    /// </returns>
    [HttpPut("{taskId:guid}")]
    public async Task<IActionResult> UpdateTask([FromRoute] Guid taskId, [FromBody] UpdateTaskDtoRequest request)
    {
        var command = new UpdateTaskCommand(
            new UpdateTaskDto
            {
                TaskId = taskId,
                Title = request.Title,
                Description = request.Description,
                DueDate = request.DueDate,
            }, this.CurrentUserId);

        var result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }

    /// <summary>
    /// Deletes a task by its identifier.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <returns>
    /// Returns <see cref="NoContentResult"/> if the operation succeeds;
    /// otherwise, a <see cref="BadRequestObjectResult"/>.
    /// </returns>
    [HttpDelete("{taskId:guid}")]
    public async Task<IActionResult> DeleteTask([FromRoute] Guid taskId)
    {
        var command = new DeleteTaskCommand(taskId, this.CurrentUserId);
        var result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }

    /// <summary>
    /// Adds a tag to the specified task.
    /// </summary>
    /// <param name="taskId">The task identifier..</param>
    /// <param name="tagId">The tag identifier.</param>
    /// <returns>
    /// Returns <see cref="NoContentResult"/> if the operation succeeds;
    /// otherwise, a <see cref="BadRequestObjectResult"/>.
    /// </returns>
    [HttpPut("{taskId:guid}/tags/{tagId:guid}")]
    public async Task<IActionResult> AddTagToTask([FromRoute] Guid taskId, [FromRoute] Guid tagId)
    {
        var command = new AddTagToTaskCommand(taskId, this.CurrentUserId, tagId);
        var result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }

    /// <summary>
    /// Removes the single tag from a task.
    /// Each task can only have one tag.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <returns>
    /// Returns <see cref="NoContentResult"/> if the operation succeeds;
    /// otherwise, a <see cref="BadRequestObjectResult"/>.
    /// </returns>
    [HttpDelete("{taskId:guid}/tag")]
    public async Task<IActionResult> RemoveTagFromTask([FromRoute] Guid taskId)
    {
        var command = new RemoveTagFromTaskCommand(taskId, this.CurrentUserId);
        var result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }

    /// <summary>
    /// Changes the status of a task.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="status">The status of task.</param>
    /// <returns>
    /// Returns <see cref="NoContentResult"/> if the operation succeeds;
    /// otherwise, a <see cref="BadRequestObjectResult"/>.
    /// </returns>
    [HttpPut("{taskId:guid}/status/{status:int}")]
    public async Task<IActionResult> ChangeTaskStatus([FromRoute] Guid taskId, [FromRoute] StatusTask status)
    {
        var command = new ChangeTaskStatusCommand(taskId, this.CurrentUserId, status);
        var result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }
}