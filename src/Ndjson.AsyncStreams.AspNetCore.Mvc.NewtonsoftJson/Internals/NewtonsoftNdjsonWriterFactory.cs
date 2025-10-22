using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;
using Ndjson.AsyncStreams.AspNetCore.Mvc.Internals;

namespace Ndjson.AsyncStreams.AspNetCore.Mvc.NewtonsoftJson.Internals
{
    internal class NewtonsoftNdjsonWriterFactory : INdjsonWriterFactory
    {
        private static readonly Dictionary<string, string> CONTENT_TYPES = new Dictionary<string, string>
        {
            { MediaTypeHeaderValues.APPLICATION_NDJSON_MEDIA_TYPE, MediaTypeHeaderValues.ApplicationNdjsonWithUTF8Encoding.ToString() },
            { MediaTypeHeaderValues.APPLICATION_JSONL_MEDIA_TYPE, MediaTypeHeaderValues.ApplicationJsonlWithUTF8Encoding.ToString() }
        };

        private readonly IHttpResponseStreamWriterFactory _httpResponseStreamWriterFactory;
        private readonly MvcNewtonsoftJsonOptions _options;
        private readonly NewtonsoftNdjsonArrayPool _jsonArrayPool;

        public NewtonsoftNdjsonWriterFactory(IHttpResponseStreamWriterFactory httpResponseStreamWriterFactory, IOptions<MvcNewtonsoftJsonOptions> options, ArrayPool<char> innerJsonArrayPool)
        {
            _httpResponseStreamWriterFactory = httpResponseStreamWriterFactory ?? throw new ArgumentNullException(nameof(httpResponseStreamWriterFactory));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            if (innerJsonArrayPool == null)
            {
                throw new ArgumentNullException(nameof(innerJsonArrayPool));
            }

            _jsonArrayPool = new NewtonsoftNdjsonArrayPool(innerJsonArrayPool);
        }

        public INdjsonWriter<T> CreateWriter<T>(ActionContext context, IStatusCodeActionResult result)
            => CreateWriter<T>(MediaTypeHeaderValues.APPLICATION_NDJSON_MEDIA_TYPE, context, result);

        public INdjsonWriter<T> CreateWriter<T>(string mediaType, ActionContext context, IStatusCodeActionResult result)
        {
            if (!MediaTypeHeaderValues.IsSupportedMediaType(mediaType))
            {
                throw new NotSupportedException($"Not supported media type {mediaType}.");
            }

            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (result is null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            HttpResponse response = context.HttpContext.Response;

            response.ContentType = CONTENT_TYPES[mediaType];

            if (result.StatusCode != null)
            {
                response.StatusCode = result.StatusCode.Value;
            }

            context.HttpContext.DisableResponseBuffering();

            return new NewtonsoftNdjsonWriter<T>(_httpResponseStreamWriterFactory.CreateWriter(response.Body, Encoding.UTF8), _options.SerializerSettings, _jsonArrayPool);
        }
    }
}
