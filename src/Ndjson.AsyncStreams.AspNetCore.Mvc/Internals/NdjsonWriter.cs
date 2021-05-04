using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Ndjson.AsyncStreams.AspNetCore.Mvc.Internals
{
    internal class NdjsonWriter<T> : INdjsonWriter<T>
    {
        private static readonly byte[] _newlineDelimiter = Encoding.UTF8.GetBytes("\n");

        private readonly Stream _writeStream;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public NdjsonWriter(Stream writeStream, JsonOptions jsonOptions)
        {
            _writeStream = writeStream ?? throw new ArgumentNullException(nameof(Stream));

            _jsonSerializerOptions = jsonOptions.JsonSerializerOptions;
            if (_jsonSerializerOptions.Encoder is null)
            {
                _jsonSerializerOptions = _jsonSerializerOptions.Copy(JavaScriptEncoder.UnsafeRelaxedJsonEscaping);
            }
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
