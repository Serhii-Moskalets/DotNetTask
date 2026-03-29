using DotNetTask.Application.Abstractions.Interfaces.Common;
using DotNetTask.Application.Abstractions.Interfaces.DotNetTaskDbContext;
using DotNetTask.Application.Abstractions.Interfaces.Notifications;
using DotNetTask.Application.Abstractions.Interfaces.Repositories;
using DotNetTask.Application.Abstractions.Interfaces.Security;
using DotNetTask.Application.Abstractions.Interfaces.UnitOfWork;
using DotNetTask.Application.Common.Settings;
using DotNetTask.Domain.Constants;
using DotNetTask.Infrastructure.Notifications.Services;
using DotNetTask.Infrastructure.Notifications.Settings;
using DotNetTask.Infrastructure.Persistence.DatabaseContext;
using DotNetTask.Infrastructure.Persistence.Options;
using DotNetTask.Infrastructure.Persistence.Repositories;
using DotNetTask.Infrastructure.Persistence.UnitOfWork;
using DotNetTask.Infrastructure.Security;
using DotNetTask.Infrastructure.Security.Settings;
using DotNetTask.Infrastructure.Services;
using DotNetTask.Infrastructure.Web.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotNetTask.Infrastructure.Extensions;

/// <summary>
/// Provides extension methods for registering infrastructure services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the infrastructure services to the specified <see cref="IServiceCollection"/>,
    /// including the <see cref="DotNetTaskDbContext"/> configured to use PostgreSQL.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="config">The application configuration to access connection strings and settings.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        DatabaseOptions dbOptions = config.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
            ?? new DatabaseOptions();

        services.AddDbContext<DotNetTaskDbContext>(options =>
        options.UseNpgsql(
            config.GetConnectionString(CommonPolicy.DataBaseConnectionString),
            npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: dbOptions.MaxRetryCount,
                maxRetryDelay: TimeSpan.FromSeconds(dbOptions.MaxRetryDelaySeconds),
                errorCodesToAdd: null);

            npgsqlOptions.CommandTimeout(dbOptions.CommandTimeout);
        }));

        services.AddScoped<IDotNetTaskDbContext>(provider =>
            provider.GetRequiredService<DotNetTaskDbContext>());

        // --- Add System time ---
        services.AddSingleton<IClock, SystemClock>();

        // --- Add all repository ---
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<ITaskListRepository, TaskListRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserTaskAccessRepository, UserTaskAccessRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // --- Add Email services ---
        services.Configure<EmailSettings>(config.GetSection(EmailSettings.SectionName));
        services.AddTransient<IEmailSender, EmailSender>();
        services.AddSingleton<IEmailTemplateProvider, EmailTemplateProvider>();
        services.AddScoped<IEmailService, EmailService>();

        // --- Add URL Provider ---
        services.AddScoped<IUrlProvider, UrlProvider>();

        // --- Add Security services ---
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenGenerator, TokenGenerator>();

        // --- Add Jwt Token service ---
        services.AddOptions<JwtSettings>()
            .Bind(config.GetSection(JwtSettings.SectionName))
            .Validate(
            settings =>
            {
                return !string.IsNullOrWhiteSpace(settings.Secret) &&
                       settings.Secret.Length >= 32 &&
                       !string.IsNullOrWhiteSpace(settings.Issuer) &&
                       !string.IsNullOrWhiteSpace(settings.Audience);
            }, "JWT Settings are invalid: Secret (min 32 chars), Issuer and Audience are required.")
        .ValidateOnStart();

        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        // --- Add ApiEndpointOptions ---
        services.Configure<ApiEndpointOptions>(config.GetSection(ApiEndpointOptions.SectionName));

        // -- Add TokenOptions ---
        services.Configure<TokenOptions>(config.GetSection(TokenOptions.SectionName));
        services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<TokenOptions>>().Value);

        return services;
    }
}
