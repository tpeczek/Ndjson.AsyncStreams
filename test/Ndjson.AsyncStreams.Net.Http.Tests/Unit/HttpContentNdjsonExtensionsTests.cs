using System;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xunit;

namespace Ndjson.AsyncStreams.Net.Http.Tests.Unit
{
    public class HttpContentNdjsonExtensionsTests
    {
        private struct ValueType
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        private const string NDJSON = "{\"id\":1,\"name\":\"Value 01\"}\n{\"id\":2,\"name\":\"Value 02\"}\n";
        private static readonly List<ValueType> VALUES = new ()
        {
            new ValueType { Id = 1, Name = "Value 01" },
            new ValueType { Id = 2, Name = "Value 02" }
        };

        [Fact]
        public async Task ReadFromNdjsonAsync_ContentIsNull_ThrowsArgumentNullException()
        {
            HttpContent content = null;

            await Assert.ThrowsAsync<ArgumentNullException>("content", async () =>
            {
                await foreach (ValueType value in content.ReadFromNdjsonAsync<ValueType>());
            });
        }

        [Fact]
        public async Task ReadFromNdjsonAsync_MediaTypeIsNotSupported_ThrowsNotSupportedException()
        {
            HttpContent content = new StringContent(String.Empty);
            content.Headers.ContentType.MediaType = "application/json";
            content.Headers.ContentType.CharSet = Encoding.UTF8.WebName;

            await Assert.ThrowsAsync<NotSupportedException> (async () =>
            {
                await foreach (ValueType value in content.ReadFromNdjsonAsync<ValueType>()) ;
            });
        }

        [Fact]
        public async Task ReadFromNdjsonAsync_CharSetIsNotUtf8_ThrowsNotSupportedException()
        {
            HttpContent content = new StringContent(String.Empty);
            content.Headers.ContentType.MediaType = "application/x-ndjson";
            content.Headers.ContentType.CharSet = Encoding.UTF32.WebName;

            await Assert.ThrowsAsync<NotSupportedException>(async () =>
            {
                await foreach (ValueType value in content.ReadFromNdjsonAsync<ValueType>()) ;
            });
        }

        [Theory]
        [InlineData("application/x-ndjson")]
        [InlineData("application/jsonl")]
        public async Task ReadFromNdjsonAsync_ReturnsCorrectValues(string mediaType)
        {
            HttpContent content = new StringContent(NDJSON);
            content.Headers.ContentType.MediaType = mediaType;
            content.Headers.ContentType.CharSet = Encoding.UTF8.WebName;

            int valueIndex = 0;
            await foreach (ValueType value in content.ReadFromNdjsonAsync<ValueType>())
            {
                Assert.Equal(VALUES[valueIndex++], value);
            }
            Assert.Equal(VALUES.Count, valueIndex);
        }
    }
}
