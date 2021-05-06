using System;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Mvc;
using Ndjson.AsyncStreams.AspNetCore.Mvc;
using Ndjson.AsyncStreams.AspNetCore.Mvc.NewtonsoftJson.Internals;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions methods for configuring MVC via an <see cref="IMvcCoreBuilder"/>.
    /// </summary>
    public static class NewtonsoftNdjsonMvcCoreBuilderExtensions
    {
        /// <summary>
        /// Configures NDJSON support for async streams.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcCoreBuilder"/>.</param>
        /// <returns>The <see cref="IMvcCoreBuilder"/>.</returns>
        public static IMvcCoreBuilder AddNewtonsoftNdjson(this IMvcCoreBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.TryAddSingleton<INdjsonWriterFactory, NewtonsoftNdjsonWriterFactory>();
            builder.Services.AddSingleton<IConfigureOptions<MvcOptions>, NewtonsoftNdjsonMvcOptionsSetup>();

            return builder;
        }
    }
}
