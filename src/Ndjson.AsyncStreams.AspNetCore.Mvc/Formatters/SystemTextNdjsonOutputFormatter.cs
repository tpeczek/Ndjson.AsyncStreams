﻿using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Encodings.Web;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Ndjson.AsyncStreams.AspNetCore.Mvc.Internals;

namespace Ndjson.AsyncStreams.AspNetCore.Mvc.Formatters
{
    /// <summary>
    /// A <see cref="TextOutputFormatter"/> for NDJSON content that uses <see cref="JsonSerializer"/>.
    /// </summary>
    public class SystemTextNdjsonOutputFormatter : TextOutputFormatter
    {
        private interface IAsyncEnumerableStreamSerializer
        {
            Task SerializeAsync(object? asyncEnumerable, Stream writeStream, JsonSerializerOptions jsonSerializerOptions);
        }

        private class AsyncEnumerableStreamSerializer<T> : IAsyncEnumerableStreamSerializer
        {
            public async Task SerializeAsync(object? asyncEnumerable, Stream writeStream, JsonSerializerOptions jsonSerializerOptions)
            {
                IAsyncEnumerable<T>? values = asyncEnumerable as IAsyncEnumerable<T>;
                if (values is null)
                {
                    throw new NotSupportedException($"The only type supported by {nameof(SystemTextNdjsonOutputFormatter)} is IAsyncEnumerable<T>");
                }

                using INdjsonWriter<T> ndjsonTextWriter = new SystemTextNdjsonWriter<T>(writeStream, jsonSerializerOptions);

                await foreach (T value in values)
                {
                    await ndjsonTextWriter.WriteAsync(value);
                }
            }
        }

        private static readonly Type _asyncEnumerableType = typeof(IAsyncEnumerable<>);

        private readonly ILogger<SystemTextNdjsonOutputFormatter> _logger;

        /// <summary>
        /// Gets the <see cref="JsonSerializerOptions"/> used to configure the <see cref="JsonSerializer"/>.
        /// </summary>
        /// <remarks>
        /// A single instance of <see cref="SystemTextNdjsonInputFormatter"/> is used for all JSON formatting. Any changes to the options will affect all input formatting.
        /// </remarks>
        public JsonSerializerOptions SerializerOptions { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="SystemTextNdjsonOutputFormatter"/>.
        /// </summary>
        /// <param name="options">The <see cref="JsonOptions"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public SystemTextNdjsonOutputFormatter(JsonOptions? options, ILogger<SystemTextNdjsonOutputFormatter> logger)
        {
            SerializerOptions = options?.JsonSerializerOptions ?? SystemTextJsonSerializerOptionsExtensions.DefaultJsonSerializerOptions;
            if (SerializerOptions.Encoder is null)
            {
                SerializerOptions = SerializerOptions.CreateCopyWithDifferentEncoder(JavaScriptEncoder.UnsafeRelaxedJsonEscaping);
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            SupportedEncodings.Add(Encoding.UTF8);

            SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationNdjson);
        }

        /// <inheritdoc />
        protected override bool CanWriteType(Type type)
        {
            return type.IsGenericType && (type.GetGenericTypeDefinition() == _asyncEnumerableType);
        }

        /// <inheritdoc />
        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (selectedEncoding is null)
            {
                throw new ArgumentNullException(nameof(selectedEncoding));
            }

            Type serializerType = typeof(AsyncEnumerableStreamSerializer<>).MakeGenericType(context.ObjectType.GetGenericArguments()[0]);
            IAsyncEnumerableStreamSerializer? serializer = (IAsyncEnumerableStreamSerializer?)Activator.CreateInstance(serializerType);

            if (serializer is null)
            {
                throw new Exception($"Couldn't create an instance of {serializerType.Name} for serializing async stream.");
            }

            context.HttpContext.DisableResponseBuffering();

            await serializer.SerializeAsync(context.Object, context.HttpContext.Response.Body, SerializerOptions);
        }
    }
}