using Microsoft.AspNetCore.Mvc;
using Ndjson.AsyncStreams.AspNetCore.Mvc.Internals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Ndjson.AsyncStreams.AspNetCore.Mvc.Tests.Unit
{
    public class NdjsonWriterTests
    {
        private struct ValueType
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        private static readonly ValueType VALUE = new ValueType { Id = 1, Name = "Value 01" };
        private const string SERIALIZED_VALUE_WITH_DELIMITER = "{\"id\":1,\"name\":\"Value 01\"}\n";

        [Fact]
        public async Task WriteAsync_SerializesValueWithDelimiter()
        {
            MemoryStream writeStream = new MemoryStream();
            NdjsonWriter<ValueType> ndjsonWriter = new NdjsonWriter<ValueType>(writeStream, new JsonOptions());

            await ndjsonWriter.WriteAsync(VALUE);

            writeStream.Seek(0, SeekOrigin.Begin);
            StreamReader writeStreamReader = new StreamReader(writeStream);
            Assert.Equal(SERIALIZED_VALUE_WITH_DELIMITER, writeStreamReader.ReadToEnd());
        }
    }
}
