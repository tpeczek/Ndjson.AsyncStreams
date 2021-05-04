using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Ndjson.AsyncStreams.AspNetCore.Mvc.NewtonsoftJson.Internals
{
    internal class NewtonsoftNdjsonWriter<T> : INdjsonWriter<T>
    {
        private readonly TextWriter _textResponseStreamWriter;
        private readonly JsonTextWriter _jsonResponseStreamWriter;
        private readonly JsonSerializer _jsonSerializer;

        public NewtonsoftNdjsonWriter(TextWriter textResponseStreamWriter, JsonSerializerSettings jsonSerializerSettings, NewtonsoftNdjsonArrayPool jsonArrayPool)
        {
            _textResponseStreamWriter = textResponseStreamWriter ?? throw new ArgumentNullException(nameof(textResponseStreamWriter));

            _jsonResponseStreamWriter = new JsonTextWriter(textResponseStreamWriter)
            {
                ArrayPool = jsonArrayPool,
                CloseOutput = false,
                AutoCompleteOnClose = false
            };

            _jsonSerializer = JsonSerializer.Create(jsonSerializerSettings);
        }

        public Task WriteAsync(T value)
        {
            return WriteAsync(value, CancellationToken.None);
        }

        public async Task WriteAsync(T value, CancellationToken cancellationToken)
        {
            _jsonSerializer.Serialize(_jsonResponseStreamWriter, value);
            await _textResponseStreamWriter.WriteAsync("\n");
            await _textResponseStreamWriter.FlushAsync();
        }

        public void Dispose()
        {
            _textResponseStreamWriter?.Dispose();
            ((IDisposable)_jsonResponseStreamWriter)?.Dispose();
        }
    }
}
