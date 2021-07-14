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
        private readonly ILogger<SystemTextNdjsonInputFormatter> _inputFormatterLogger;
        private readonly ILogger<SystemTextNdjsonOutputFormatter> _outputFormatterLogger;

        public SystemTextNdjsonMvcOptionsSetup(IOptions<JsonOptions>? jsonOptions,
            ILogger<SystemTextNdjsonInputFormatter> inputFormatterLogger,
            ILogger<SystemTextNdjsonOutputFormatter> outputFormatterLogger)
        {
            _jsonOptions = jsonOptions;
            _inputFormatterLogger = inputFormatterLogger ?? throw new ArgumentNullException(nameof(inputFormatterLogger));
            _outputFormatterLogger = outputFormatterLogger ?? throw new ArgumentNullException(nameof(outputFormatterLogger));
        }

        public void Configure(MvcOptions options)
        {
            options.InputFormatters.Add(new SystemTextNdjsonInputFormatter(_jsonOptions?.Value, _inputFormatterLogger));
            options.OutputFormatters.Add(new SystemTextNdjsonOutputFormatter(_jsonOptions?.Value, _outputFormatterLogger));
        }
    }
}
