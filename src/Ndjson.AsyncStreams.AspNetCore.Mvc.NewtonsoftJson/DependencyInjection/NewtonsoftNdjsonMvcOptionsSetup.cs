using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ndjson.AsyncStreams.AspNetCore.Mvc.NewtonsoftJson.Formatters;

namespace Microsoft.Extensions.DependencyInjection
{
    internal class NewtonsoftNdjsonMvcOptionsSetup : IConfigureOptions<MvcOptions>
    {
        private readonly IOptions<MvcNewtonsoftJsonOptions> _jsonOptions;
        private readonly ILogger<NewtonsoftNdjsonInputFormatter> _logger;

        public NewtonsoftNdjsonMvcOptionsSetup(IOptions<MvcNewtonsoftJsonOptions> jsonOptions, ILogger<NewtonsoftNdjsonInputFormatter> logger)
        {
            _jsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Configure(MvcOptions options)
        {
            options.InputFormatters.Add(new NewtonsoftNdjsonInputFormatter(_jsonOptions.Value.SerializerSettings, _logger));
        }
    }
}
