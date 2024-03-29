﻿using System;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Ndjson.AsyncStreams.Net.Http
{
    /// <summary>
    /// Contains extension methods to read and then parse the <see cref="IAsyncEnumerable{T}"/> from NDJSON.
    /// </summary>
    public static class HttpContentNdjsonExtensions
    {
        private static readonly JsonSerializerOptions _defaultJsonSerializerOptions = new(JsonSerializerDefaults.Web);

        /// <summary>
        /// Reads the HTTP content and returns the async stream of values that results from deserializing the content as NDJSON in an asynchronous operation.
        /// </summary>
        /// <typeparam name="T">The target type to deserialize the values in async stream to.</typeparam>
        /// <param name="content">The content to read from.</param>
        /// <param name="options">Options to control the behavior during deserialization.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public static async IAsyncEnumerable<T?> ReadFromNdjsonAsync<T>(this HttpContent content, JsonSerializerOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (content is null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            string? mediaType = content.Headers.ContentType?.MediaType;
            string? charset = content.Headers.ContentType?.CharSet;

            if (!IsNdjsonMediaType(mediaType) || !IsUtf8Encoding(charset))
            {
                throw new NotSupportedException();
            }

            JsonSerializerOptions jsonSerializerOptions = options ?? _defaultJsonSerializerOptions;

            using Stream contentStream = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using StreamReader contentStreamReader = new (contentStream);

            string? valueUtf8Json = await contentStreamReader.ReadLineAsync(cancellationToken).ConfigureAwait(false);

            while (valueUtf8Json is not null)
            {
                yield return JsonSerializer.Deserialize<T>(valueUtf8Json, jsonSerializerOptions);

                valueUtf8Json = await contentStreamReader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private static bool IsNdjsonMediaType(string? mediaType)
        {
            if (mediaType is null)
            {
                return false;
            }

            return mediaType.Equals("application/x-ndjson", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsUtf8Encoding(string? charset)
        {
            if (charset is null)
            {
                return true;
            }

            try
            {
                if (charset.Length > 2 && charset[0] == '\"' && charset[charset.Length - 1] == '\"')
                {
                    return Encoding.GetEncoding(charset.Substring(1, charset.Length - 2)) == Encoding.UTF8;
                }
                else
                {
                    return Encoding.GetEncoding(charset) == Encoding.UTF8;
                }
            }
            catch (ArgumentException ex)
            {
                throw new InvalidOperationException("The character set provided in ContentType is invalid.", ex);
            }
        }
    }
}
