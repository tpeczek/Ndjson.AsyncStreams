using System.Threading.Tasks;
using System.Collections.Generic;
using Xunit;

namespace Ndjson.AsyncStreams.Net.Http.Tests.Unit
{
    public class NdjsonAsyncEnumerableContentTests
    {
        private struct ValueType
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        private const string NDJSON = "{\"id\":1,\"name\":\"Value 01\"}\n{\"id\":2,\"name\":\"Value 02\"}\n";

        private async IAsyncEnumerable<ValueType> GetValuesAsync()
        {
            await Task.CompletedTask;

            yield return new ValueType { Id = 1, Name = "Value 01" };

            await Task.CompletedTask;

            yield return new ValueType { Id = 2, Name = "Value 02" };
        }

        [Fact]
        public void Create_ValuesIsProvidedValues()
        {
            IAsyncEnumerable<ValueType> values = GetValuesAsync();

            NdjsonAsyncEnumerableContent<ValueType> ndjsonAsyncEnumerableContent = new NdjsonAsyncEnumerableContent<ValueType>(values);

            Assert.Same(values, ndjsonAsyncEnumerableContent.Values);
        }

        [Fact]
        public void Create_MediaTypeIsNdjson()
        {
            IAsyncEnumerable<ValueType> values = GetValuesAsync();

            NdjsonAsyncEnumerableContent<ValueType> ndjsonAsyncEnumerableContent = new NdjsonAsyncEnumerableContent<ValueType>(values);

            Assert.Equal("application/x-ndjson", ndjsonAsyncEnumerableContent.Headers.ContentType.MediaType);
        }

        [Fact]
        public void Create_CharSetIsUtf8()
        {
            IAsyncEnumerable<ValueType> values = GetValuesAsync();

            NdjsonAsyncEnumerableContent<ValueType> ndjsonAsyncEnumerableContent = new NdjsonAsyncEnumerableContent<ValueType>(values);

            Assert.Equal("utf-8", ndjsonAsyncEnumerableContent.Headers.ContentType.CharSet);
        }
        
        [Fact]
        public async Task ReadAsStringAsync_ReturnsCorrectNdjson()
        {
            IAsyncEnumerable<ValueType> values = GetValuesAsync();

            NdjsonAsyncEnumerableContent<ValueType> ndjsonAsyncEnumerableContent = new NdjsonAsyncEnumerableContent<ValueType>(values);

            Assert.Equal(NDJSON, await ndjsonAsyncEnumerableContent.ReadAsStringAsync());
        }
    }
}
