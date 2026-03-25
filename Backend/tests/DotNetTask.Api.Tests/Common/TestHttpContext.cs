using Microsoft.AspNetCore.Http;

namespace DotNetTask.Api.Tests.Common;

/// <summary>
/// A wrapper around <see cref="DefaultHttpContext"/> that manages a <see cref="MemoryStream"/>
/// for testing response bodies.
/// </summary>
internal sealed class TestHttpContext : IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestHttpContext"/> class.
    /// </summary>
    public TestHttpContext()
    {
        this.BodyStream = new MemoryStream();
        this.Context = new DefaultHttpContext();
        this.Context.Response.Body = this.BodyStream;
    }

    /// <summary>
    /// Gets the underlying <see cref="DefaultHttpContext"/>.
    /// </summary>
    public DefaultHttpContext Context { get; }

    /// <summary>
    /// Gets the <see cref="MemoryStream"/> associated with the response body.
    /// </summary>
    public MemoryStream BodyStream { get; }

    /// <summary>
    /// Disposes the underlying <see cref="MemoryStream"/>.
    /// </summary>
    public void Dispose()
    {
        this.BodyStream.Dispose();
    }
}
