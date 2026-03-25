using System.Reflection;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using TinyResult;
using TinyResult.Enums;

namespace DotNetTask.Application.Abstractions.Behaviors;

/// <summary>
/// A pipeline behavior that validates the request using FluentValidation before calling the handler.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response (expected to be a Result).</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="ValidationBehavior{TRequest, TResponse}"/> class.
/// </remarks>
/// <param name="validators">The collection of validators for the request.</param>
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next();
        }

        ValidationContext<TRequest> context = new(request);

        FluentValidation.Results.ValidationResult[] validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        List<ValidationFailure> failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            return CreateFailureResult(failures[0].ErrorMessage);
        }

        return await next();
    }

    /// <summary>
    /// Creates a failure result of the appropriate type using reflection.
    /// </summary>
    /// <param name="errorMessage">The error message to include in the result.</param>
    /// <returns>A failure result of type <typeparamref name="TResponse"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Failure method cannot be found.</exception>
    private static TResponse CreateFailureResult(string errorMessage)
    {
        Type resultType = typeof(TResponse).GetGenericArguments()[0];

        MethodInfo method = typeof(Result<>)
            .MakeGenericType(resultType)
            .GetMethod("Failure", [typeof(ErrorCode), typeof(string)])
            ?? throw new InvalidOperationException($"Could not find Failure method on Result<{resultType.Name}>");

        return (TResponse)method.Invoke(null, [ErrorCode.ValidationError, errorMessage])!;
    }
}
