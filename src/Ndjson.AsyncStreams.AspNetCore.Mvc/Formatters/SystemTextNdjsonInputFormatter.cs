using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Ndjson.AsyncStreams.AspNetCore.Mvc.Internals;

namespace Ndjson.AsyncStreams.AspNetCore.Mvc.Formatters
{
    /// <summary>
    /// A <see cref="TextInputFormatter"/> for async stream incoming as NDJSON or JSONL content that uses <see cref="JsonSerializer"/>.
    /// </summary>
    public class SystemTextNdjsonInputFormatter : TextInputFormatter, IInputFormatterExceptionPolicy
    {
        private interface IAsyncEnumerableModelReader
        {
            object ReadModel(Stream inputStream, JsonSerializerOptions serializerOptions);
        }

        private class AsyncEnumerableModelReader<T> : IAsyncEnumerableModelReader
        {
            public object ReadModel(Stream inputStream, JsonSerializerOptions serializerOptions)
            {
                return ReadModelInternal(inputStream, serializerOptions);
            }

            private static async IAsyncEnumerable<T?> ReadModelInternal(Stream inputStream, JsonSerializerOptions serializerOptions)
            {
                using StreamReader inputStreamReader = new(inputStream);

                string? valueUtf8Json = await inputStreamReader.ReadLineAsync();
                while (valueUtf8Json is not null)
                {
                    yield return JsonSerializer.Deserialize<T>(valueUtf8Json, serializerOptions);

                    valueUtf8Json = await inputStreamReader.ReadLineAsync();
                }
            }
        }

        private static readonly Type _asyncEnumerableType = typeof(IAsyncEnumerable<>);

        private readonly ConcurrentDictionary<Type, IAsyncEnumerableModelReader?> _asyncEnumerableModelReaders = new();
        private readonly ILogger<SystemTextNdjsonInputFormatter> _logger;

        /// <inheritdoc />
        public InputFormatterExceptionPolicy ExceptionPolicy => InputFormatterExceptionPolicy.MalformedInputExceptions;

        /// <summary>
        /// Gets the <see cref="JsonSerializerOptions"/> used to configure the <see cref="JsonSerializer"/>.
        /// </summary>
        /// <remarks>
        /// A single instance of <see cref="SystemTextNdjsonInputFormatter"/> is used for all JSON formatting. Any changes to the options will affect all input formatting.
        /// </remarks>
        public JsonSerializerOptions SerializerOptions { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="SystemTextNdjsonInputFormatter"/>.
        /// </summary>
        /// <param name="options">The <see cref="JsonOptions"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public SystemTextNdjsonInputFormatter(JsonOptions? options, ILogger<SystemTextNdjsonInputFormatter> logger)
        {
            SerializerOptions = options?.JsonSerializerOptions ?? SystemTextJsonSerializerOptionsExtensions.DefaultJsonSerializerOptions;

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            SupportedEncodings.Add(UTF8EncodingWithoutBOM);

            SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationNdjson);
            SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationJsonl);
        }

        /// <inheritdoc />
        protected override bool CanReadType(Type type)
        {
            return type.IsGenericType && (type.GetGenericTypeDefinition() == _asyncEnumerableType);
        }

        /// <inheritdoc />
        public override Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (encoding is null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            IAsyncEnumerableModelReader? modelReader = GetModelReader(context.ModelType);

            if (modelReader is null)
            {
                string errorMessage = $"Couldn't create an instance of {nameof(IAsyncEnumerableModelReader)} for deserializing incoming async stream.";

                _logger.LogDebug(errorMessage);
                context.ModelState.TryAddModelError(String.Empty, errorMessage);

                return Task.FromResult(InputFormatterResult.Failure());
            }

            return Task.FromResult(InputFormatterResult.Success(modelReader.ReadModel(context.HttpContext.Request.Body, SerializerOptions)));
        }

        private IAsyncEnumerableModelReader? GetModelReader(Type modelType)
        {
            IAsyncEnumerableModelReader? modelReader;
            if (!_asyncEnumerableModelReaders.TryGetValue(modelType, out modelReader))
            {
                Type modelReaderType = typeof(AsyncEnumerableModelReader<>).MakeGenericType(modelType.GetGenericArguments()[0]);
                modelReader = (IAsyncEnumerableModelReader?)Activator.CreateInstance(modelReaderType);

                _asyncEnumerableModelReaders.TryAdd(modelType, modelReader);
            }

            return modelReader;
        }
    }
}
