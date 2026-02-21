using Microsoft.Extensions.Configuration;
using TodoListApp.Application.Abstractions.Interfaces.Common;

namespace TodoListApp.Infrastructure.Services;

/// <summary>
/// Provides concrete implementation for generating application URLs
/// based on the frontend base address configured in the application settings.
/// </summary>
public class UrlProvider : IUrlProvider
{
    private readonly string _frontendBaseUrl;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlProvider"/> class.
    /// </summary>
    /// <param name="configuration">The application configuration used to retrieve settings.</param>
    /// <exception cref="InvalidOperationException">Thrown when the 'FrontendSettings:BaseUrl' is missing in configuration.</exception>
    public UrlProvider(IConfiguration configuration)
    {
        var baseUrl = configuration["FrontendSettings:BaseUrl"]
            ?? throw new InvalidOperationException("Frontend BaseUrl is not configured.");

        this._frontendBaseUrl = baseUrl.TrimEnd('/');
    }

    /// <inheritdoc />
    public string GetEmailChangeLink(Guid userId, string token, string newEmail)
        => this.BuildUrl("confirm-email-change", new Dictionary<string, string>
            {
                { "userId", userId.ToString() },
                { "token", token },
                { "newEmail", newEmail },
            });

    /// <inheritdoc />
    public string GetEmailConfirmationLink(Guid userId, string token)
        => this.BuildUrl("confirm-email", new Dictionary<string, string>
            {
                { "userId", userId.ToString() },
                { "token", token },
            });

    /// <inheritdoc />
    public string GetEmailRevertLink(Guid userId, string token)
        => this.BuildUrl("revert-confirm-email", new Dictionary<string, string>
        {
            { "userId", userId.ToString() },
            { "token", token },
        });

    /// <inheritdoc />
    public string GetPasswordResetLink(Guid userId, string token)
        => this.BuildUrl("reset-password", new Dictionary<string, string>
        {
            { "userId", userId.ToString() },
            { "token", token },
        });

    /// <summary>
    /// Helper method to build a secure URL with query parameters.
    /// </summary>
    private string BuildUrl(string path, Dictionary<string, string> queryParams)
    {
        var builder = new UriBuilder(this._frontendBaseUrl);

        if (builder.Uri.IsDefaultPort)
        {
            builder.Port = -1;
        }

        builder.Path = builder.Path.TrimEnd('/') + "/" + path.TrimStart('/');

        var query = System.Web.HttpUtility.ParseQueryString(string.Empty);
        foreach (var param in queryParams)
        {
            query[param.Key] = param.Value;
        }

        builder.Query = query.ToString();

        return builder.ToString();
    }
}
