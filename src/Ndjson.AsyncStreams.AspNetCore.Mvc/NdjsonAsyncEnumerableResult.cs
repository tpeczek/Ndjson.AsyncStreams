using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Ndjson.AsyncStreams.AspNetCore.Mvc.Internals;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ndjson.AsyncStreams.AspNetCore.Mvc
{
    /// <summary>
    /// Provides <see cref="ActionResult"/> based on NDJSON/JSONL and <see cref="IAsyncEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the values in async stream to be serialized.</typeparam>
    public class NdjsonAsyncEnumerableResult<T> : ActionResult, IStatusCodeActionResult
    {
        private readonly IAsyncEnumerable<T> _values;
        private readonly string _mediaType;

        /// <summary>
        /// Gets or sets the HTTP status code.
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// Initializes new instance of <see cref="NdjsonAsyncEnumerableResult{T}"/>.
        /// </summary>
        /// <param name="values">The async stream of values to be serialized.</param>
        public NdjsonAsyncEnumerableResult(IAsyncEnumerable<T> values)
            : this(values, MediaTypeHeaderValues.APPLICATION_NDJSON_MEDIA_TYPE) { }

        /// <summary>
        /// Initializes new instance of <see cref="NdjsonAsyncEnumerableResult{T}"/>.
        /// </summary>
        /// <param name="values">The async stream of values to be serialized.</param>
        /// <param name="mediaType">The media type to be used.</param>
        public NdjsonAsyncEnumerableResult(IAsyncEnumerable<T> values, string mediaType)
        {
            _values = values ?? throw new ArgumentNullException(nameof(values));
            _mediaType = _mediaType ?? throw new ArgumentNullException(nameof(mediaType));
        }

        /// <summary>
        /// Executes the result operation of the action method synchronously. This method is called by MVC to process the result of an action method.
        /// </summary>
        /// <param name="context">The <see cref="ActionContext"/> in which the result is executed. The context information includes information about the action that was executed and request information.</param>
        public override void ExecuteResult(ActionContext context)
        {
            throw new NotSupportedException($"The {nameof(NdjsonAsyncEnumerableResult<T>)} doesn't support synchronous execution.");
        }

        /// <summary>
        /// Executes the result operation of the action method asynchronously. This method is called by MVC to process the result of an action method.
        /// </summary>
        /// <param name="context">The <see cref="ActionContext"/> in which the result is executed. The context information includes information about the action that was executed and request information.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous execute operation.</returns>
        public override async Task ExecuteResultAsync(ActionContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            INdjsonWriterFactory ndjsonTextWriterFactory = context.HttpContext.RequestServices.GetRequiredService<INdjsonWriterFactory>();
            using INdjsonWriter<T> ndjsonTextWriter = ndjsonTextWriterFactory.CreateWriter<T>(_mediaType, context, this);

            try
            {
                await foreach (T value in _values.WithCancellation(context.HttpContext.RequestAborted))
                {
                    await ndjsonTextWriter.WriteAsync(value, context.HttpContext.RequestAborted);
                }
            }
            catch (OperationCanceledException) when (context.HttpContext.RequestAborted.IsCancellationRequested) { }
        }
    }
}
