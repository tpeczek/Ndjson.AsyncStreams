using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ndjson.AsyncStreams.AspNetCore.Mvc;
using Ndjson.AsyncStreams.AspNetCore.Mvc.Internals;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions methods for configuring MVC via an <see cref="IMvcBuilder"/>.
    /// </summary>
    public static class NdjsonMvcBuilderExtensions
    {
        /// <summary>
        /// Configures NDJSON specific action results.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
        /// <returns>The <see cref="IMvcBuilder"/>.</returns>
        public static IMvcBuilder AddNdjson(this IMvcBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.TryAddSingleton<INdjsonWriterFactory, NdjsonWriterFactory>();

            return builder;
        }
    }
}
