using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ndjson.AsyncStreams.Net.Http.Internals;

namespace Ndjson.AsyncStreams.Net.Http
{
    /// <summary>
    /// Provides <see cref="HttpContent" /> based on NDJSON/JSONL and <see cref="IAsyncEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the values in async stream to be serialized.</typeparam>
    public class NdjsonAsyncEnumerableContent<T> : HttpContent
    {
        private static readonly byte[] _newlineDelimiter = Encoding.UTF8.GetBytes("\n");
        private static readonly JsonSerializerOptions _defaultJsonSerializerOptions = new(JsonSerializerDefaults.Web);

        private readonly JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        /// Gets the async stream of values to be serialized and used as the body of the <see cref="HttpRequestMessage"/> that sends this instance.
        /// </summary>
        public IAsyncEnumerable<T> Values { get;  }

        /// <summary>
        /// Creates a new instance of the <see cref="NdjsonAsyncEnumerableContent{T}"/> class that will contain the values async stream serialized as NDJSON.
        /// </summary>
        /// <param name="values">The async stream of values to be serialized.</param>
        /// <param name="options">The options to control the behavior during serialization.</param>
        public NdjsonAsyncEnumerableContent(IAsyncEnumerable<T> values, JsonSerializerOptions? options = null)
            : this(values, MediaTypeHeaderValues.APPLICATION_NDJSON_MEDIA_TYPE, options) { }

        /// <summary>
        /// Creates a new instance of the <see cref="NdjsonAsyncEnumerableContent{T}"/> class that will contain the values async stream serialized as NDJSON.
        /// </summary>
        /// <param name="values">The async stream of values to be serialized.</param>
        /// <param name="mediaType">The media type to be used.</param>
        /// <param name="options">The options to control the behavior during serialization.</param>
        public NdjsonAsyncEnumerableContent(IAsyncEnumerable<T> values, string mediaType, JsonSerializerOptions? options = null)
        {
            Values = values ?? throw new ArgumentNullException(nameof(values));

            if (!MediaTypeHeaderValues.IsSupportedMediaType(mediaType))
            {
                throw new NotSupportedException($"Not supported media type {mediaType}.");
            }

            Headers.ContentType = new MediaTypeHeaderValue(mediaType)
            {
                CharSet = Encoding.UTF8.WebName
            };

            _jsonSerializerOptions = options ?? _defaultJsonSerializerOptions;
        }

        /// <inheritdoc/>
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            return SerializeToStreamAsync(stream, context, CancellationToken.None);
        }

        /// <inheritdoc/>
        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken)
        {
            await foreach (T value in Values.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                await JsonSerializer.SerializeAsync<T>(stream, value, _jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
                await stream.WriteAsync(_newlineDelimiter, cancellationToken).ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        protected override bool TryComputeLength(out long length)
        {
            length = -1;

            return false;
        }
    }
}
