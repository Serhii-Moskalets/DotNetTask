using DotNetTask.Api.Requests.UserTaskAccess;
using DotNetTask.Application.Common.Dtos;
using DotNetTask.Application.UserTaskAccess.Commands.CreateUserTaskAccess;
using DotNetTask.Application.UserTaskAccess.Commands.DeleteTaskAccessById;
using DotNetTask.Application.UserTaskAccess.Commands.DeleteTaskAccessByUserEmail;
using DotNetTask.Application.UserTaskAccess.Commands.DeleteTaskAccessesByTask;
using DotNetTask.Application.UserTaskAccess.Commands.DeleteTaskAccessesByUser;
using DotNetTask.Application.UserTaskAccess.Dtos;
using DotNetTask.Application.UserTaskAccess.Queries.GetSharedTaskById;
using DotNetTask.Application.UserTaskAccess.Queries.GetSharedTasksByUserId;
using DotNetTask.Application.UserTaskAccess.Queries.GetUsersWithTaskAccess;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TinyResult;

namespace DotNetTask.Api.Controllers;

/// <summary>
/// Controller responsible for managing user-task access operations.
/// Provides endpoints to retrieve shared tasks, get users with access to a task,
/// and manage access assignments and deletions.
/// </summary>
[Authorize]
[Route("api/access")]
public class UserTaskAccessesController : BaseController
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserTaskAccessesController"/> class.
    /// </summary>
    /// <param name="mediator">The Mediatr sender for dispatching commands and queries.</param>
    public UserTaskAccessesController(ISender mediator)
        : base(mediator)
    {
    }

    /// <summary>
    /// Retrieves a shared task by its ID for the current user.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <returns>An <see cref="IActionResult"/> containing the task details if found; otherwise, an error response.</returns>
    [HttpGet("tasks/{taskId:guid}/shared")]
    public async Task<IActionResult> GetSharedTaskById([FromRoute] Guid taskId)
    {
        GetSharedTaskByIdQuery query = new(taskId, this.CurrentUserId);
        Result<TaskDto> result = await this.Mediator.Send(query, this.HttpContext.RequestAborted);
        return this.HandleResult(result);
    }

    /// <summary>
    /// Retrieves all tasks that are shared with the current user.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>An <see cref="IActionResult"/> containing a list of tasks shared with the user.</returns>
    [HttpGet]
    public async Task<IActionResult> GetSharedTasksByUserId([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        GetSharedTasksByUserIdQuery query = new(this.CurrentUserId, page, pageSize);
        Result<PagedResultDto<TaskDto>> result = await this.Mediator.Send(query, this.HttpContext.RequestAborted);
        return this.HandleResult(result);
    }

    /// <summary>
    /// Retrieves a task along with all users who have access to it.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>An <see cref="IActionResult"/> containing the task access list details.</returns>
    [HttpGet("tasks/{taskId:guid}/users")]
    public async Task<IActionResult> GetUsersWithTaskAccess([FromRoute] Guid taskId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        GetUsersWithTaskAccessQuery query = new(taskId, this.CurrentUserId, page, pageSize);
        Result<TaskAccessListDto> result = await this.Mediator.Send(query, this.HttpContext.RequestAborted);
        return this.HandleResult(result);
    }

    /// <summary>
    /// Grants access to a user for a specific task.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="request">Request containing the email of the user to share the task with.</param>
    /// <returns>An <see cref="IActionResult"/> indicating success or failure of the operation.</returns>
    [HttpPost("tasks/{taskId:guid}/share-task")]
    public async Task<IActionResult> CreateUserTaskAccess([FromRoute] Guid taskId, [FromBody] AccessEmailRequest request)
    {
        CreateUserTaskAccessCommand command = new(taskId, this.CurrentUserId, request.Email ?? string.Empty);
        Result<bool> result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleResult(result);
    }

    /// <summary>
    /// Deletes a task access entry based on the user's email.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="email">The email of the user whose access should be removed.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the outcome of the deletion.</returns>
    [HttpDelete("tasks/{taskId:guid}/by-email")]
    public async Task<IActionResult> DeleteTaskAccessByEmail([FromRoute] Guid taskId, [FromQuery] string? email)
    {
        DeleteTaskAccessByUserEmailCommand command = new(taskId, this.CurrentUserId, email ?? string.Empty);
        Result<bool> result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }

    /// <summary>
    /// Deletes a task access entry access ID.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="userId">The unique identifier of the user whose access will be removed.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the outcome of the deletion.</returns>
    [HttpDelete("tasks/{taskId:guid}/users/{userId:guid}")]
    public async Task<IActionResult> DeleteTaskAccessById([FromRoute] Guid taskId, [FromRoute] Guid userId)
    {
        DeleteTaskAccessByIdCommand command = new(taskId, userId, this.CurrentUserId);
        Result<bool> result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }

    /// <summary>
    /// Deletes all access entries for a specific task.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the outcome of the deletion.</returns>
    [HttpDelete("tasks/{taskId:guid}")]
    public async Task<IActionResult> DeleteTaskAccessesByTask([FromRoute] Guid taskId)
    {
        DeleteTaskAccessesByTaskCommand command = new(taskId, this.CurrentUserId);
        Result<bool> result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }

    /// <summary>
    /// Deletes all task access entries associated with the current user.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> indicating the outcome of the deletion.</returns>
    [HttpDelete("users/my")]
    public async Task<IActionResult> DeleteTasksAccessesByUser()
    {
        DeleteTaskAccessesByUserCommand command = new(this.CurrentUserId);
        Result<bool> result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }
}
