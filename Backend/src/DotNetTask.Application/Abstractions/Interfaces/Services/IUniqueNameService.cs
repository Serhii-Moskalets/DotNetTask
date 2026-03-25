namespace DotNetTask.Application.Abstractions.Interfaces.Services;

/// <summary>
/// Defines a service for generating unique values (typically Value Objects)
/// by appending a numeric suffix if the initial value already exists.
/// </summary>
public interface IUniqueValueService
{
    /// <summary>
    /// Generates a unique Value Object based on the provided base name and a uniqueness check.
    /// </summary>
    /// <typeparam name="TValueObject">The type of the Value Object to be returned.</typeparam>
    /// <param name="baseName">The original string to start with (e.g., "Work").</param>
    /// <param name="factory">A delegate that creates an instance of <typeparamref name="TValueObject"/> from a string.</param>
    /// <param name="existsCheck">
    /// A delegate that checks if the generated <typeparamref name="TValueObject"/> already exists in the system.
    /// Returns <see langword="true"/> if the value is taken; otherwise, <see langword="false"/>.
    /// </param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The result contains a unique instance of <typeparamref name="TValueObject"/>.
    /// </returns>
    Task<TValueObject> GetUniqueValueAsync<TValueObject>(
        string baseName,
        Func<string, TValueObject> factory,
        Func<TValueObject, CancellationToken, Task<bool>> existsCheck,
        CancellationToken cancellationToken)
        where TValueObject : class;
}
