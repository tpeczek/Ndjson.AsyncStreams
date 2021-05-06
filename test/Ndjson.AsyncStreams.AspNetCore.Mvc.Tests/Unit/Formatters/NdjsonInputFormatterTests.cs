using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Ndjson.AsyncStreams.AspNetCore.Mvc.Formatters;
using Ndjson.AsyncStreams.AspNetCore.Mvc.NewtonsoftJson.Formatters;
using Xunit;

namespace Ndjson.AsyncStreams.AspNetCore.Mvc.Tests.Unit.Formatters
{
    public class NdjsonInputFormatterTests
    {
        private struct ValueType
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        private const string NDJSON_MEDIA_TYPE = "application/x-ndjson";
        private const string NDJSON_CONTENT = "{\"id\":1,\"name\":\"Value 01\"}\n{\"id\":2,\"name\":\"Value 02\"}\n";
        private static readonly List<ValueType> VALUES = new()
        {
            new ValueType { Id = 1, Name = "Value 01" },
            new ValueType { Id = 2, Name = "Value 02" }
        };
        private static readonly Encoding UTF8_ENCODING_WITHOUT_BOM = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        public static IEnumerable<object[]> NdjsonInputFormatters => new List<object[]>
        {
            new object[] { PrepareSystemTextNdjsonInputFormatter() },
            new object[] { PrepareNewtonsoftNdjsonInputFormatter() }
        };

        private static TextInputFormatter PrepareSystemTextNdjsonInputFormatter()
        {
            return new SystemTextNdjsonInputFormatter(new JsonOptions(), new NullLogger<SystemTextNdjsonInputFormatter>());
        }

        private static TextInputFormatter PrepareNewtonsoftNdjsonInputFormatter()
        {
            return new NewtonsoftNdjsonInputFormatter(
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }, 
                new NullLogger<NewtonsoftNdjsonInputFormatter>()
            );
        }

        private static InputFormatterContext PrepareInputFormatterContext(string contentType = NDJSON_MEDIA_TYPE, string content = NDJSON_CONTENT, Type modelType = null)
        {
            HttpContext httpContext = new DefaultHttpContext();
            httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(content));
            httpContext.Request.ContentType = contentType;

            IModelMetadataProvider modelMetadataProvider = new EmptyModelMetadataProvider();
            return new InputFormatterContext(
                httpContext,
                String.Empty,
                new ModelStateDictionary(),
                modelMetadataProvider.GetMetadataForType(modelType ?? typeof(IAsyncEnumerable<ValueType>)),
                (Stream stream, Encoding encoding) => new HttpRequestStreamReader(stream, encoding),
                false
            );
        }

        [Theory]
        [MemberData(nameof(NdjsonInputFormatters))]
        public void SupportedMediaTypes_ContainsOnlyNdjson(TextInputFormatter ndjsonInputFormatter)
        {
            Assert.Collection(ndjsonInputFormatter.SupportedMediaTypes, (string mediaType) => Assert.Equal(NDJSON_MEDIA_TYPE, mediaType));
        }

        [Theory]
        [MemberData(nameof(NdjsonInputFormatters))]
        public void SupportedEncodings_ContainsOnlyUTF8EncodingWithoutBOM(TextInputFormatter ndjsonInputFormatter)
        {
            Assert.Collection(ndjsonInputFormatter.SupportedEncodings, (Encoding encoding) => Assert.Equal(UTF8_ENCODING_WITHOUT_BOM, encoding));
        }

        [Theory]
        [MemberData(nameof(NdjsonInputFormatters))]
        public void CanRead_ForSupportedContentTypeAndModelType_ReturnsTrue(TextInputFormatter ndjsonInputFormatter)
        {
            InputFormatterContext inputFormatterContext = PrepareInputFormatterContext();

            bool canRead = ndjsonInputFormatter.CanRead(inputFormatterContext);

            Assert.True(canRead);
        }

        [Theory]
        [MemberData(nameof(NdjsonInputFormatters))]
        public void CanRead_ForNotSupportedContentTypeAndSupportedModelType_ReturnsFalse(TextInputFormatter ndjsonInputFormatter)
        {
            InputFormatterContext inputFormatterContext = PrepareInputFormatterContext(contentType: "application/json");

            bool canRead = ndjsonInputFormatter.CanRead(inputFormatterContext);

            Assert.False(canRead);
        }

        [Theory]
        [MemberData(nameof(NdjsonInputFormatters))]
        public void CanRead_SupportedContentTypeAndNotSupportedModelType_ReturnsTrue(TextInputFormatter ndjsonInputFormatter)
        {
            InputFormatterContext inputFormatterContext = PrepareInputFormatterContext(modelType: typeof(IEnumerable<ValueType>));

            bool canRead = ndjsonInputFormatter.CanRead(inputFormatterContext);

            Assert.False(canRead);
        }

        [Theory]
        [MemberData(nameof(NdjsonInputFormatters))]
        public async Task ReadAsync_ForSupportedContentTypeAndModelType_ReadsCorrectValues(TextInputFormatter ndjsonInputFormatter)
        {
            InputFormatterContext inputFormatterContext = PrepareInputFormatterContext();

            InputFormatterResult inputFormatterResult = await ndjsonInputFormatter.ReadAsync(inputFormatterContext);

            Assert.False(inputFormatterResult.HasError);
            IAsyncEnumerable<ValueType> values = Assert.IsAssignableFrom<IAsyncEnumerable<ValueType>>(inputFormatterResult.Model);

            int valueIndex = 0;
            await foreach (ValueType value in values)
            {
                Assert.Equal(VALUES[valueIndex++], value);
            }
            Assert.Equal(VALUES.Count, valueIndex);
        }
    }
}
