using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Ndjson.AsyncStreams.AspNetCore.Mvc
{
    /// <summary>
    /// An interface for <see cref="INdjsonWriter{T}"/> factory.
    /// </summary>
    public interface INdjsonWriterFactory
    {
        /// <summary>
        /// Creates a new <see cref="INdjsonWriter{T}"/>.
        /// </summary>
        /// <param name="context">The <see cref="ActionContext"/>.</param>
        /// <param name="result">The <see cref="IStatusCodeActionResult"/>.</param>
        /// <returns>The new <see cref="INdjsonWriter{T}"/>.</returns>
        INdjsonWriter<T> CreateWriter<T>(ActionContext context, IStatusCodeActionResult result);
    }
}
