using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TodoListApp.Application.Abstractions.Interfaces.Common;
using TodoListApp.Application.Abstractions.Interfaces.Notifications;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.Security;
using TodoListApp.Application.Abstractions.Interfaces.TodoListAppDbContext;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Domain.Constants;
using TodoListApp.Infrastructure.Notifications.Services;
using TodoListApp.Infrastructure.Notifications.Settings;
using TodoListApp.Infrastructure.Persistence.DatabaseContext;
using TodoListApp.Infrastructure.Persistence.Repositories;
using TodoListApp.Infrastructure.Persistence.UnitOfWork;
using TodoListApp.Infrastructure.Security;
using TodoListApp.Infrastructure.Security.Settings;
using TodoListApp.Infrastructure.Services;

namespace TodoListApp.Infrastructure.Extensions;

/// <summary>
/// Provides extension methods for registering infrastructure services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the infrastructure services to the specified <see cref="IServiceCollection"/>,
    /// including the <see cref="TodoListAppDbContext"/> configured to use PostgreSQL.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="config">The application configuration to access connection strings and settings.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString(CommonPolicy.DataBaseConnectionString)
                ?? throw new InvalidOperationException(CommonPolicy.MissingConnectionStringMessage);

        services.AddDbContext<TodoListAppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ITodoListAppDbContext>(provider =>
            provider.GetRequiredService<TodoListAppDbContext>());

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

        return services;
    }
}
