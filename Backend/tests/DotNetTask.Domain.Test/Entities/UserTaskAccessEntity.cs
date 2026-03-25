using DotNetTask.Domain.Entities;

namespace DotNetTask.Domain.Test.Entities;

/// <summary>
/// Unit tests for the <see cref="UserTaskAccessEntity"/> class.
/// </summary>
public class UserTaskAccessEntityTest
{
    /// <summary>
    /// Tests that the constructor sets the <see cref="UserTaskAccessEntity.TaskId"/>
    /// and <see cref="UserTaskAccessEntity.UserId"/> correctly.
    /// </summary>
    [Fact]
    public void Constructor_Should_SetTaskIdAndUserId()
    {
        Guid taskId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        UserTaskAccessEntity access = new UserTaskAccessEntity(taskId, userId);

        Assert.Equal(taskId, access.TaskId);
        Assert.Equal(userId, access.UserId);
    }
}
