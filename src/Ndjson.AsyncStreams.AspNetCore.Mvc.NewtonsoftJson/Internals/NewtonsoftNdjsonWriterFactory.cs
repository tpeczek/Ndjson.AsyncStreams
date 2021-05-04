using System;
using System.Buffers;
using System.Text;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Ndjson.AsyncStreams.AspNetCore.Mvc.NewtonsoftJson.Internals
{
    internal class NewtonsoftNdjsonWriterFactory : INdjsonWriterFactory
    {
        private static readonly string CONTENT_TYPE = new MediaTypeHeaderValue("application/x-ndjson")
        {
            Encoding = Encoding.UTF8
        }.ToString();

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

            DisableResponseBuffering(context.HttpContext);

            return new NewtonsoftNdjsonWriter<T>(_httpResponseStreamWriterFactory.CreateWriter(response.Body, Encoding.UTF8), _options.SerializerSettings, _jsonArrayPool);
        }

        private static void DisableResponseBuffering(HttpContext context)
        {
            IHttpResponseBodyFeature responseBodyFeature = context.Features.Get<IHttpResponseBodyFeature>();
            if (responseBodyFeature != null)
            {
                responseBodyFeature.DisableBuffering();
            }
        }
    }
}
