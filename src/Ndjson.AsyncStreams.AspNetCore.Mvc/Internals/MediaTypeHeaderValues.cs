using System;
using System.Text;
using Microsoft.Net.Http.Headers;

namespace Ndjson.AsyncStreams.AspNetCore.Mvc.Internals
{
    internal static class MediaTypeHeaderValues
    {
        public const string APPLICATION_NDJSON_MEDIA_TYPE = "application/x-ndjson";
        public const string APPLICATION_JSONL_MEDIA_TYPE = "application/jsonl";

        public static readonly MediaTypeHeaderValue ApplicationNdjson = new MediaTypeHeaderValue(APPLICATION_NDJSON_MEDIA_TYPE).CopyAsReadOnly();

        public static readonly MediaTypeHeaderValue ApplicationNdjsonWithUTF8Encoding = new MediaTypeHeaderValue(APPLICATION_NDJSON_MEDIA_TYPE)
        {
            Encoding = Encoding.UTF8
        }.CopyAsReadOnly();

        public static readonly MediaTypeHeaderValue ApplicationJsonl = new MediaTypeHeaderValue(APPLICATION_JSONL_MEDIA_TYPE).CopyAsReadOnly();

        public static readonly MediaTypeHeaderValue ApplicationJsonlWithUTF8Encoding = new MediaTypeHeaderValue(APPLICATION_JSONL_MEDIA_TYPE)
        {
            Encoding = Encoding.UTF8
        }.CopyAsReadOnly();

        public static bool IsSupportedMediaType(string? mediaType)
        {
            if (mediaType is null)
            {
                return false;
            }

            return mediaType.Equals(APPLICATION_NDJSON_MEDIA_TYPE, StringComparison.OrdinalIgnoreCase) || mediaType.Equals(APPLICATION_JSONL_MEDIA_TYPE, StringComparison.OrdinalIgnoreCase);
        }
    }
}
