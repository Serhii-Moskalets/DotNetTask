using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TodoListApp.Application.Abstractions.Behaviors;
using TodoListApp.Application.Abstractions.Interfaces.Services;
using TodoListApp.Application.Common.Services;

namespace TodoListApp.Application.Common.Extensions;

/// <summary>
/// Provides extension methods for registering application layer services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers MediatR, FluentValidation, and custom application services.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    public static void AddApplicationServices(this IServiceCollection services)
    {
        var assembly = typeof(ServiceCollectionExtensions).Assembly;

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        // --- Custom Application Services ---
        services.AddScoped<ITaskAccessService, TaskAccessService>();
        services.AddScoped<IUniqueValueService, UniqueValueService>();
        services.AddScoped<IUserTaskAccessService, UserTaskAccessService>();
    }
}
