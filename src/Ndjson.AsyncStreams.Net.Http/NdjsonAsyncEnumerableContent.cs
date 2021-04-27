using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Ndjson.AsyncStreams.Net.Http
{
    /// <summary>
    /// Provides HTTP content based on NDJSON and <see cref="IAsyncEnumerable{T}"/>.
    /// </summary>
    public class NdjsonAsyncEnumerableContent : HttpContent
    {
        private static readonly byte[] _newlineDelimiter = Encoding.UTF8.GetBytes("\n");

#if NETCOREAPP3_1
        private static readonly JsonSerializerOptions _defaultJsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy .CamelCase
        };
#endif

#if NET5_0
        private static readonly JsonSerializerOptions _defaultJsonSerializerOptions = new(JsonSerializerDefaults.Web);
#endif

        private readonly JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        /// Gets the type of the values in async stream to be serialized by this instance.
        /// </summary>
        public Type ValuesType { get; }

        /// <summary>
        /// Gets the async stream of values to be serialized and used as the body of the <see cref="HttpRequestMessage"/> that sends this instance.
        /// </summary>
        public IAsyncEnumerable<object> Values { get;  }

        private NdjsonAsyncEnumerableContent(IAsyncEnumerable<object> values, Type valuesType, JsonSerializerOptions? jsonSerializerOptions = null)
        {
            Values = values ?? throw new ArgumentNullException(nameof(values));
            ValuesType = valuesType ?? throw new ArgumentNullException(nameof(valuesType));

            _jsonSerializerOptions = jsonSerializerOptions ?? _defaultJsonSerializerOptions;

            Headers.ContentType = new MediaTypeHeaderValue("application/x-ndjson")
            {
                CharSet = Encoding.UTF8.WebName
            };
        }

        /// <summary>
        /// Creates a new instance of the <see cref="NdjsonAsyncEnumerableContent"/> class that will contain the values async stream serialized as NDJSON.
        /// </summary>
        /// <typeparam name="T">The type of the values in async stream to be serialized.</typeparam>
        /// <param name="values">The async stream of values to be serialized.</param>
        /// <param name="options">The options to control the behavior during serialization.</param>
        /// <returns>A <see cref="NdjsonAsyncEnumerableContent"/> instance.</returns>
        public static NdjsonAsyncEnumerableContent Create<T>(IAsyncEnumerable<T> values, JsonSerializerOptions? options = null)
            => new ((IAsyncEnumerable<object>)values, typeof(T), options);

        /// <inheritdoc/>
        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            await foreach (object value in Values.ConfigureAwait(false))
            {
                await JsonSerializer.SerializeAsync(stream, value, ValuesType, _jsonSerializerOptions).ConfigureAwait(false);
                await stream.WriteAsync(_newlineDelimiter).ConfigureAwait(false);
                await stream.FlushAsync().ConfigureAwait(false);
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
