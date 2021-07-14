using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using Ndjson.AsyncStreams.AspNetCore.Mvc.Internals;

namespace Ndjson.AsyncStreams.AspNetCore.Mvc.NewtonsoftJson.Formatters
{
    /// <summary>
    /// A <see cref="TextInputFormatter"/> for async stream incoming as NDJSON content that uses <see cref="JsonSerializer"/>.
    /// </summary>
    public class NewtonsoftNdjsonInputFormatter : TextInputFormatter, IInputFormatterExceptionPolicy
    {
        private interface IAsyncEnumerableModelReader
        {
            object ReadModel(Stream inputStream, JsonSerializerSettings serializerSettings);
        }

        private class AsyncEnumerableModelReader<T> : IAsyncEnumerableModelReader
        {
            public object ReadModel(Stream inputStream, JsonSerializerSettings serializerSettings)
            {
                return ReadModelInternal(inputStream, serializerSettings);
            }

            private static async IAsyncEnumerable<T> ReadModelInternal(Stream inputStream, JsonSerializerSettings serializerSettings)
            {
                using StreamReader inputStreamReader = new(inputStream);

                string? valueUtf8Json = await inputStreamReader.ReadLineAsync();
                while (!(valueUtf8Json is null))
                {
                    yield return JsonConvert.DeserializeObject<T>(valueUtf8Json, serializerSettings);

                    valueUtf8Json = await inputStreamReader.ReadLineAsync();
                }
            }
        }

        private static readonly Type _asyncEnumerableType = typeof(IAsyncEnumerable<>);

        private readonly ILogger<NewtonsoftNdjsonInputFormatter> _logger;

        /// <inheritdoc />
        public InputFormatterExceptionPolicy ExceptionPolicy => InputFormatterExceptionPolicy.MalformedInputExceptions;

        /// <summary>
        /// Gets the <see cref="JsonSerializerSettings"/> used to configure the <see cref="JsonSerializer"/>.
        /// </summary>
        /// <remarks>
        /// A single instance of <see cref="NewtonsoftNdjsonInputFormatter"/> is used for all JSON formatting. Any changes to the options will affect all input formatting.
        /// </remarks>
        public JsonSerializerSettings SerializerSettings { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="NewtonsoftNdjsonInputFormatter"/>.
        /// </summary>
        /// <param name="serializerSettings">The <see cref="JsonSerializerSettings"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public NewtonsoftNdjsonInputFormatter(JsonSerializerSettings serializerSettings, ILogger<NewtonsoftNdjsonInputFormatter> logger)
        {
            SerializerSettings = serializerSettings ?? throw new ArgumentNullException(nameof(serializerSettings));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            SupportedEncodings.Add(UTF8EncodingWithoutBOM);

            SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationNdjson);
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

            Type modelReaderType = typeof(AsyncEnumerableModelReader<>).MakeGenericType(context.ModelType.GetGenericArguments()[0]);
            IAsyncEnumerableModelReader? modelReader = (IAsyncEnumerableModelReader?)Activator.CreateInstance(modelReaderType);

            if (modelReader is null)
            {
                string errorMessage = $"Couldn't create an instance of {modelReaderType.Name} for deserializing incoming async stream.";

                _logger.LogDebug(errorMessage);
                context.ModelState.TryAddModelError(String.Empty, errorMessage);

                return Task.FromResult(InputFormatterResult.Failure());
            }

            return Task.FromResult(InputFormatterResult.Success(modelReader.ReadModel(context.HttpContext.Request.Body, SerializerSettings)));
        }
    }
}
