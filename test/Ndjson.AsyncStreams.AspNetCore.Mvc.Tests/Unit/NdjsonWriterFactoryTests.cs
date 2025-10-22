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
        private const string NDJSON_MEDIA_TYPE = "application/x-ndjson";
        private const string JSONL_MEDIA_TYPE = "application/jsonl";

        public static IEnumerable<object[]> WriterFactories => new List<object[]>
        {
            new object[] { PrepareSystemTextNdjsonWriterFactory() },
            new object[] { PrepareNewtonsoftNdjsonWriterFactory() }
        };

        public static IEnumerable<object[]> WriterFactoriesAndMediaTypesMatrix => new List<object[]>
        {
            new object[] { PrepareSystemTextNdjsonWriterFactory(), NDJSON_MEDIA_TYPE },
            new object[] { PrepareSystemTextNdjsonWriterFactory(), JSONL_MEDIA_TYPE },
            new object[] { PrepareNewtonsoftNdjsonWriterFactory(), NDJSON_MEDIA_TYPE },
            new object[] { PrepareNewtonsoftNdjsonWriterFactory(), JSONL_MEDIA_TYPE }
        };

        private static INdjsonWriterFactory PrepareSystemTextNdjsonWriterFactory()
        {
            return new SystemTextNdjsonWriterFactory(Options.Create(new JsonOptions()));
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
        [MemberData(nameof(WriterFactories))]
        public void CreateWriter_ContextIsNull_ThrowsArgumentNullException(INdjsonWriterFactory writerFactory)
        {
            Assert.Throws<ArgumentNullException>("context", () =>
            {
                writerFactory.CreateWriter<ValueType>(null, OK_RESULT);
            });
        }

        [Theory]
        [MemberData(nameof(WriterFactories))]
        public void CreateWriter_ResultIsNull_ThrowsArgumentNullException(INdjsonWriterFactory writerFactory)
        {
            Assert.Throws<ArgumentNullException>("result", () =>
            {
                writerFactory.CreateWriter<ValueType>(PrepareActionContext(), null);
            });
        }

        [Theory]
        [MemberData(nameof(WriterFactoriesAndMediaTypesMatrix))]
        public void CreateWriter_ResponseContentTypeIsSetToMediaTypeWithUtf8Encoding(INdjsonWriterFactory writerFactory, string mediaType)
        {
            ActionContext actionContext = PrepareActionContext();
            string mediaTypeWithUtf8Encoding = new MediaTypeHeaderValue(mediaType) { Encoding = Encoding.UTF8 }.ToString();

            writerFactory.CreateWriter<ValueType>(mediaType, actionContext, OK_RESULT);

            Assert.Equal(mediaTypeWithUtf8Encoding, actionContext.HttpContext.Response.ContentType);
        }

        [Theory]
        [MemberData(nameof(WriterFactories))]
        public void CreateWriter_ResponseStatusCodeIsProvided(INdjsonWriterFactory writerFactory)
        {
            ActionContext actionContext = PrepareActionContext();

            writerFactory.CreateWriter<ValueType>(actionContext, OK_RESULT);

            Assert.Equal(OK_RESULT.StatusCode, actionContext.HttpContext.Response.StatusCode);
        }

        [Theory]
        [MemberData(nameof(WriterFactories))]
        public void CreateWriter_DisablesResponseBuffering(INdjsonWriterFactory writerFactory)
        {
            Mock<StreamResponseBodyFeature> httpResponseBodyFeatureMock = new (Stream.Null);
            ActionContext actionContext = PrepareActionContext(httpResponseBodyFeatureMock.Object);

            writerFactory.CreateWriter<ValueType>(actionContext, OK_RESULT);

            httpResponseBodyFeatureMock.Verify(m => m.DisableBuffering(), Times.Once);
        }

        [Theory]
        [MemberData(nameof(WriterFactories))]
        public void CreateWriter_CreatesWriter(INdjsonWriterFactory writerFactory)
        {
            ActionContext actionContext = PrepareActionContext();

            INdjsonWriter<ValueType> writer = writerFactory.CreateWriter<ValueType>(actionContext, OK_RESULT);

            Assert.NotNull(writer);
        }
    }
}
