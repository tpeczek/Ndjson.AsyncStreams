using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ndjson.AsyncStreams.AspNetCore.Mvc;
using Ndjson.AsyncStreams.AspNetCore.Mvc.Internals;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions methods for configuring MVC via an <see cref="IMvcCoreBuilder"/>.
    /// </summary>
    public static class SystemTextNdjsonMvcCoreBuilderExtensions
    {
        /// <summary>
        /// Configures NDJSON support for async streams.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcCoreBuilder"/>.</param>
        /// <returns>The <see cref="IMvcCoreBuilder"/>.</returns>
        public static IMvcCoreBuilder AddNdjson(this IMvcCoreBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.TryAddSingleton<INdjsonWriterFactory, SystemTextNdjsonWriterFactory>();
            builder.Services.AddSingleton<IConfigureOptions<MvcOptions>, SystemTextNdjsonMvcOptionsSetup>();

            return builder;
        }
    }
}
