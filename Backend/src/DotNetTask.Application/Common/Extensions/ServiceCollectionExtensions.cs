using System.Reflection;
using DotNetTask.Application.Abstractions.Behaviors;
using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Common.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetTask.Application.Common.Extensions;

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
        Assembly assembly = typeof(ServiceCollectionExtensions).Assembly;

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);

            config.AddOpenBehavior(typeof(ThrottlingBehavior<,>));

            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        // --- Custom Application Services ---
        services.AddScoped<ITaskAccessService, TaskAccessService>();
        services.AddScoped<IUniqueValueService, UniqueValueService>();
        services.AddScoped<IUserTaskAccessService, UserTaskAccessService>();
    }
}
