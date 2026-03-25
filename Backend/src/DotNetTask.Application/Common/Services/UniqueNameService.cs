using DotNetTask.Application.Abstractions.Interfaces.Services;

namespace DotNetTask.Application.Common.Services;

/// <summary>
/// Provides a generic implementation for generating unique domain values
/// by appending a numeric suffix in the format "BaseName (N)".
/// </summary>
public class UniqueValueService : IUniqueValueService
{
    /// <inheritdoc />
    /// <remarks>
    /// This implementation starts with the <paramref name="baseName"/> and iteratively
    /// appends a suffix starting from 1 (e.g., "Name (1)") until the
    /// <paramref name="existsCheck"/> returns <see langword="false"/>.
    /// </remarks>
    public async Task<TValueObject> GetUniqueValueAsync<TValueObject>(
        string baseName,
        Func<string, TValueObject> factory,
        Func<TValueObject, CancellationToken, Task<bool>> existsCheck,
        CancellationToken cancellationToken)
        where TValueObject : class
    {
        int suffix = 1;
        TValueObject currentValueObject = factory(baseName);

        while (await existsCheck(currentValueObject, cancellationToken))
        {
            string newName = $"{baseName} ({suffix++})";
            currentValueObject = factory(newName);
        }

        return currentValueObject;
    }
}
