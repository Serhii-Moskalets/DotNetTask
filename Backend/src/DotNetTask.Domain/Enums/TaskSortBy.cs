namespace DotNetTask.Domain.Enums;

/// <summary>
/// Specifies the property by which tasks can be sorted.
/// </summary>
public enum TaskSortBy
{
    /// <summary>
    /// Sort tasks by their title.
    /// </summary>
    Title,

    /// <summary>
    /// Sort tasks by the date they were created.
    /// </summary>
    CreatedDate,

    /// <summary>
    /// Sort tasks by their due date.
    /// </summary>
    DueDate,

    /// <summary>
    /// Sort tasks by their status.
    /// </summary>
    Status,
}
