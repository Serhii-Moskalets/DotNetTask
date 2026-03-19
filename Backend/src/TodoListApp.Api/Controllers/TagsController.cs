using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Api.Requests.Tag;
using TodoListApp.Application.Common.Dtos;
using TodoListApp.Application.Tag.Commands.CreateTag;
using TodoListApp.Application.Tag.Commands.DeleteTag;
using TodoListApp.Application.Tag.Queries.GetTags;

namespace TodoListApp.Api.Controllers;

/// <summary>
/// Controller for managing tags.
/// Provides endpoints to get all tags, create a new tag, and delete an existing tag.
/// </summary>
[Authorize]
public class TagsController : BaseController
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TagsController"/> class.
    /// </summary>
    /// <param name="mediator">The Mediatr sender for dispatching commands and queries.</param>
    public TagsController(ISender mediator)
        : base(mediator)
    {
    }

    /// <summary>
    /// Retrieves all tags for the current user.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="TagDto"/>.</returns>
    [HttpGet]
    public async Task<IActionResult> GetTags([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetTagsQuery(this.CurrentUserId, page, pageSize);
        var result = await this.Mediator.Send(query, this.HttpContext.RequestAborted);
        return this.HandleResult(result);
    }

    /// <summary>
    /// Creates a new tag for a specific task.
    /// </summary>
    /// <param name="taskId">The ID of the task to attach the tag to.</param>
    /// <param name="request">The tag creation request containing the tag name.</param>
    /// <returns>An <see cref="IActionResult"/> containing the created tag ID.</returns>
    [HttpPost("{taskId:guid}")]
    public async Task<IActionResult> CreateTag([FromRoute] Guid taskId, [FromBody] TagTitleRequest request)
    {
        var command = new CreateTagCommand(this.CurrentUserId, taskId, request.Name);
        var result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);

        if (result.IsSuccess)
        {
            return this.CreatedAtAction(nameof(this.GetTags), result.Value);
        }

        return this.HandleResult(result);
    }

    /// <summary>
    /// Deletes a tag by its ID.
    /// </summary>
    /// <param name="tagId">The ID of the tag to delete.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the outcome of the operation.</returns>
    [HttpDelete("{tagId:guid}")]
    public async Task<IActionResult> DeleteTag([FromRoute] Guid tagId)
    {
        var command = new DeleteTagCommand(tagId, this.CurrentUserId);
        var result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }
}
