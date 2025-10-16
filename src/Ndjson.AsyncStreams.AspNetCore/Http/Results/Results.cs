using System.Text.Json;
using System.Collections.Generic;
using Ndjson.AsyncStreams.AspNetCore.Http.HttpResults;

namespace Microsoft.AspNetCore.Http;

/// <summary>
/// An extension for <see cref="Results"/> to provide NDJSON related IResult instances.
/// </summary>
public static partial class NdjsonResultExtensions
{
    private const string NDJSON_CONTENT_TYPE = "application/x-ndjson";
    private const string JSONL_CONTENT_TYPE = "application/jsonl";

    /// <summary>
    /// Creates a <see cref="IResult"/> that on execution will write the given <see cref="IAsyncEnumerable{T}"/> as NDJSON to the response.
    /// </summary>
    /// <param name="resultExtensions">The interface for registering external method that provides <see cref="NdjsonAsyncEnumerableHttpResult{T}"/> instance.</param>
    /// <param name="stream">The async stream of values to write as NDJSON.</param>
    /// <param name="options">The serializer options to use when serializing the values.</param>
    /// <param name="statusCode">The status code to set on the response.</param>
    /// <typeparam name="T">The type of the values in async stream to be serialized.</typeparam>
    /// <returns>The created <see cref="NdjsonAsyncEnumerableHttpResult{T}"/> that on execution will write the given <paramref name="stream"/> as NDJSON to the response.</returns>
    /// <remarks>Callers should cache an instance of serializer settings to avoid recreating cached data with each call.</remarks>
    public static IResult Ndjson<T>(this IResultExtensions resultExtensions, IAsyncEnumerable<T>? stream, JsonSerializerOptions? options = null, int? statusCode = null)
    {
        return new NdjsonAsyncEnumerableHttpResult<T>(stream, NDJSON_CONTENT_TYPE, options, statusCode);
    }

    /// <summary>
    /// Creates a <see cref="IResult"/> that on execution will write the given <see cref="IAsyncEnumerable{T}"/> as JSONL to the response.
    /// </summary>
    /// <param name="resultExtensions">The interface for registering external method that provides <see cref="NdjsonAsyncEnumerableHttpResult{T}"/> instance.</param>
    /// <param name="stream">The async stream of values to write as JSONL.</param>
    /// <param name="options">The serializer options to use when serializing the values.</param>
    /// <param name="statusCode">The status code to set on the response.</param>
    /// <typeparam name="T">The type of the values in async stream to be serialized.</typeparam>
    /// <returns>The created <see cref="NdjsonAsyncEnumerableHttpResult{T}"/> that on execution will write the given <paramref name="stream"/> as JSONL to the response.</returns>
    /// <remarks>Callers should cache an instance of serializer settings to avoid recreating cached data with each call.</remarks>
    public static IResult Jsonl<T>(this IResultExtensions resultExtensions, IAsyncEnumerable<T>? stream, JsonSerializerOptions? options = null, int? statusCode = null)
    {
        return new NdjsonAsyncEnumerableHttpResult<T>(stream, JSONL_CONTENT_TYPE, options, statusCode);
    }
}
