using DotNetTask.Application.Abstractions.Interfaces.Repositories;
using DotNetTask.Domain.Entities;
using DotNetTask.Infrastructure.Extensions;
using DotNetTask.Infrastructure.Persistence.DatabaseContext;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace DotNetTask.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for managing <see cref="CommentEntity"/> objects in the database.
/// Provides methods to retrieve, check ownership, and paginate comments.
/// </summary>
public class CommentRepository(DotNetTaskDbContext context)
    : BaseRepository<CommentEntity>(context), ICommentRepository
{
    /// <summary>
    /// Retrieves all comments associated with a specific task.
    /// </summary>
    /// <param name="taskId">The ID of the task for which to retrieve comments.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of comments per page.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only collection of comments for the specified task.</returns>
    public async Task<(IReadOnlyCollection<CommentEntity> Items, int TotalCount)> GetCommentsByTaskIdAsync(
        Guid taskId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        IIncludableQueryable<CommentEntity, UserEntity> commentsQuery = this.DbSet.AsNoTracking()
            .Where(x => x.TaskId == taskId)
            .Include(x => x.User);

        int totalCount = await commentsQuery.CountAsync(cancellationToken);

        List<CommentEntity> items = await commentsQuery
            .OrderBy(x => x.CreatedDate)
            .ApplyPagination(page, pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
