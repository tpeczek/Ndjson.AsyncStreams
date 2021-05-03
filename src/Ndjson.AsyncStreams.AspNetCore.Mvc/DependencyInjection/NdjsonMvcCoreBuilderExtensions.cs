using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ndjson.AsyncStreams.AspNetCore.Mvc;
using Ndjson.AsyncStreams.AspNetCore.Mvc.Internals;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for adding Newtonsoft.Json to <see cref="IMvcCoreBuilder"/>.
    /// </summary>
    public static class NdjsonMvcCoreBuilderExtensions
    {
        /// <summary>
        /// Configures NDJSON specific action results.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
        /// <returns>The <see cref="IMvcBuilder"/>.</returns>
        public static IMvcCoreBuilder AddNdjson(this IMvcCoreBuilder builder)
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
