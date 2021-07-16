using System;
using System.Buffers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ndjson.AsyncStreams.AspNetCore.Mvc.NewtonsoftJson.Formatters;

namespace Microsoft.Extensions.DependencyInjection
{
    internal class NewtonsoftNdjsonMvcOptionsSetup : IConfigureOptions<MvcOptions>
    {
        private readonly IOptions<MvcNewtonsoftJsonOptions> _jsonOptions;
        private readonly ArrayPool<char> _charPool;
        private readonly ILogger<NewtonsoftNdjsonInputFormatter> _inputFormatterLogger;
        private readonly ILogger<NewtonsoftNdjsonOutputFormatter> _outputFormatterLogger;

        public NewtonsoftNdjsonMvcOptionsSetup(IOptions<MvcNewtonsoftJsonOptions> jsonOptions,
            ArrayPool<char> charPool,
            ILogger<NewtonsoftNdjsonInputFormatter> inputFormatterLogger,
            ILogger<NewtonsoftNdjsonOutputFormatter> outputFormatterLogger)
        {
            _jsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
            _charPool = charPool ?? throw new ArgumentNullException(nameof(charPool));
            _inputFormatterLogger = inputFormatterLogger ?? throw new ArgumentNullException(nameof(inputFormatterLogger));
            _outputFormatterLogger = outputFormatterLogger ?? throw new ArgumentNullException(nameof(outputFormatterLogger));
        }

        public void Configure(MvcOptions options)
        {
            options.InputFormatters.Add(new NewtonsoftNdjsonInputFormatter(_jsonOptions.Value.SerializerSettings, _inputFormatterLogger));
            options.OutputFormatters.Add(new NewtonsoftNdjsonOutputFormatter(_jsonOptions.Value.SerializerSettings, _charPool, _outputFormatterLogger));
        }
    }
}
