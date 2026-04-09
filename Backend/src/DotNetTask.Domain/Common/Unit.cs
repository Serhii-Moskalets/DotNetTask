namespace DotNetTask.Domain.Common;

/// <summary>
/// Represents a type that has only one value.
/// </summary>
public readonly struct Unit
{
    /// <summary>
    /// The single instance of the <see cref="Unit"/> value.
    /// </summary>
    public static readonly Unit Value = default;
}
