using System.IO;
using System.Linq;
using System.Text;
using System.Buffers;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Ndjson.AsyncStreams.AspNetCore.Mvc.Formatters;
using Ndjson.AsyncStreams.AspNetCore.Mvc.NewtonsoftJson.Formatters;
using Xunit;

namespace Ndjson.AsyncStreams.AspNetCore.Mvc.Tests.Unit.Formatters
{
    public class NdjsonOutputFormatterTests
    {
        private struct ValueType
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        private const string NDJSON_MEDIA_TYPE = "application/x-ndjson";

        private const string NDJSON = "{\"id\":1,\"name\":\"Value 01\"}\n{\"id\":2,\"name\":\"Value 02\"}\n";

        public static IEnumerable<object[]> NdjsonOutputFormatters => new List<object[]>
        {
            new object[] { PrepareSystemTextNdjsonOutputFormatter() },
            new object[] { PrepareNewtonsoftNdjsonOutputFormatter() }
        };

        private static TextOutputFormatter PrepareSystemTextNdjsonOutputFormatter()
        {
            return new SystemTextNdjsonOutputFormatter(new JsonOptions(), new NullLogger<SystemTextNdjsonOutputFormatter>());
        }

        private static TextOutputFormatter PrepareNewtonsoftNdjsonOutputFormatter()
        {
            return new NewtonsoftNdjsonOutputFormatter(
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() },
                ArrayPool<char>.Create(),
                new NullLogger<NewtonsoftNdjsonOutputFormatter>()
            );
        }

        private static async IAsyncEnumerable<ValueType> GetValuesAsync()
        {
            await Task.CompletedTask;

            yield return new ValueType { Id = 1, Name = "Value 01" };

            await Task.CompletedTask;

            yield return new ValueType { Id = 2, Name = "Value 02" };
        }

        private static TextWriter CreateWriterForOutputFormatterWriteContext(Stream stream, Encoding encoding)
        {
            return new HttpResponseStreamWriter(stream, encoding, 16 * 1024);
        }

        private static OutputFormatterWriteContext PrepareOutputFormatterCanWriteContext(string contentType = NDJSON_MEDIA_TYPE, object @object = null)
        {
            HttpContext httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("Accept", contentType);
            httpContext.Response.Body = new MemoryStream();

            return new OutputFormatterWriteContext(
                httpContext,
                CreateWriterForOutputFormatterWriteContext,
                @object is null ? typeof(IAsyncEnumerable<ValueType>) : @object.GetType(),
                @object ?? GetValuesAsync()
            ) { ContentType = new StringSegment(httpContext.Request.Headers["Accept"]) };
        }

        [Theory]
        [MemberData(nameof(NdjsonOutputFormatters))]
        public void SupportedMediaTypes_ContainsOnlyNdjson(TextOutputFormatter ndjsonOutputFormatter)
        {
            Assert.Collection(ndjsonOutputFormatter.SupportedMediaTypes, (string mediaType) => Assert.Equal(NDJSON_MEDIA_TYPE, mediaType));
        }

        [Theory]
        [MemberData(nameof(NdjsonOutputFormatters))]
        public void SupportedEncodings_ContainsOnlyUTF8Encoding(TextOutputFormatter ndjsonOutputFormatter)
        {
            Assert.Collection(ndjsonOutputFormatter.SupportedEncodings, (Encoding encoding) => Assert.Equal(Encoding.UTF8, encoding));
        }

        [Theory]
        [MemberData(nameof(NdjsonOutputFormatters))]
        public void CanWriteResult_ForSupportedContentTypeAndObjectType_ReturnsTrue(TextOutputFormatter ndjsonOutputFormatter)
        {
            OutputFormatterWriteContext outputFormatterCanWriteContext = PrepareOutputFormatterCanWriteContext();

            bool canWriteResult = ndjsonOutputFormatter.CanWriteResult(outputFormatterCanWriteContext);

            Assert.True(canWriteResult);
        }

        [Theory]
        [MemberData(nameof(NdjsonOutputFormatters))]
        public void CanWriteResult_ForNotSupportedContentTypeAndSupportedObjectType_ReturnsFalse(TextOutputFormatter ndjsonOutputFormatter)
        {
            OutputFormatterWriteContext outputFormatterCanWriteContext = PrepareOutputFormatterCanWriteContext(contentType: "application/json");

            bool canWriteResult = ndjsonOutputFormatter.CanWriteResult(outputFormatterCanWriteContext);

            Assert.False(canWriteResult);
        }

        [Theory]
        [MemberData(nameof(NdjsonOutputFormatters))]
        public void CanWriteResult_SupportedContentTypeAndNotSupportedObjectType_ReturnsTrue(TextOutputFormatter ndjsonOutputFormatter)
        {
            OutputFormatterWriteContext outputFormatterCanWriteContext = PrepareOutputFormatterCanWriteContext(@object: Enumerable.Empty<ValueType>());

            bool canWriteResult = ndjsonOutputFormatter.CanWriteResult(outputFormatterCanWriteContext);

            Assert.False(canWriteResult);
        }

        [Theory]
        [MemberData(nameof(NdjsonOutputFormatters))]
        public async Task WriteAsync_ForSupportedContentTypeAndObjectType_WritesCorrectNdjson(TextOutputFormatter ndjsonOutputFormatter)
        {
            OutputFormatterWriteContext outputFormatterCanWriteContext = PrepareOutputFormatterCanWriteContext();

            await ndjsonOutputFormatter.WriteAsync(outputFormatterCanWriteContext);

            using StreamReader responseBodyReader = new StreamReader(outputFormatterCanWriteContext.HttpContext.Response.Body);
            outputFormatterCanWriteContext.HttpContext.Response.Body.Seek(0, SeekOrigin.Begin);

            Assert.Equal(NDJSON, await responseBodyReader.ReadToEndAsync());
        }
    }
}
