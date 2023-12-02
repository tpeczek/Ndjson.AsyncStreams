using System.Text.Json;
using System.Collections.Generic;
using Ndjson.AsyncStreams.AspNetCore.Http.HttpResults;

namespace Microsoft.AspNetCore.Http;

/// <summary>
/// An extension for <see cref="Results"/> to provide NDJSON related IResult instances.
/// </summary>
public static partial class NdjsonResultExtensions
{
    /// <summary>
    /// Creates a <see cref="IResult"/> that on execution will write the given <see cref="IAsyncEnumerable{T}"/> as NDJSON to the response.
    /// </summary>
    /// <param name="resultExtensions">The interface for registering external method that provides <see cref="NdjsonAsyncEnumerableHttpResult{T}"/> instance.</param>
    /// <param name="stream">The async stream of values to write as NDJSON.</param>
    /// <param name="options">The serializer options to use when serializing the values.</param>
    /// <param name="statusCode">The status code to set on the response.</param>
    /// <returns>The created <see cref="NdjsonAsyncEnumerableHttpResult{T}"/> that on execution will write the given <paramref name="stream"/> as NDJSON to the response.</returns>
    /// <remarks>Callers should cache an instance of serializer settings to avoid recreating cached data with each call.</remarks>
    public static IResult Ndjson<T>(this IResultExtensions resultExtensions, IAsyncEnumerable<T>? stream, JsonSerializerOptions? options = null, int? statusCode = null)
    {
        return new NdjsonAsyncEnumerableHttpResult<T>(stream, options, statusCode);
    }
}
