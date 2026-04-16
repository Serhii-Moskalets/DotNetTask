using System.Collections.Specialized;
using DotNetTask.Application.Abstractions.Interfaces.Common;
using DotNetTask.Infrastructure.Notifications.Options;
using Microsoft.Extensions.Options;

namespace DotNetTask.Infrastructure.Services;

/// <summary>
/// Provides concrete implementation for generating application URLs
/// based on the frontend base address configured in the application settings.
/// </summary>
public class UrlProvider : IUrlProvider
{
    private readonly string _frontendBaseUrl;
    private readonly FrontendSettingsOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlProvider"/> class.
    /// </summary>
    /// /// <param name="options">The frontend settings containing the base URL.</param>
    /// <exception cref="InvalidOperationException">Thrown when the 'FrontendSettings:BaseUrl' is missing in configuration.</exception>
    public UrlProvider(IOptions<FrontendSettingsOptions> options)
    {
        string baseUrl = options.Value.BaseUrl
            ?? throw new InvalidOperationException("Frontend BaseUrl is not configured.");

        this._options = options.Value;
        this._frontendBaseUrl = baseUrl.TrimEnd('/');
    }

    /// <inheritdoc />
    public string GetEmailChangeLink(string token)
    {
        return this.BuildUrl(this._options.EmailChangeConfirmationPath, new Dictionary<string, string>
            {
                { "token", token },
            });
    }

    /// <inheritdoc />
    public string GetEmailConfirmationLink(string token)
    {
        return this.BuildUrl(this._options.EmailConfirmationPath, new Dictionary<string, string>
            {
                { "token", token },
            });
    }

    /// <inheritdoc />
    public string GetEmailRevertLink(string token)
    {
        return this.BuildUrl(this._options.EmailRevertPath, new Dictionary<string, string>
        {
            { "token", token },
        });
    }

    /// <inheritdoc />
    public string GetPasswordResetLink(string token)
    {
        return this.BuildUrl(this._options.PasswordResetPath, new Dictionary<string, string>
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
