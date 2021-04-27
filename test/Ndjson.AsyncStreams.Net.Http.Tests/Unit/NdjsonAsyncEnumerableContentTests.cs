using System.Threading.Tasks;
using System.Collections.Generic;
using Xunit;

namespace Ndjson.AsyncStreams.Net.Http.Tests.Unit
{
    public class NdjsonAsyncEnumerableContentTests
    {
        private const string SERIALIZED_VALUES = "{\"id\":1,\"name\":\"Value 01\"}\n{\"id\":2,\"name\":\"Value 02\"}\n";

        private class ValueType
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        private async IAsyncEnumerable<ValueType> GetValuesAsync()
        {
            await Task.CompletedTask;

            yield return new ValueType { Id = 1, Name = "Value 01" };

            await Task.CompletedTask;

            yield return new ValueType { Id = 2, Name = "Value 02" };
        }

        [Fact]
        public void Create_ValuesType_IsDeclaredType()
        {
            IAsyncEnumerable<ValueType> values = GetValuesAsync();

            NdjsonAsyncEnumerableContent ndjsonAsyncEnumerableContent = NdjsonAsyncEnumerableContent.Create(values);

            Assert.Equal(typeof(ValueType), ndjsonAsyncEnumerableContent.ValuesType);
        }

        [Fact]
        public void Create_Values_IsProvidedValues()
        {
            IAsyncEnumerable<ValueType> values = GetValuesAsync();

            NdjsonAsyncEnumerableContent ndjsonAsyncEnumerableContent = NdjsonAsyncEnumerableContent.Create(values);

            Assert.Same(values, ndjsonAsyncEnumerableContent.Values);
        }

        [Fact]
        public void Create_MediaType_IsNdjson()
        {
            IAsyncEnumerable<ValueType> values = GetValuesAsync();

            NdjsonAsyncEnumerableContent ndjsonAsyncEnumerableContent = NdjsonAsyncEnumerableContent.Create(values);

            Assert.Equal("application/x-ndjson", ndjsonAsyncEnumerableContent.Headers.ContentType.MediaType);
        }

        [Fact]
        public void Create_CharSet_IsUtf8()
        {
            IAsyncEnumerable<ValueType> values = GetValuesAsync();

            NdjsonAsyncEnumerableContent ndjsonAsyncEnumerableContent = NdjsonAsyncEnumerableContent.Create(values);

            Assert.Equal("utf-8", ndjsonAsyncEnumerableContent.Headers.ContentType.CharSet);
        }
        
        [Fact]
        public async Task ReadAsStringAsync_ReturnsCorrectNdjson()
        {
            IAsyncEnumerable<ValueType> values = GetValuesAsync();

            NdjsonAsyncEnumerableContent ndjsonAsyncEnumerableContent = NdjsonAsyncEnumerableContent.Create(values);

            Assert.Equal(SERIALIZED_VALUES, await ndjsonAsyncEnumerableContent.ReadAsStringAsync());
        }
    }
}
