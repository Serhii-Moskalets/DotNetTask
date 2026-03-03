namespace TodoListApp.Application.Abstractions.Interfaces.Notifications;

/// <summary>
/// Defines a provider for loading and processing email templates.
/// </summary>
public interface IEmailTemplateProvider
{
    /// <summary>
    /// Loads an HTML template and replaces placeholders.
    /// </summary>
    /// <param name="templateName">Name of the template file.</param>
    /// <param name="placeholders">Dictionary of keys and values to replace.</param>
    /// <returns>Processed HTML content.</returns>
    Task<string> GetEmailTemplateAsync(string templateName, Dictionary<string, string> placeholders);
}
