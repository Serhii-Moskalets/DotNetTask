using System.Collections.Concurrent;
using System.Net;
using System.Text;
using DotNetTask.Application.Abstractions.Interfaces.Notifications;

namespace DotNetTask.Infrastructure.Notifications.Services;

/// <summary>
/// Responsible for loading and processing HTML email templates from the local file system.
/// </summary>
public class EmailTemplateProvider : IEmailTemplateProvider
{
    private static readonly ConcurrentDictionary<string, string> TemplateCache = [];

    /// <summary>
    /// Loads an HTML template file and replaces defined placeholders with actual values.
    /// </summary>
    /// <param name="templateName">The name of the template file (without extension).</param>
    /// <param name="placeholders">A dictionary of key-value pairs to replace in the template (e.g., {{USER_NAME}}).</param>
    /// <returns>A processed string containing the final HTML content.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the specified template file does not exist.</exception>
    public async Task<string> GetEmailTemplateAsync(string templateName, Dictionary<string, string> placeholders)
    {
        if (!TemplateCache.TryGetValue(templateName, out string? originalTemplate))
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Notifications", "Templates", $"{templateName}.html");
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Email template not found at: {path}");
            }

            originalTemplate = await File.ReadAllTextAsync(path);
            TemplateCache[templateName] = originalTemplate;
        }

        StringBuilder processedContent = new(originalTemplate);

        foreach (KeyValuePair<string, string> item in placeholders)
        {
            string safeValue = WebUtility.HtmlEncode(item.Value);
            processedContent.Replace($"{{{{{item.Key}}}}}", safeValue);
        }

        return processedContent.ToString();
    }
}
