using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ndjson.AsyncStreams.AspNetCore.Mvc
{
    /// <summary>
    /// An interface for NDJSON writer.
    /// </summary>
    /// <typeparam name="T">The type of the value to be written.</typeparam>
    public interface INdjsonWriter<T> : IDisposable
    {
        /// <summary>
        /// Writes a value.
        /// </summary>
        /// <param name="value">The value to be written.</param>
        /// <returns>The <see cref="Task"/> object representing the asynchronous operation.</returns>
        Task WriteAsync(T value);

        /// <summary>
        /// Writes a value.
        /// </summary>
        /// <param name="value">The value to be written.</param>
        /// <param name="cancellationToken">The token that may be used to cancel the write operation.</param>
        /// <returns>The <see cref="Task"/> object representing the asynchronous operation.</returns>
        Task WriteAsync(T value, CancellationToken cancellationToken);
    }
}
