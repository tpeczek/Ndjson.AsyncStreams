using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ndjson.AsyncStreams.Net.Http.Internals;

namespace Ndjson.AsyncStreams.Net.Http
{
    /// <summary>
    /// Contains extension methods to send and receive HTTP content as NDJSON.
    /// </summary>
    public static partial class HttpClientNdjsonExtensions
    {
        /// <summary>
        /// Sends a POST request to the specified Uri containing the values async stream serialized as NDJSON in the request body.
        /// </summary>
        /// <typeparam name="T">The type of the values in async stream to be serialized.</typeparam>
        /// <param name="client">The client used to send the request.</param>
        /// <param name="requestUri">The Uri the request is sent to.</param>
        /// <param name="values">The async stream of values to be serialized.</param>
        /// <param name="options">The options to control the behavior during serialization.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public static Task<HttpResponseMessage> PostAsNdjsonAsync<T>(this HttpClient client, string? requestUri, IAsyncEnumerable<T> values, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
            => client.PostAsStreamedJsonAsync(requestUri, values, MediaTypeHeaderValues.APPLICATION_NDJSON_MEDIA_TYPE, options, cancellationToken);

        /// <summary>
        /// Sends a POST request to the specified Uri containing the values async stream serialized as NDJSON in the request body.
        /// </summary>
        /// <typeparam name="T">The type of the values in async stream to be serialized.</typeparam>
        /// <param name="client">The client used to send the request.</param>
        /// <param name="requestUri">The Uri the request is sent to.</param>
        /// <param name="values">The async stream of values to be serialized.</param>
        /// <param name="options">The options to control the behavior during serialization.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public static Task<HttpResponseMessage> PostAsNdjsonAsync<T>(this HttpClient client, Uri? requestUri, IAsyncEnumerable<T> values, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
            => client.PostAsStreamedJsonAsync(requestUri, values, MediaTypeHeaderValues.APPLICATION_NDJSON_MEDIA_TYPE, options, cancellationToken);

        /// <summary>
        /// Sends a POST request to the specified Uri containing the values async stream serialized as NDJSON in the request body.
        /// </summary>
        /// <typeparam name="T">The type of the values in async stream to be serialized.</typeparam>
        /// <param name="client">The client used to send the request.</param>
        /// <param name="requestUri">The Uri the request is sent to.</param>
        /// <param name="values">The async stream of values to be serialized.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public static Task<HttpResponseMessage> PostAsNdjsonAsync<T>(this HttpClient client, string? requestUri, IAsyncEnumerable<T> values, CancellationToken cancellationToken)
            => client.PostAsStreamedJsonAsync(requestUri, values, MediaTypeHeaderValues.APPLICATION_NDJSON_MEDIA_TYPE, options: null, cancellationToken);

        /// <summary>
        /// Sends a POST request to the specified Uri containing the values async stream serialized as NDJSON in the request body.
        /// </summary>
        /// <typeparam name="T">The type of the values in async stream to be serialized.</typeparam>
        /// <param name="client">The client used to send the request.</param>
        /// <param name="requestUri">The Uri the request is sent to.</param>
        /// <param name="values">The async stream of values to be serialized.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public static Task<HttpResponseMessage> PostAsNdjsonAsync<T>(this HttpClient client, Uri? requestUri, IAsyncEnumerable<T> values, CancellationToken cancellationToken)
            => client.PostAsStreamedJsonAsync(requestUri, values, MediaTypeHeaderValues.APPLICATION_NDJSON_MEDIA_TYPE, options: null, cancellationToken);

        /// <summary>
        /// Sends a POST request to the specified Uri containing the values async stream serialized as JSONL in the request body.
        /// </summary>
        /// <typeparam name="T">The type of the values in async stream to be serialized.</typeparam>
        /// <param name="client">The client used to send the request.</param>
        /// <param name="requestUri">The Uri the request is sent to.</param>
        /// <param name="values">The async stream of values to be serialized.</param>
        /// <param name="options">The options to control the behavior during serialization.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public static Task<HttpResponseMessage> PostAsJsonlAsync<T>(this HttpClient client, string? requestUri, IAsyncEnumerable<T> values, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
            => client.PostAsStreamedJsonAsync(requestUri, values, MediaTypeHeaderValues.APPLICATION_JSONL_MEDIA_TYPE, options, cancellationToken);

        /// <summary>
        /// Sends a POST request to the specified Uri containing the values async stream serialized as JSONL in the request body.
        /// </summary>
        /// <typeparam name="T">The type of the values in async stream to be serialized.</typeparam>
        /// <param name="client">The client used to send the request.</param>
        /// <param name="requestUri">The Uri the request is sent to.</param>
        /// <param name="values">The async stream of values to be serialized.</param>
        /// <param name="options">The options to control the behavior during serialization.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public static Task<HttpResponseMessage> PostAsJsonlAsync<T>(this HttpClient client, Uri? requestUri, IAsyncEnumerable<T> values, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
            => client.PostAsStreamedJsonAsync(requestUri, values, MediaTypeHeaderValues.APPLICATION_JSONL_MEDIA_TYPE, options, cancellationToken);

        /// <summary>
        /// Sends a POST request to the specified Uri containing the values async stream serialized as JSONL in the request body.
        /// </summary>
        /// <typeparam name="T">The type of the values in async stream to be serialized.</typeparam>
        /// <param name="client">The client used to send the request.</param>
        /// <param name="requestUri">The Uri the request is sent to.</param>
        /// <param name="values">The async stream of values to be serialized.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public static Task<HttpResponseMessage> PostAsJsonlAsync<T>(this HttpClient client, string? requestUri, IAsyncEnumerable<T> values, CancellationToken cancellationToken)
            => client.PostAsStreamedJsonAsync(requestUri, values, MediaTypeHeaderValues.APPLICATION_JSONL_MEDIA_TYPE, options: null, cancellationToken);

        /// <summary>
        /// Sends a POST request to the specified Uri containing the values async stream serialized as JSONL in the request body.
        /// </summary>
        /// <typeparam name="T">The type of the values in async stream to be serialized.</typeparam>
        /// <param name="client">The client used to send the request.</param>
        /// <param name="requestUri">The Uri the request is sent to.</param>
        /// <param name="values">The async stream of values to be serialized.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public static Task<HttpResponseMessage> PostAsJsonlAsync<T>(this HttpClient client, Uri? requestUri, IAsyncEnumerable<T> values, CancellationToken cancellationToken)
            => client.PostAsStreamedJsonAsync(requestUri, values, MediaTypeHeaderValues.APPLICATION_JSONL_MEDIA_TYPE, options: null, cancellationToken);

        private static Task<HttpResponseMessage> PostAsStreamedJsonAsync<T>(this HttpClient client, string? requestUri, IAsyncEnumerable<T> values, string mediaType, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            NdjsonAsyncEnumerableContent<T> ndjsonAsyncEnumerableContent = new NdjsonAsyncEnumerableContent<T>(values, mediaType, options);

            return client.PostAsync(requestUri, ndjsonAsyncEnumerableContent, cancellationToken);
        }

        private static Task<HttpResponseMessage> PostAsStreamedJsonAsync<T>(this HttpClient client, Uri? requestUri, IAsyncEnumerable<T> values, string mediaType, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            NdjsonAsyncEnumerableContent<T> ndjsonAsyncEnumerableContent = new NdjsonAsyncEnumerableContent<T>(values, mediaType, options);

            return client.PostAsync(requestUri, ndjsonAsyncEnumerableContent, cancellationToken);
        }
    }
}
