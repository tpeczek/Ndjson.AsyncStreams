using System;
using System.IO;
using System.Text;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ndjson.AsyncStreams.AspNetCore.Mvc.Internals;
using Ndjson.AsyncStreams.AspNetCore.Mvc.NewtonsoftJson.Internals;

namespace Ndjson.AsyncStreams.AspNetCore.Mvc.NewtonsoftJson.Formatters
{
    /// <summary>
    /// A <see cref="TextOutputFormatter"/> for NDJSON or JSONL content that uses <see cref="JsonSerializer"/>.
    /// </summary>
    public class NewtonsoftNdjsonOutputFormatter : TextOutputFormatter
    {
        private interface IAsyncEnumerableStreamSerializer
        {
            Task SerializeAsync(object? asyncEnumerable, TextWriter textResponseStreamWriter, JsonSerializerSettings jsonSerializerSettings, NewtonsoftNdjsonArrayPool jsonArrayPool, CancellationToken cancellationToken);
        }

        private class AsyncEnumerableStreamSerializer<T> : IAsyncEnumerableStreamSerializer
        {
            public async Task SerializeAsync(object? asyncEnumerable, TextWriter textResponseStreamWriter, JsonSerializerSettings jsonSerializerSettings, NewtonsoftNdjsonArrayPool jsonArrayPool, CancellationToken cancellationToken)
            {
                IAsyncEnumerable<T>? values = asyncEnumerable as IAsyncEnumerable<T>;
                if (values is null)
                {
                    throw new NotSupportedException($"The only type supported by {nameof(NewtonsoftNdjsonOutputFormatter)} is IAsyncEnumerable<T>");
                }

                using INdjsonWriter<T> ndjsonTextWriter = new NewtonsoftNdjsonWriter<T>(textResponseStreamWriter, jsonSerializerSettings, jsonArrayPool);

                await foreach (T value in values.WithCancellation(cancellationToken))
                {
                    await ndjsonTextWriter.WriteAsync(value, cancellationToken);
                }
            }
        }

        private static readonly Type _asyncEnumerableType = typeof(IAsyncEnumerable<>);

        private readonly ConcurrentDictionary<Type, IAsyncEnumerableStreamSerializer?> _asyncEnumerableStreamSerializers = new();
        private readonly NewtonsoftNdjsonArrayPool _jsonArrayPool;
        private readonly ILogger<NewtonsoftNdjsonOutputFormatter> _logger;

        /// <summary>
        /// Gets the <see cref="JsonSerializerSettings"/> used to configure the <see cref="JsonSerializer"/>.
        /// </summary>
        /// <remarks>
        /// A single instance of <see cref="NewtonsoftNdjsonOutputFormatter"/> is used for all JSON formatting. Any changes to the options will affect all input formatting.
        /// </remarks>
        public JsonSerializerSettings SerializerSettings { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="NewtonsoftNdjsonOutputFormatter"/>.
        /// </summary>
        /// <param name="serializerSettings">The <see cref="JsonSerializerSettings"/>.</param>
        /// <param name="innerJsonArrayPool">The <see cref="ArrayPool{Char}"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public NewtonsoftNdjsonOutputFormatter(JsonSerializerSettings serializerSettings, ArrayPool<char> innerJsonArrayPool, ILogger<NewtonsoftNdjsonOutputFormatter> logger)
        {
            SerializerSettings = serializerSettings ?? throw new ArgumentNullException(nameof(serializerSettings));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (innerJsonArrayPool == null)
            {
                throw new ArgumentNullException(nameof(innerJsonArrayPool));
            }

            _jsonArrayPool = new NewtonsoftNdjsonArrayPool(innerJsonArrayPool);

            SupportedEncodings.Add(Encoding.UTF8);

            SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationNdjson);
            SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationJsonl);
        }

        /// <inheritdoc />
        protected override bool CanWriteType(Type? type)
        {
            return (type is not null) && type.IsGenericType && (type.GetGenericTypeDefinition() == _asyncEnumerableType);
        }

        /// <inheritdoc />
        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext? context, Encoding? selectedEncoding)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (selectedEncoding is null)
            {
                throw new ArgumentNullException(nameof(selectedEncoding));
            }

            IAsyncEnumerableStreamSerializer? serializer = GetSerializer(context.ObjectType);

            if (serializer is null)
            {
                throw new Exception($"Couldn't create an instance of {nameof(IAsyncEnumerableStreamSerializer)} for serializing async stream.");
            }

            context.HttpContext.DisableResponseBuffering();

            await using TextWriter textResponseStreamWriter = context.WriterFactory(context.HttpContext.Response.Body, selectedEncoding);

            try
            {
                await serializer.SerializeAsync(context.Object, textResponseStreamWriter, SerializerSettings, _jsonArrayPool, context.HttpContext.RequestAborted);
            }
            catch (OperationCanceledException) when (context.HttpContext.RequestAborted.IsCancellationRequested) { }
        }

        private IAsyncEnumerableStreamSerializer? GetSerializer(Type? objectType)
        {
            IAsyncEnumerableStreamSerializer? serializer = null;

            if ((objectType is not null) && !_asyncEnumerableStreamSerializers.TryGetValue(objectType, out serializer))
            {
                Type serializerType = typeof(AsyncEnumerableStreamSerializer<>).MakeGenericType(objectType.GetGenericArguments()[0]);
                serializer = (IAsyncEnumerableStreamSerializer?)Activator.CreateInstance(serializerType);

                _asyncEnumerableStreamSerializers.TryAdd(objectType, serializer);
            }

            return serializer;
        }
    }
}
