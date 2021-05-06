using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ndjson.AsyncStreams.AspNetCore.Mvc.Formatters;

namespace Microsoft.Extensions.DependencyInjection
{
    internal class SystemTextNdjsonMvcOptionsSetup : IConfigureOptions<MvcOptions>
    {
        private readonly IOptions<JsonOptions>? _jsonOptions;
        private readonly ILogger<SystemTextNdjsonInputFormatter> _logger;

        public SystemTextNdjsonMvcOptionsSetup(IOptions<JsonOptions>? jsonOptions, ILogger<SystemTextNdjsonInputFormatter> logger)
        {
            _jsonOptions = jsonOptions;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger)); ;
        }

        public void Configure(MvcOptions options)
        {
            options.InputFormatters.Add(new SystemTextNdjsonInputFormatter(_jsonOptions?.Value, _logger));
        }
    }
}
