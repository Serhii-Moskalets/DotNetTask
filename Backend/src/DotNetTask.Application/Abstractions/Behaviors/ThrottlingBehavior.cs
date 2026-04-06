using System.Reflection;
using DotNetTask.Application.Abstractions.Interfaces.Services;
using DotNetTask.Application.Abstractions.Options;
using DotNetTask.Domain.Constants.Policy;
using MediatR;
using Microsoft.Extensions.Options;
using TinyResult;

namespace DotNetTask.Application.Abstractions.Behaviors;

/// <summary>
/// A MediatR pipeline behavior that enforces rate limiting on requests implementing <see cref="IThrottledRequest"/>.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled, which must implement <see cref="IThrottledRequest"/>.</typeparam>
/// <typeparam name="TResponse">The type of response returned by the handler.</typeparam>
public class ThrottlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
     where TRequest : IThrottledRequest
{
    private readonly IFloodProtectionService _floodService;
    private readonly ThrottlingSettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThrottlingBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="floodService">The service used to check and enforce flood protection rules.</param>
    /// <param name="options">The configuration options containing global or action-specific throttling settings.</param>
    public ThrottlingBehavior(IFloodProtectionService floodService, IOptions<ThrottlingSettings> options)
    {
        this._floodService = floodService;
        this._settings = options.Value;
    }

    /// <summary>
    /// Intercepts the request execution to verify if the requester has exceeded their rate limit.
    /// </summary>
    /// <param name="request">The incoming request object.</param>
    /// <param name="next">The delegate for the next action in the mediator pipeline.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <typeparamref name="TResponse"/> containing a failure result if throttled,
    /// otherwise the result of the next handler in the pipeline.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the request is throttled but the response type cannot be cast to a failure <see cref="Result{T}"/>.
    /// </exception>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ThrottlingActionSettings? actionConfig = this._settings.GetSettingsByAction(request.ActionName);

        if (actionConfig is null)
        {
            return await next();
        }

        bool isAllowed = await this._floodService.IsAllowedAsync(
            request.GetIdentity(),
            request.ActionName,
            actionConfig.MaxAttempts,
            actionConfig.Window);

        if (!isAllowed)
        {
            return CreateFailureResult();
        }

        return await next();
    }

    private static TResponse CreateFailureResult()
    {
        if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            Type valueType = typeof(TResponse).GetGenericArguments()[0];

            MethodInfo? failureMethod = typeof(Result<>)
                .MakeGenericType(valueType)
                .GetMethod("Failure", [typeof(TinyResult.Enums.ErrorCode), typeof(string)]);

            if (failureMethod is not null)
            {
                object? result = failureMethod.Invoke(null, [TinyResult.Enums.ErrorCode.ValidationError, ThrottlingPolicy.TooManyAttemptsMessage]);
                return (TResponse)result!;
            }
        }

        throw new InvalidOperationException(ThrottlingPolicy.InvalidOperationMessage);
    }
}
