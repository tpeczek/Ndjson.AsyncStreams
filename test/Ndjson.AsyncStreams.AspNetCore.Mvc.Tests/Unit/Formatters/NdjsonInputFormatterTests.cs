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
        private const string JSONL_MEDIA_TYPE = "application/jsonl";
        private const string VALUES_CONTENT = "{\"id\":1,\"name\":\"Value 01\"}\n{\"id\":2,\"name\":\"Value 02\"}\n";
        private static readonly List<ValueType> VALUES = new()
        {
            new ValueType { Id = 1, Name = "Value 01" },
            new ValueType { Id = 2, Name = "Value 02" }
        };
        private static readonly Encoding UTF8_ENCODING_WITHOUT_BOM = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        public static IEnumerable<object[]> InputFormatters => new List<object[]>
        {
            new object[] { PrepareSystemTextNdjsonInputFormatter() },
            new object[] { PrepareNewtonsoftNdjsonInputFormatter() }
        };

        public static IEnumerable<object[]> InputFormattersAndMediaTypesMatrix => new List<object[]>
        {
            new object[] { PrepareSystemTextNdjsonInputFormatter(), NDJSON_MEDIA_TYPE },
            new object[] { PrepareSystemTextNdjsonInputFormatter(), JSONL_MEDIA_TYPE },
            new object[] { PrepareNewtonsoftNdjsonInputFormatter(), NDJSON_MEDIA_TYPE },
            new object[] { PrepareNewtonsoftNdjsonInputFormatter(), JSONL_MEDIA_TYPE }
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

        private static InputFormatterContext PrepareInputFormatterContext(string contentType, string content = VALUES_CONTENT, Type modelType = null)
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
        [MemberData(nameof(InputFormatters))]
        public void SupportedMediaTypes_ContainsNdjsonAndJsonl(TextInputFormatter inputFormatter)
        {
            Assert.Collection(inputFormatter.SupportedMediaTypes,
                (string mediaType) => Assert.Equal(NDJSON_MEDIA_TYPE, mediaType),
                (string mediaType) => Assert.Equal(JSONL_MEDIA_TYPE, mediaType)
            );
        }

        [Theory]
        [MemberData(nameof(InputFormatters))]
        public void SupportedEncodings_ContainsOnlyUTF8EncodingWithoutBOM(TextInputFormatter inputFormatter)
        {
            Assert.Collection(inputFormatter.SupportedEncodings, (Encoding encoding) => Assert.Equal(UTF8_ENCODING_WITHOUT_BOM, encoding));
        }

        [Theory]
        [MemberData(nameof(InputFormattersAndMediaTypesMatrix))]
        public void CanRead_ForSupportedContentTypeAndModelType_ReturnsTrue(TextInputFormatter inputFormatter, string contentType)
        {
            InputFormatterContext inputFormatterContext = PrepareInputFormatterContext(contentType);

            bool canRead = inputFormatter.CanRead(inputFormatterContext);

            Assert.True(canRead);
        }

        [Theory]
        [MemberData(nameof(InputFormatters))]
        public void CanRead_ForNotSupportedContentTypeAndSupportedModelType_ReturnsFalse(TextInputFormatter inputFormatter)
        {
            InputFormatterContext inputFormatterContext = PrepareInputFormatterContext(contentType: "application/json");

            bool canRead = inputFormatter.CanRead(inputFormatterContext);

            Assert.False(canRead);
        }

        [Theory]
        [MemberData(nameof(InputFormattersAndMediaTypesMatrix))]
        public void CanRead_SupportedContentTypeAndNotSupportedModelType_ReturnsTrue(TextInputFormatter inputFormatter, string contentType)
        {
            InputFormatterContext inputFormatterContext = PrepareInputFormatterContext(contentType, modelType: typeof(IEnumerable<ValueType>));

            bool canRead = inputFormatter.CanRead(inputFormatterContext);

            Assert.False(canRead);
        }

        [Theory]
        [MemberData(nameof(InputFormattersAndMediaTypesMatrix))]
        public async Task ReadAsync_ForSupportedContentTypeAndModelType_ReadsCorrectValues(TextInputFormatter inputFormatter, string contentType)
        {
            InputFormatterContext inputFormatterContext = PrepareInputFormatterContext(contentType);

            InputFormatterResult inputFormatterResult = await inputFormatter.ReadAsync(inputFormatterContext);

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
