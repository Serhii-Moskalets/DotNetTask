using DotNetTask.Api.Requests.TaskList;
using DotNetTask.Application.Common.Dtos;
using DotNetTask.Application.TaskList.Commands.CreateTaskList;
using DotNetTask.Application.TaskList.Commands.DeleteTaskList;
using DotNetTask.Application.TaskList.Commands.UpdateTaskList;
using DotNetTask.Application.TaskList.Dtos;
using DotNetTask.Application.TaskList.Queries.GetTaskLists;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TinyResult;

namespace DotNetTask.Api.Controllers;

/// <summary>
/// Provides API endpoints to manage task lists for the current user.
/// Supports creating, updating, deleting, and retrieving task lists.
/// </summary>
[Authorize]
public class TaskListsController : BaseController
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TaskListsController"/> class.
    /// </summary>
    /// <param name="mediator">The Mediatr sender for dispatching commands and queries.</param>
    public TaskListsController(ISender mediator)
        : base(mediator)
    {
    }

    /// <summary>
    /// Retrieves all task lists for the current user.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="TaskListDto"/> or an error.</returns>
    [HttpGet]
    public async Task<IActionResult> GetTaskLists([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        GetTaskListsQuery query = new(this.CurrentUserId, page, pageSize);
        Result<PagedResultDto<TaskListDto>> result = await this.Mediator.Send(query, this.HttpContext.RequestAborted);
        return this.HandleResult(result);
    }

    /// <summary>
    /// Creates a new task list for the current user.
    /// </summary>
    /// <param name="request">The task list creation request containing the title.</param>
    /// <returns>
    /// A <see cref="CreatedAtActionResult"/> containing the created task list identifier
    /// if the operation succeeds; otherwise, a <see cref="BadRequestObjectResult"/>
    /// containing error details.
    /// </returns>
    [HttpPost]
    public async Task<IActionResult> CreateTaskList([FromBody] TaskListTitleRequest request)
    {
        CreateTaskListCommand command = new(this.CurrentUserId, request.Title);
        Result<Guid> result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);

        if (result.IsSuccess)
        {
            return this.CreatedAtAction(nameof(this.GetTaskLists), result.Value);
        }

        return this.HandleResult(result);
    }

    /// <summary>
    /// Deletes a task list for the current user by its ID.
    /// </summary>
    /// <param name="taskListId">The unique identifier of the task list to delete.</param>
    /// <returns>
    /// Returns <see cref="NoContentResult"/> if the operation succeeds;
    /// otherwise, a <see cref="BadRequestObjectResult"/>.
    /// </returns>
    [HttpDelete("{taskListId:guid}")]
    public async Task<IActionResult> DeleteTaskList([FromRoute] Guid taskListId)
    {
        DeleteTaskListCommand command = new(taskListId, this.CurrentUserId);
        Result<bool> result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }

    /// <summary>
    /// Updates the title of an existing task list.
    /// </summary>
    /// <param name="taskListId">The unique identifier of the task list to update.</param>
    /// <param name="request">The request containing the new task list title.</param>
    /// <returns>
    /// Returns <see cref="NoContentResult"/> if the task list was successfully updated;
    /// otherwise, returns a <see cref="BadRequestObjectResult"/> with error details.
    /// </returns>
    [HttpPut("{taskListId:guid}")]
    public async Task<IActionResult> UpdateTaskList([FromRoute] Guid taskListId, [FromBody] TaskListTitleRequest request)
    {
        UpdateTaskListCommand command = new(taskListId, this.CurrentUserId, request.Title);
        Result<bool> result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }
}
