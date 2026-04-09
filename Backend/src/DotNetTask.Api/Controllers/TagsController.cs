using DotNetTask.Api.Requests.Tag;
using DotNetTask.Application.Common.Dtos;
using DotNetTask.Application.Tag.Commands.CreateTag;
using DotNetTask.Application.Tag.Commands.DeleteTag;
using DotNetTask.Application.Tag.Queries.GetTags;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TinyResult;

using Unit = DotNetTask.Domain.Common.Unit;

namespace DotNetTask.Api.Controllers;

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
        GetTagsQuery query = new(this.CurrentUserId, page, pageSize);
        Result<PagedResultDto<TagDto>> result = await this.Mediator.Send(query, this.HttpContext.RequestAborted);
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
        CreateTagCommand command = new(this.CurrentUserId, taskId, request.Name);
        Result<Guid> result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);

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
        DeleteTagCommand command = new(tagId, this.CurrentUserId);
        Result<Unit> result = await this.Mediator.Send(command, this.HttpContext.RequestAborted);
        return this.HandleNoContent(result);
    }
}
