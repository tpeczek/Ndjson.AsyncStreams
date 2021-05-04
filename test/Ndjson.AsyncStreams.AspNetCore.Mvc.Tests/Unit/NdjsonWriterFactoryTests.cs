using System;
using System.IO;
using System.Text;
using System.Buffers;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Ndjson.AsyncStreams.AspNetCore.Mvc.Internals;
using Ndjson.AsyncStreams.AspNetCore.Mvc.NewtonsoftJson.Internals;
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

        public static IEnumerable<object[]> NdjsonWriterFactories => new List<object[]>
        {
            new object[] { PrepareNdjsonWriterFactory() },
            new object[] { PrepareNewtonsoftNdjsonWriterFactory() }
        };

        private static INdjsonWriterFactory PrepareNdjsonWriterFactory()
        {
            return new NdjsonWriterFactory(Options.Create(new JsonOptions()));
        }

        private static INdjsonWriterFactory PrepareNewtonsoftNdjsonWriterFactory()
        {
            Mock<IHttpResponseStreamWriterFactory> httpResponseStreamWriterFactory = new ();
            httpResponseStreamWriterFactory.Setup(m => m.CreateWriter(It.IsAny<Stream>(), It.IsIn(Encoding.UTF8)))
                .Returns((Stream stream, Encoding encoding) => new StreamWriter(stream));

            return new NewtonsoftNdjsonWriterFactory(
                httpResponseStreamWriterFactory.Object,
                Options.Create(new MvcNewtonsoftJsonOptions()),
                ArrayPool<char>.Create()
            );
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

        [Theory]
        [MemberData(nameof(NdjsonWriterFactories))]
        public void CreateWriter_ContextIsNull_ThrowsArgumentNullException(INdjsonWriterFactory ndjsonWriterFactory)
        {
            Assert.Throws<ArgumentNullException>("context", () =>
            {
                ndjsonWriterFactory.CreateWriter<ValueType>(null, OK_RESULT);
            });
        }

        [Theory]
        [MemberData(nameof(NdjsonWriterFactories))]
        public void CreateWriter_ResultIsNull_ThrowsArgumentNullException(INdjsonWriterFactory ndjsonWriterFactory)
        {
            Assert.Throws<ArgumentNullException>("result", () =>
            {
                ndjsonWriterFactory.CreateWriter<ValueType>(PrepareActionContext(), null);
            });
        }

        [Theory]
        [MemberData(nameof(NdjsonWriterFactories))]
        public void CreateWriter_ResponseContentTypeIsSetToNdjsonWithUtf8Encoding(INdjsonWriterFactory ndjsonWriterFactory)
        {
            ActionContext actionContext = PrepareActionContext();

            ndjsonWriterFactory.CreateWriter<ValueType>(actionContext, OK_RESULT);

            Assert.Equal(CONTENT_TYPE, actionContext.HttpContext.Response.ContentType);
        }

        [Theory]
        [MemberData(nameof(NdjsonWriterFactories))]
        public void CreateWriter_ResponseStatusCodeIsProvided(INdjsonWriterFactory ndjsonWriterFactory)
        {
            ActionContext actionContext = PrepareActionContext();

            ndjsonWriterFactory.CreateWriter<ValueType>(actionContext, OK_RESULT);

            Assert.Equal(OK_RESULT.StatusCode, actionContext.HttpContext.Response.StatusCode);
        }

        [Theory]
        [MemberData(nameof(NdjsonWriterFactories))]
        public void CreateWriter_DisablesResponseBuffering(INdjsonWriterFactory ndjsonWriterFactory)
        {
            Mock<StreamResponseBodyFeature> httpResponseBodyFeatureMock = new (Stream.Null);
            ActionContext actionContext = PrepareActionContext(httpResponseBodyFeatureMock.Object);

            ndjsonWriterFactory.CreateWriter<ValueType>(actionContext, OK_RESULT);

            httpResponseBodyFeatureMock.Verify(m => m.DisableBuffering(), Times.Once);
        }

        [Theory]
        [MemberData(nameof(NdjsonWriterFactories))]
        public void CreateWriter_CreatesWriter(INdjsonWriterFactory ndjsonWriterFactory)
        {
            ActionContext actionContext = PrepareActionContext();

            INdjsonWriter<ValueType> ndjsonWriter = ndjsonWriterFactory.CreateWriter<ValueType>(actionContext, OK_RESULT);

            Assert.NotNull(ndjsonWriter);
        }
    }
}
