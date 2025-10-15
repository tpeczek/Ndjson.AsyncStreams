using System;

namespace Ndjson.AsyncStreams.Net.Http.Internals
{
    internal class MediaTypeHeaderValues
    {
        internal const string APPLICATION_NDJSON_MEDIA_TYPE = "application/x-ndjson";

        internal const string APPLICATION_JSONL_MEDIA_TYPE = "application/jsonl";

        internal static bool IsSupportedMediaType(string? mediaType)
        {
            if (mediaType is null)
            {
                return false;
            }

            return mediaType.Equals(APPLICATION_NDJSON_MEDIA_TYPE, StringComparison.OrdinalIgnoreCase) || mediaType.Equals(APPLICATION_JSONL_MEDIA_TYPE, StringComparison.OrdinalIgnoreCase);
        }
    }
}
