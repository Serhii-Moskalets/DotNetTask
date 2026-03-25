using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DotNetTask.Api.Tests.Common;

/// <summary>
/// Provides extension methods for <see cref="HttpContext"/> to simplify unit testing.
/// </summary>
internal static class HttpContextExtensions
{
    /// <summary>
    /// Reads and deserializes the JSON response body into a <see cref="ProblemDetails"/> object.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> containing the response.</param>
    /// <returns>A <see cref="Task{ProblemDetails}"/> containing the deserialized problem details.</returns>
    /// <exception cref="Exception">Thrown when the response body cannot be deserialized.</exception>
    public static async Task<ProblemDetails> ReadProblemDetailsAsync(this HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        return await JsonSerializer.DeserializeAsync<ProblemDetails>(context.Response.Body)
            ?? throw new Exception("Response body is empty or not a valid ProblemDetails JSON.");
    }
}
