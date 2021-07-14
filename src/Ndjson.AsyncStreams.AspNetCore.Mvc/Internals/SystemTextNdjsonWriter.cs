using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Ndjson.AsyncStreams.AspNetCore.Mvc.Internals
{
    internal class SystemTextNdjsonWriter<T> : INdjsonWriter<T>
    {
        private static readonly byte[] _newlineDelimiter = Encoding.UTF8.GetBytes("\n");

        private readonly Stream _writeStream;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public SystemTextNdjsonWriter(Stream writeStream, JsonSerializerOptions jsonSerializerOptions)
        {
            _writeStream = writeStream ?? throw new ArgumentNullException(nameof(writeStream));
            _jsonSerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
        }

        public Task WriteAsync(T value)
        {
            return WriteAsync(value, CancellationToken.None);
        }

        public async Task WriteAsync(T value, CancellationToken cancellationToken)
        {
            await JsonSerializer.SerializeAsync<T>(_writeStream, value, _jsonSerializerOptions, cancellationToken);
            await _writeStream.WriteAsync(_newlineDelimiter, cancellationToken);
            await _writeStream.FlushAsync(cancellationToken);
        }

        public void Dispose()
        { }
    }
}
