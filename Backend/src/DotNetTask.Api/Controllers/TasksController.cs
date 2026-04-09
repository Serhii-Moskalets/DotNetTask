using DotNetTask.Api.Requests.Task;
using DotNetTask.Application.Common.Dtos;
using DotNetTask.Application.Tasks.Commands.AddTagToTask;
using DotNetTask.Application.Tasks.Commands.ChangeTaskStatus;
using DotNetTask.Application.Tasks.Commands.CreateTask;
using DotNetTask.Application.Tasks.Commands.DeleteRangeTasks;
using DotNetTask.Application.Tasks.Commands.DeleteTask;
using DotNetTask.Application.Tasks.Commands.RemoveTagFromTask;
using DotNetTask.Application.Tasks.Commands.UpdateTask;
using DotNetTask.Application.Tasks.Dtos;
using DotNetTask.Application.Tasks.Queries.GetTaskById;
using DotNetTask.Application.Tasks.Queries.GetTaskByTitle;
using DotNetTask.Application.Tasks.Queries.GetTasks;
using DotNetTask.Domain.Enums;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TinyResult;

using Unit = DotNetTask.Domain.Common.Unit;

namespace DotNetTask.Api.Controllers;

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
        GetTaskByIdQuery query = new(this.CurrentUserId, taskId);
        Result<TaskDto> result = await this.Mediator.Send(query, this.HttpContext.RequestAborted);
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
        GetTasksQuery query = new(
            this.CurrentUserId,
            request.TaskListId,
            request.Page,
            request.PageSize,
            request.TaskStatuses,
            request.DueBefore,
            request.DueAfter,
            request.TaskSortBy,
            request.Ascending);

        Result<PagedResultDto<TaskBriefDto>> result = await this.Mediator.Send(query, this.HttpContext.RequestAborted);
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
        GetTaskByTitleQuery query = new(this.CurrentUserId, request.Title);
        Result<PagedResultDto<TaskBriefDto>> result = await this.Mediator.Send(query, this.HttpContext.RequestAborted);
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
        CreateTaskCommand command = new(
            new CreateTaskDto
            {
                Title = request.Title,
                DueDate = request.DueDate,
                TaskListId = taskListId,
            }, this.CurrentUserId);

        Result<Guid> result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);

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
        UpdateTaskCommand command = new(
            new UpdateTaskDto
            {
                TaskId = taskId,
                Title = request.Title,
                Description = request.Description,
                DueDate = request.DueDate,
            }, this.CurrentUserId);

        Result<Unit> result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
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
        DeleteTaskCommand command = new(taskId, this.CurrentUserId);
        Result<Unit> result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }

    /// <summary>
    /// Deletes a batch of tasks.
    /// </summary>
    /// <param name="request">An object containing the identifiers of the tasks to delete. Cannot be null.</param>
    /// <returns>
    /// Returns <see cref="OkObjectResult"/> containing the count of deleted tasks if the operation succeeds;
    /// otherwise, a <see cref="BadRequestObjectResult"/>.
    /// </returns>
    [HttpDelete("batch")]
    public async Task<IActionResult> DeleteRangeTasks([FromBody] DeleteRangeTasksRequest request)
    {
        DeleteRangeTasksCommand command = new(request.TaskIds, this.CurrentUserId);
        Result<int> result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleResult(result);
    }

    /// <summary>
    /// Adds a tag to the specified task.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="tagId">The tag identifier.</param>
    /// <returns>
    /// Returns <see cref="NoContentResult"/> if the operation succeeds;
    /// otherwise, a <see cref="BadRequestObjectResult"/>.
    /// </returns>
    [HttpPut("{taskId:guid}/tags/{tagId:guid}")]
    public async Task<IActionResult> AddTagToTask([FromRoute] Guid taskId, [FromRoute] Guid tagId)
    {
        AddTagToTaskCommand command = new(taskId, this.CurrentUserId, tagId);
        Result<Unit> result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
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
        RemoveTagFromTaskCommand command = new(taskId, this.CurrentUserId);
        Result<Unit> result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
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
        ChangeTaskStatusCommand command = new(taskId, this.CurrentUserId, status);
        Result<Unit> result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }
}
