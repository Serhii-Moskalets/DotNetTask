using FluentValidation;
using MediatR;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.Constants;

namespace TodoListApp.Application.Tag.Commands.DeleteTag;

/// <summary>
/// Handles the <see cref="DeleteTagCommand"/> by deleting a tag that belongs to a specific user.
/// </summary>
public class DeleteTagCommandHandler(
    IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), IRequestHandler<DeleteTagCommand, Result<bool>>
{
    /// <summary>
    /// Processes the command to delete a user's tag.
    /// </summary>
    /// <param name="command">
    /// The command containing the ID of the tag to delete and the user ID of the owner.
    /// </param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating success if the tag was deleted,
    /// or failure if the tag was not found or the user is not the owner.
    /// </returns>
    public async Task<Result<bool>> Handle(DeleteTagCommand command, CancellationToken cancellationToken)
    {
        var tag = await this.UnitOfWork.Tags
            .GetTagByIdForUserAsync(command.TagId, command.UserId, asNoTracking: false, cancellationToken);
        if (tag is null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, TagPolicy.NotFoundMessage);
        }

        await this.UnitOfWork.Tags.DeleteAsync(tag, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
