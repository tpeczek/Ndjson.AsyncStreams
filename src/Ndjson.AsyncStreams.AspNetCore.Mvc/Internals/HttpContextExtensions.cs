using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Ndjson.AsyncStreams.AspNetCore.Mvc.Internals
{
    internal static class HttpContextExtensions
    {
        public static void DisableResponseBuffering(this HttpContext context)
        {
            IHttpResponseBodyFeature responseBodyFeature = context.Features.Get<IHttpResponseBodyFeature>();
            if (responseBodyFeature != null)
            {
                responseBodyFeature.DisableBuffering();
            }
        }
    }
}
