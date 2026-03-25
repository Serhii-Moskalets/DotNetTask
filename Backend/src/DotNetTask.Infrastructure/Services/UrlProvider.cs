using System.Collections.Specialized;
using DotNetTask.Application.Abstractions.Interfaces.Common;
using Microsoft.Extensions.Configuration;

namespace DotNetTask.Infrastructure.Services;

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
        string baseUrl = configuration["FrontendSettings:BaseUrl"]
            ?? throw new InvalidOperationException("Frontend BaseUrl is not configured.");

        this._frontendBaseUrl = baseUrl.TrimEnd('/');
    }

    /// <inheritdoc />
    public string GetEmailChangeLink(string token)
    {
        return this.BuildUrl("confirm-email-change", new Dictionary<string, string>
            {
                { "token", token },
            });
    }

    /// <inheritdoc />
    public string GetEmailConfirmationLink(string token)
    {
        return this.BuildUrl("confirm-email", new Dictionary<string, string>
            {
                { "token", token },
            });
    }

    /// <inheritdoc />
    public string GetEmailRevertLink(string token)
    {
        return this.BuildUrl("revert-email-change", new Dictionary<string, string>
        {
            { "token", token },
        });
    }

    /// <inheritdoc />
    public string GetPasswordResetLink(string token)
    {
        return this.BuildUrl("reset-password", new Dictionary<string, string>
        {
            { "token", token },
        });
    }

    /// <summary>
    /// Helper method to build a secure URL with query parameters.
    /// </summary>
    private string BuildUrl(string path, Dictionary<string, string> queryParams)
    {
        UriBuilder builder = new(this._frontendBaseUrl);

        if (builder.Uri.IsDefaultPort)
        {
            builder.Port = -1;
        }

        builder.Path = builder.Path.TrimEnd('/') + "/" + path.TrimStart('/');

        NameValueCollection query = System.Web.HttpUtility.ParseQueryString(string.Empty);
        foreach (KeyValuePair<string, string> param in queryParams)
        {
            query[param.Key] = param.Value;
        }

        builder.Query = query.ToString();

        return builder.ToString();
    }
}
