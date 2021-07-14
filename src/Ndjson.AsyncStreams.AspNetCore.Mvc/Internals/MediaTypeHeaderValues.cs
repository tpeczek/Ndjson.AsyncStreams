using System.Text;
using Microsoft.Net.Http.Headers;

namespace Ndjson.AsyncStreams.AspNetCore.Mvc.Internals
{
    internal static class MediaTypeHeaderValues
    {
        private const string APPLICATION_NDJSON_MEDIA_TYPE = "application/x-ndjson";

        public static readonly MediaTypeHeaderValue ApplicationNdjson = new MediaTypeHeaderValue(APPLICATION_NDJSON_MEDIA_TYPE).CopyAsReadOnly();

        public static readonly MediaTypeHeaderValue ApplicationNdjsonWithUTF8Encoding = new MediaTypeHeaderValue(APPLICATION_NDJSON_MEDIA_TYPE)
        {
            Encoding = Encoding.UTF8
        }.CopyAsReadOnly();
    }
}
