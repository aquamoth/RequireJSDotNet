using RequireJsNet.HttpModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;
using Xunit;

namespace RequireJsNet.Tests
{
    public class RequireJsHttpHandlerBuilderShould
    {
        [Fact]
        public void FormatsDatesToGmt()
        {
            var dt = new DateTime(2011, 10, 18, 22, 17, 53, DateTimeKind.Local);
            var actual = RequireJsHttpHandlerBuilder.GmtString(dt);
            Assert.Equal("Tue, 18 Oct 2011 15:17:53 GMT", actual);
        }

        [Fact]
        public void onlyProcessGETandHEADrequests()
        {
            var configurations = testConfigurations();
            //var lastModified = configurations.Single().Value.LastModified;
            using (new FakeHttpContext.FakeHttpContext())
            {
                System.Web.HttpContext.Current.Request.RequestType = "POST";

                var builder = createBuilder(configurations);
                var processed = builder.ProcessRequest(System.Web.HttpContext.Current);

                Assert.True(processed);
                Assert.Equal((int)System.Net.HttpStatusCode.MethodNotAllowed, builder.StatusCode);
            }
        }

        [Fact]
        public void returnConfigScriptForValidRequest()
        {
            var configurations = testConfigurations();
            var lastModified = configurations.Single().Value.LastModified;
            using (new FakeHttpContext.FakeHttpContext())
            {
                var expectedHeaders = new Dictionary<string, string>() {
                    { "Last-Modified", RequireJsHttpHandlerBuilder.GmtString(lastModified) },
                    { "ETag", lastModified.Ticks.ToString() }
                };

                var expectedContent = @"
requireConfig = {""locale"":""sv"",""pageOptions"":{},""websiteOptions"":{}};
require = {""baseUrl"":""/Scripts/"",""locale"":""sv"",""urlArgs"":null,""waitSeconds"":7,""paths"":{},""packages"":[],""shim"":{},""map"":{}};
";

                var builder = createBuilder(configurations);
                var processed = builder.ProcessRequest(System.Web.HttpContext.Current);

                Assert.True(processed);
                Assert.Equal((int)System.Net.HttpStatusCode.OK, builder.StatusCode);
                Assert.Equal("text/javascript", builder.ContentType);
                Assert.Equal(Encoding.UTF8, builder.ContentEncoding);
                Assert.Equal(expectedHeaders, builder.Headers);
                Assert.Equal(expectedContent, builder.Content);
            }
        }

        [Fact]
        public void returnIfModifiedSince()
        {
            var configurations = testConfigurations();
            var lastModified = configurations.Single().Value.LastModified;
            using (var fake = new FakeHttpContext.FakeHttpContext())
            {
                fake.Request.Add("If-Modified-Since", RequireJsHttpHandlerBuilder.GmtString(lastModified.AddSeconds(-1)));

                var builder = createBuilder(configurations);
                var processed = builder.ProcessRequest(System.Web.HttpContext.Current);

                Assert.True(processed);
                Assert.Equal((int)System.Net.HttpStatusCode.OK, builder.StatusCode);
                Assert.Equal("text/javascript", builder.ContentType);
                Assert.Equal(Encoding.UTF8, builder.ContentEncoding);
            }
        }

        [Fact]
        public void returnNotModifiedByTimestamp()
        {
            var configurations = testConfigurations();
            var lastModified = configurations.Single().Value.LastModified;
            using (var fake = new FakeHttpContext.FakeHttpContext())
            {
                fake.Request.Add("If-Modified-Since", RequireJsHttpHandlerBuilder.GmtString(lastModified));

                var builder = createBuilder(configurations);
                var processed = builder.ProcessRequest(System.Web.HttpContext.Current);

                Assert.True(processed);
                Assert.Equal((int)System.Net.HttpStatusCode.NotModified, builder.StatusCode);
                Assert.Equal("text/javascript", builder.ContentType);
                Assert.Equal(Encoding.UTF8, builder.ContentEncoding);
                //Assert.Equal(RequireJsHttpHandlerBuilder.GmtString(lastModified), builder.Headers["Last-Modified"]);
                Assert.Null(builder.Content);
            }
        }

        [Fact]
        public void returnIfNoneMatch()
        {
            var configurations = testConfigurations();
            var lastModified = configurations.Single().Value.LastModified;
            using (var fake = new FakeHttpContext.FakeHttpContext())
            {
                fake.Request.Add("If-Modified-Since", RequireJsHttpHandlerBuilder.GmtString(lastModified));
                fake.Request.Add("If-None-Match", "UNMATCHED-ETAG");

                var builder = createBuilder(configurations);
                var processed = builder.ProcessRequest(System.Web.HttpContext.Current);

                Assert.True(processed);
                Assert.Equal((int)System.Net.HttpStatusCode.OK, builder.StatusCode);
                Assert.Equal("text/javascript", builder.ContentType);
                Assert.Equal(Encoding.UTF8, builder.ContentEncoding);
            }
        }

        [Fact]
        public void returnNotModifiedByETag()
        {
            var configurations = testConfigurations();
            var lastModified = configurations.Single().Value.LastModified;
            using (var fake = new FakeHttpContext.FakeHttpContext())
            {
                fake.Request.Add("If-Modified-Since", RequireJsHttpHandlerBuilder.GmtString(lastModified));
                fake.Request.Add("If-None-Match", configurations.Single().Value.Hashcode);

                var builder = createBuilder(configurations);
                var processed = builder.ProcessRequest(System.Web.HttpContext.Current);

                Assert.True(processed);
                Assert.Equal((int)System.Net.HttpStatusCode.NotModified, builder.StatusCode);
                Assert.Equal("text/javascript", builder.ContentType);
                Assert.Equal(Encoding.UTF8, builder.ContentEncoding);
                Assert.Null(builder.Content);
            }
        }

        private static IReadOnlyDictionary<string, RequireRendererConfiguration> testConfigurations()
        {
            var routeHandler = new RequireJsRouteHandler("unimportant-prefix");
            routeHandler.Get(RequireJsRouteHandler.DEFAULT_CONFIG_NAME).ConfigurationFiles[0] = "..\\..\\TestData\\RequireJsHttpHandlerBuilderShould\\init.json";
            var configurations = routeHandler.Configurations;
            return configurations;
        }

        private static RequireJsHttpHandlerBuilder createBuilder(IReadOnlyDictionary<string, RequireRendererConfiguration> configurations)
        {
            var routeData = buildRoute(RequireJsRouteHandler.DEFAULT_CONFIG_NAME, "myEntrypoint");
            var builder = new RequireJsHttpHandlerBuilder(configurations, routeData);
            return builder;
        }

        private static RouteData buildRoute(string configName, string entryPoint)
        {
            var routeData = new RouteData();
            routeData.Values.Add("configName", configName);
            routeData.Values.Add("entrypoint", entryPoint);
            return routeData;
        }
    }
}
