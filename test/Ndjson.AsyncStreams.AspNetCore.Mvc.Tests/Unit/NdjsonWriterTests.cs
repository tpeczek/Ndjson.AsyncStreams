using System.IO;
using System.Buffers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Ndjson.AsyncStreams.AspNetCore.Mvc.Internals;
using Ndjson.AsyncStreams.AspNetCore.Mvc.NewtonsoftJson.Internals;
using Xunit;

namespace Ndjson.AsyncStreams.AspNetCore.Mvc.Tests.Unit
{
    public class NdjsonWriterTests
    {
        public struct ValueType
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        public delegate INdjsonWriter<ValueType> PrepareINdjsonWriter(Stream writeStream);

        private static readonly ValueType VALUE = new() { Id = 1, Name = "Value 01" };
        private const string SERIALIZED_VALUE_WITH_DELIMITER = "{\"id\":1,\"name\":\"Value 01\"}\n";

        public static IEnumerable<object[]> NdjsonWriterPreparers => new List<object[]>
        {
            new object[] { (PrepareINdjsonWriter)PrepareNdjsonWriter },
            new object[] { (PrepareINdjsonWriter)PrepareNewtonsoftNdjsonWriter }
        };

        private static INdjsonWriter<ValueType> PrepareNdjsonWriter(Stream writeStream)
        {
            return new NdjsonWriter<ValueType>(writeStream, new JsonOptions());
        }

        private static INdjsonWriter<ValueType> PrepareNewtonsoftNdjsonWriter(Stream writeStream)
        {
            return new NewtonsoftNdjsonWriter<ValueType>(
                new StreamWriter(writeStream),
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() },
                new NewtonsoftNdjsonArrayPool(ArrayPool<char>.Create())
            );
        }

        [Theory]
        [MemberData(nameof(NdjsonWriterPreparers))]
        public async Task WriteAsync_SerializesValueWithDelimiter(PrepareINdjsonWriter ndjsonWriterPreperer)
        {
            MemoryStream writeStream = new();
            INdjsonWriter<ValueType> ndjsonWriter = ndjsonWriterPreperer.Invoke(writeStream);

            await ndjsonWriter.WriteAsync(VALUE);

            writeStream.Seek(0, SeekOrigin.Begin);
            StreamReader writeStreamReader = new(writeStream);
            Assert.Equal(SERIALIZED_VALUE_WITH_DELIMITER, writeStreamReader.ReadToEnd());
        }
    }
}
