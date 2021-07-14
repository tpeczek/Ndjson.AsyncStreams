using System;
using System.Text.Json;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;

namespace Ndjson.AsyncStreams.AspNetCore.Mvc.Internals
{
    internal class SystemTextNdjsonWriterFactory : INdjsonWriterFactory
    {
        private static readonly string CONTENT_TYPE = MediaTypeHeaderValues.ApplicationNdjsonWithUTF8Encoding.ToString();

        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public SystemTextNdjsonWriterFactory(IOptions<JsonOptions> options)
        {
            if (options is null || options.Value is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _jsonSerializerOptions = options.Value.JsonSerializerOptions;
            if (_jsonSerializerOptions.Encoder is null)
            {
                _jsonSerializerOptions = _jsonSerializerOptions.CreateCopyWithDifferentEncoder(JavaScriptEncoder.UnsafeRelaxedJsonEscaping);
            }
        }

        public INdjsonWriter<T> CreateWriter<T>(ActionContext context, IStatusCodeActionResult result)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (result is null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            HttpResponse response = context.HttpContext.Response;

            response.ContentType = CONTENT_TYPE;

            if (result.StatusCode != null)
            {
                response.StatusCode = result.StatusCode.Value;
            }

            context.HttpContext.DisableResponseBuffering();

            return new SystemTextNdjsonWriter<T>(response.Body, _jsonSerializerOptions);
        }
    }
}
