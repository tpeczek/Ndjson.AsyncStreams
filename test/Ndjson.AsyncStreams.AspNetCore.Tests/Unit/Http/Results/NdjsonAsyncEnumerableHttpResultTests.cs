using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Ndjson.AsyncStreams.AspNetCore.Http.HttpResults;
using Xunit;
using Moq;

namespace Ndjson.AsyncStreams.AspNetCore.Tests.Unit.Http.Results
{
    public class NdjsonAsyncEnumerableHttpResultTests
    {
        public struct ValueType
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        private static readonly int STATUS_CODE = 100;
        private static readonly string NDJSON_CONTENT_TYPE = new MediaTypeHeaderValue("application/x-ndjson")
        {
            Encoding = Encoding.UTF8
        }.ToString();
        private static readonly string JSONL_CONTENT_TYPE = new MediaTypeHeaderValue("application/jsonl")
        {
            Encoding = Encoding.UTF8
        }.ToString();

        private static readonly ValueType[] VALUES = [new ValueType { Id = 1, Name = "Value 01" }, new ValueType { Id = 2, Name = "Value 02" }];
        private static readonly JsonSerializerOptions JSON_SERIALIZER_OPTIONS = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        private const string VALUES_AS_NDJSON = "{\"id\":1,\"name\":\"Value 01\"}\n{\"id\":2,\"name\":\"Value 02\"}\n";

        private static HttpContext PrepareHttpContext(IHttpResponseBodyFeature httpResponseBodyFeature = null)
        {
            HttpContext httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();
            if (httpResponseBodyFeature != null)
            {
                httpContext.Features.Set(httpResponseBodyFeature);
            }

            return httpContext;
        }

        [Fact]
        public async Task ExecuteAsync_StatusCodeIsProvided_SetsResponseStatusCode()
        {
            HttpContext httpContext = PrepareHttpContext();
            NdjsonAsyncEnumerableHttpResult<ValueType> ndjsonAsyncEnumerableHttpResult = new NdjsonAsyncEnumerableHttpResult<ValueType>(StreamValuesAsync(), "application/x-ndjson", JSON_SERIALIZER_OPTIONS, statusCode: STATUS_CODE);

            await ndjsonAsyncEnumerableHttpResult.ExecuteAsync(httpContext);

            Assert.Equal(STATUS_CODE, httpContext.Response.StatusCode);
        }

        [Fact]
        public async Task ExecuteAsync_NdjsonContentType_ResponseContentTypeIsSetToNdjsonWithUtf8Encoding()
        {
            HttpContext httpContext = PrepareHttpContext();
            NdjsonAsyncEnumerableHttpResult<ValueType> ndjsonAsyncEnumerableHttpResult = new NdjsonAsyncEnumerableHttpResult<ValueType>(StreamValuesAsync(), "application/x-ndjson", JSON_SERIALIZER_OPTIONS);

            await ndjsonAsyncEnumerableHttpResult.ExecuteAsync(httpContext);

            Assert.Equal(NDJSON_CONTENT_TYPE, httpContext.Response.ContentType);
        }

        [Fact]
        public async Task ExecuteAsync_JsonlContentType_ResponseContentTypeIsSetToJsonlWithUtf8Encoding()
        {
            HttpContext httpContext = PrepareHttpContext();
            NdjsonAsyncEnumerableHttpResult<ValueType> ndjsonAsyncEnumerableHttpResult = new NdjsonAsyncEnumerableHttpResult<ValueType>(StreamValuesAsync(), "application/jsonl", JSON_SERIALIZER_OPTIONS);

            await ndjsonAsyncEnumerableHttpResult.ExecuteAsync(httpContext);

            Assert.Equal(JSONL_CONTENT_TYPE, httpContext.Response.ContentType);
        }

        [Fact]
        public async Task ExecuteAsync_DisablesResponseBuffering()
        {
            Mock<StreamResponseBodyFeature> httpResponseBodyFeatureMock = new(Stream.Null);
            HttpContext httpContext = PrepareHttpContext(httpResponseBodyFeatureMock.Object);
            NdjsonAsyncEnumerableHttpResult<ValueType> ndjsonAsyncEnumerableHttpResult = new NdjsonAsyncEnumerableHttpResult<ValueType>(StreamValuesAsync(), "application/x-ndjson", JSON_SERIALIZER_OPTIONS);

            await ndjsonAsyncEnumerableHttpResult.ExecuteAsync(httpContext);

            httpResponseBodyFeatureMock.Verify(m => m.DisableBuffering(), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WritesValuesAsNdjson()
        {
            HttpContext httpContext = PrepareHttpContext();
            NdjsonAsyncEnumerableHttpResult<ValueType> ndjsonAsyncEnumerableHttpResult = new NdjsonAsyncEnumerableHttpResult<ValueType>(StreamValuesAsync(), "application/x-ndjson", JSON_SERIALIZER_OPTIONS);

            await ndjsonAsyncEnumerableHttpResult.ExecuteAsync(httpContext);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            StreamReader responseBodyReader = new(httpContext.Response.Body);
            Assert.Equal(VALUES_AS_NDJSON, responseBodyReader.ReadToEnd());
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        async IAsyncEnumerable<ValueType> StreamValuesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            for (int valueIndex = 0; valueIndex < VALUES.Length; valueIndex++)
            {
                yield return VALUES[valueIndex];
            };
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}
