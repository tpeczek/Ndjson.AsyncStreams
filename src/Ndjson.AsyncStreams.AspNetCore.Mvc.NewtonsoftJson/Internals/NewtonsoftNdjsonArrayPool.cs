using System;
using System.Buffers;
using Newtonsoft.Json;

namespace Ndjson.AsyncStreams.AspNetCore.Mvc.NewtonsoftJson.Internals
{
    internal class NewtonsoftNdjsonArrayPool : IArrayPool<char>
    {
        private readonly ArrayPool<char> _inner;

        public NewtonsoftNdjsonArrayPool(ArrayPool<char> inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public char[] Rent(int minimumLength)
        {
            return _inner.Rent(minimumLength);
        }

        public void Return(char[]? array)
        {
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            _inner.Return(array);
        }
    }
}
