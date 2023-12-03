using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ndjson.AsyncStreams.AspNetCore.Http.HttpResults;

/// <summary>
/// An <see cref="IResult"/> that on execution will write the given <see cref="IAsyncEnumerable{T}"/> as NDJSON to the response.
/// </summary>
/// <typeparam name="T">The type of the values in async stream to be serialized.</typeparam>
public partial class NdjsonAsyncEnumerableHttpResult<T> : IResult, IStatusCodeHttpResult, IValueHttpResult, IValueHttpResult<IAsyncEnumerable<T>>, IContentTypeHttpResult
{
    private static readonly string CONTENT_TYPE = new MediaTypeHeaderValue("application/x-ndjson")
    {
        Encoding = Encoding.UTF8
    }.ToString();

    private static readonly byte[] _newlineDelimiter = Encoding.UTF8.GetBytes("\n");

    /// <summary>
    /// Gets the object result.
    /// </summary>
    public IAsyncEnumerable<T>? Value { get; }

    object? IValueHttpResult.Value => Value;

    /// <summary>
    /// Gets the HTTP status code.
    /// </summary>
    public int? StatusCode { get; }

    /// <summary>
    /// Gets the value for the <c>Content-Type</c> header.
    /// </summary>
    public string ContentType { get; } = CONTENT_TYPE;

    /// <summary>
    /// Gets or sets the serializer settings.
    /// </summary>
    public JsonSerializerOptions? JsonSerializerOptions { get; internal init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NdjsonAsyncEnumerableHttpResult{T}"/> class with the values.
    /// </summary>
    /// <param name="value">The async stream of values to be serialized to the response.</param>
    /// <param name="jsonSerializerOptions">The serializer settings.</param>
    internal NdjsonAsyncEnumerableHttpResult(IAsyncEnumerable<T>? value, JsonSerializerOptions? jsonSerializerOptions)
        : this(value, statusCode: null, jsonSerializerOptions: jsonSerializerOptions)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="NdjsonAsyncEnumerableHttpResult{T}"/> class with the values.
    /// </summary>
    /// <param name="value">The async stream of values to be serialized to the response.</param>
    /// <param name="jsonSerializerOptions">The serializer settings.</param>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    internal NdjsonAsyncEnumerableHttpResult(IAsyncEnumerable<T>? value, JsonSerializerOptions? jsonSerializerOptions, int? statusCode)
    {
        Value = value;
        StatusCode = statusCode;
        JsonSerializerOptions = jsonSerializerOptions;        
    }

    /// <inheritdoc/>
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var loggerFactory = httpContext.RequestServices?.GetService<ILoggerFactory>();
        var logger = loggerFactory?.CreateLogger("Ndjson.AsyncStreams.AspNetCore.Http.Result.NdjsonAsyncEnumerableHttpResult") ?? NullLogger.Instance;

        SetStatusCode(httpContext, logger);
        SetContentType(httpContext, logger);
        DisableResponseBuffering(httpContext, logger);

        if (Value is null)
        {
            return;
        }

        JsonSerializerOptions jsonSerializerOptions = JsonSerializerOptions ?? ResolveJsonOptions(httpContext).SerializerOptions;

        try
        {
            Log.WritingAsyncEnumerableAsNdjson(logger);

            await foreach (T value in Value.WithCancellation(httpContext.RequestAborted))
            {
                await WriteAsyncEnumerableValue(value, jsonSerializerOptions, httpContext.Response.Body, httpContext.RequestAborted);
            }
        }
        catch (OperationCanceledException) when (httpContext.RequestAborted.IsCancellationRequested) { }
    }

    private void SetStatusCode(HttpContext httpContext, ILogger logger)
    {
        if (StatusCode is { } statusCode)
        {
            Log.SettingStatusCode(logger, statusCode);
            httpContext.Response.StatusCode = statusCode;
        }
    }

    private static void SetContentType(HttpContext httpContext, ILogger logger)
    {
        Log.SettingContentType(logger, CONTENT_TYPE);
        httpContext.Response.ContentType = CONTENT_TYPE;
    }

    private static void DisableResponseBuffering(HttpContext httpContext, ILogger logger)
    {
        IHttpResponseBodyFeature? responseBodyFeature = httpContext.Features.Get<IHttpResponseBodyFeature>();
        if (responseBodyFeature is not null)
        {
            Log.DisablingResponseBuffering(logger);
            responseBodyFeature.DisableBuffering();
        }
    }

    private static JsonOptions ResolveJsonOptions(HttpContext httpContext)
    {
        return httpContext.RequestServices.GetService<IOptions<JsonOptions>>()?.Value ?? new JsonOptions();
    }

    private static async Task WriteAsyncEnumerableValue(T value, JsonSerializerOptions jsonSerializerOptions, Stream writeStream, CancellationToken cancellationToken)
    {
        await JsonSerializer.SerializeAsync<T>(writeStream, value, jsonSerializerOptions, cancellationToken);
        await writeStream.WriteAsync(_newlineDelimiter, cancellationToken);
        await writeStream.FlushAsync(cancellationToken);
    }

    internal static partial class Log
    {
        [LoggerMessage(1, LogLevel.Information, "Setting HTTP status code {StatusCode}.", EventName = "SettingStatusCode")]
        public static partial void SettingStatusCode(ILogger logger, int statusCode);

        [LoggerMessage(2, LogLevel.Information, "Setting Content-Type header to {ContentType}.", EventName = "SettingContentType")]
        public static partial void SettingContentType(ILogger logger, string contentType);

        [LoggerMessage(3, LogLevel.Information, "Disabling response buffering.", EventName = "DisablingResponseBuffering")]
        public static partial void DisablingResponseBuffering(ILogger logger);

        [LoggerMessage(4, LogLevel.Information, "Writing values as NDJSON.", EventName = "WritingAsyncEnumerableAsNdjson")]
        public static partial void WritingAsyncEnumerableAsNdjson(ILogger logger);
    }
}
