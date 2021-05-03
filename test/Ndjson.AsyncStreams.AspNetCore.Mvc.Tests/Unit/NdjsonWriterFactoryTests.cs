using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Ndjson.AsyncStreams.AspNetCore.Mvc.Internals;
using Xunit;
using Moq;

namespace Ndjson.AsyncStreams.AspNetCore.Mvc.Tests.Unit
{
    public class NdjsonWriterFactoryTests
    {
        private struct ValueType
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        private static readonly StatusCodeResult OK_RESULT = new OkResult();
        private static readonly string CONTENT_TYPE = new MediaTypeHeaderValue("application/x-ndjson")
        {
            Encoding = Encoding.UTF8
        }.ToString();

        private NdjsonWriterFactory PrepareNdjsonWriterFactory()
        {
            return new NdjsonWriterFactory(Options.Create(new JsonOptions()));
        }

        private ActionContext PrepareActionContext(IHttpResponseBodyFeature httpResponseBodyFeature = null)
        {
            HttpContext httpContext = new DefaultHttpContext();
            if (httpResponseBodyFeature != null)
            {
                httpContext.Features.Set(httpResponseBodyFeature);
            }

            return new ActionContext(
                httpContext,
                new RouteData(),
                new ActionDescriptor()
            );
        }

        [Fact]
        public void CreateWriter_ContextIsNull_ThrowsArgumentNullException()
        {
            NdjsonWriterFactory ndjsonWriterFactory = PrepareNdjsonWriterFactory();

            Assert.Throws<ArgumentNullException>("context", () =>
            {
                ndjsonWriterFactory.CreateWriter<ValueType>(null, OK_RESULT);
            });
        }

        [Fact]
        public void CreateWriter_ResultIsNull_ThrowsArgumentNullException()
        {
            NdjsonWriterFactory ndjsonWriterFactory = PrepareNdjsonWriterFactory();

            Assert.Throws<ArgumentNullException>("result", () =>
            {
                ndjsonWriterFactory.CreateWriter<ValueType>(PrepareActionContext(), null);
            });
        }

        [Fact]
        public void CreateWriter_ResponseContentTypeIsSetToNdjsonWithUtf8Encoding()
        {
            NdjsonWriterFactory ndjsonWriterFactory = PrepareNdjsonWriterFactory();
            ActionContext actionContext = PrepareActionContext();

            ndjsonWriterFactory.CreateWriter<ValueType>(actionContext, OK_RESULT);

            Assert.Equal(CONTENT_TYPE, actionContext.HttpContext.Response.ContentType);
        }

        [Fact]
        public void CreateWriter_ResponseStatusCodeIsProvided()
        {
            NdjsonWriterFactory ndjsonWriterFactory = PrepareNdjsonWriterFactory();
            ActionContext actionContext = PrepareActionContext();

            ndjsonWriterFactory.CreateWriter<ValueType>(actionContext, OK_RESULT);

            Assert.Equal(OK_RESULT.StatusCode, actionContext.HttpContext.Response.StatusCode);
        }

        [Fact]
        public void CreateWriter_DisablesResponseBuffering()
        {
            NdjsonWriterFactory ndjsonWriterFactory = PrepareNdjsonWriterFactory();
            Mock<IHttpResponseBodyFeature> httpResponseBodyFeatureMock = new Mock<IHttpResponseBodyFeature>();
            ActionContext actionContext = PrepareActionContext(httpResponseBodyFeatureMock.Object);

            ndjsonWriterFactory.CreateWriter<ValueType>(actionContext, OK_RESULT);

            httpResponseBodyFeatureMock.Verify(m => m.DisableBuffering(), Times.Once);
        }

        [Fact]
        public void CreateWriter_CreatesWriter()
        {
            NdjsonWriterFactory ndjsonWriterFactory = PrepareNdjsonWriterFactory();
            ActionContext actionContext = PrepareActionContext();

            INdjsonWriter<ValueType> ndjsonWriter = ndjsonWriterFactory.CreateWriter<ValueType>(actionContext, OK_RESULT);

            Assert.NotNull(ndjsonWriter);
        }
    }
}
