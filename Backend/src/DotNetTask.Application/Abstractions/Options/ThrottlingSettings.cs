namespace DotNetTask.Application.Abstractions.Options;

/// <summary>
/// Contains the configuration settings for application-wide throttling and rate-limiting policies.
/// </summary>
/// <remarks>
/// This class maps to the "ThrottlingSettings" section in the configuration provider (e.g., appsettings.json).
/// </remarks>
public class ThrottlingSettings
{
    /// <summary>
    /// The configuration section name used for binding in the startup configuration.
    /// </summary>
    public const string SectionName = "ThrottlingSettings";

    /// <summary>
    /// Gets the default throttling configuration specifically for requests.
    /// </summary>
    public ThrottlingActionSettings DefaultLimit { get; init; } = new()
    {
        Window = TimeSpan.FromMinutes(1),
        MaxAttempts = 20,
    };

    /// <summary>
    /// Gets a dictionary of specific throttling configurations for various actions.
    /// </summary>
    /// <value>
    /// A <see cref="Dictionary{TKey, TValue}"/> where the key is the action name (e.g., "DeleteTask")
    /// and the value is the corresponding <see cref="ThrottlingActionSettings"/>.
    /// </value>
    /// <remarks>
    /// This dictionary is used as a fallback or alternative to explicit properties in the
    /// <see cref="ThrottlingSettings"/> class. It allows for dynamic configuration via
    /// the <c>Actions</c> section in <c>appsettings.json</c> without requiring code changes.
    /// </remarks>
    public Dictionary<string, ThrottlingActionSettings> Actions { get; init; } = [];

    /// <summary>
    /// Resolves the appropriate <see cref="ThrottlingActionSettings"/> by checking the <see cref="Actions"/>
    /// dictionary, falling back to <see cref="DefaultLimit"/> if not found.
    /// </summary>
    /// <param name="actionName">The name of the action to retrieve (case-insensitive).</param>
    /// <returns>
    /// A <see cref="ThrottlingActionSettings"/> instance;
    /// returns <see langword="null"/> if <paramref name="actionName"/> is null or empty.
    /// </returns>
    public ThrottlingActionSettings? GetSettingsByAction(string actionName)
    {
        if (string.IsNullOrWhiteSpace(actionName))
        {
            return null;
        }

        if (this.Actions.TryGetValue(actionName, out ThrottlingActionSettings? settings))
        {
            return settings;
        }

        return new ThrottlingActionSettings
        {
            ActionName = actionName,
            Window = this.DefaultLimit.Window,
            MaxAttempts = this.DefaultLimit.MaxAttempts,
        };

    }
}
