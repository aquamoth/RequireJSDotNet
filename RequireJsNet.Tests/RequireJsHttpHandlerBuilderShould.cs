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
            using (new FakeHttpContext.FakeHttpContext())
            {
                System.Web.HttpContext.Current.Request.RequestType = "POST";

                var configurations = testConfigurations();
                var routeData = buildRoute(RequireJsRouteHandler.DEFAULT_CONFIG_NAME, "myEntrypoint");
                var builder = new RequireJsHttpHandlerBuilder(configurations, routeData);

                var processed = builder.ProcessRequest(System.Web.HttpContext.Current);

                Assert.True(processed);
                Assert.Equal((int)System.Net.HttpStatusCode.MethodNotAllowed, builder.StatusCode);
            }
        }

        [Fact]
        public void returnConfigScriptForValidRequest()
        {
            using (new FakeHttpContext.FakeHttpContext())
            {
                var configurations = testConfigurations();

                var expectedHeaders = new Dictionary<string, string>() {
                    { "Last-Modified", RequireJsHttpHandlerBuilder.GmtString(configurations.Single().Value.LastModified) },
                    { "ETag", configurations.Single().Value.LastModified.Ticks.ToString() }
                };

                var expectedContent = @"<script>
requireConfig = {""locale"":""sv"",""pageOptions"":{},""websiteOptions"":{}};
require = {""baseUrl"":""/Scripts/"",""locale"":""sv"",""urlArgs"":null,""waitSeconds"":7,""paths"":{},""packages"":[],""shim"":{},""map"":{}};
</script>";
                var routeData = buildRoute(RequireJsRouteHandler.DEFAULT_CONFIG_NAME, "myEntrypoint");
                var builder = new RequireJsHttpHandlerBuilder(configurations, routeData);

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
            using (var fake = new FakeHttpContext.FakeHttpContext())
            {
                var configurations = testConfigurations();

                var lastModified = configurations.Single().Value.LastModified;
                fake.Request.Add("If-Modified-Since", RequireJsHttpHandlerBuilder.GmtString(lastModified.AddSeconds(-1)));


                var routeData = buildRoute(RequireJsRouteHandler.DEFAULT_CONFIG_NAME, "myEntrypoint");
                var builder = new RequireJsHttpHandlerBuilder(configurations, routeData);

                var processed = builder.ProcessRequest(System.Web.HttpContext.Current);

                Assert.True(processed);
                Assert.Equal((int)System.Net.HttpStatusCode.OK, builder.StatusCode);
            }
        }

        [Fact]
        public void returnNotModifiedByTimestamp()
        {
            using (var fake = new FakeHttpContext.FakeHttpContext())
            {
                var configurations = testConfigurations();

                var lastModified = configurations.Single().Value.LastModified;
                fake.Request.Add("If-Modified-Since", RequireJsHttpHandlerBuilder.GmtString(lastModified));


                var routeData = buildRoute(RequireJsRouteHandler.DEFAULT_CONFIG_NAME, "myEntrypoint");
                var builder = new RequireJsHttpHandlerBuilder(configurations, routeData);

                var processed = builder.ProcessRequest(System.Web.HttpContext.Current);

                Assert.True(processed);
                Assert.Equal((int)System.Net.HttpStatusCode.NotModified, builder.StatusCode);
                //Assert.Equal(RequireJsHttpHandlerBuilder.GmtString(lastModified), builder.Headers["Last-Modified"]);
                Assert.Null(builder.Content);
            }
        }

        [Fact]
        public void returnIfNoneMatch()
        {
            using (var fake = new FakeHttpContext.FakeHttpContext())
            {
                var configurations = testConfigurations();

                var lastModified = configurations.Single().Value.LastModified;
                fake.Request.Add("If-Modified-Since", RequireJsHttpHandlerBuilder.GmtString(lastModified));
                fake.Request.Add("If-None-Match", "UNMATCHED-ETAG");


                var routeData = buildRoute(RequireJsRouteHandler.DEFAULT_CONFIG_NAME, "myEntrypoint");
                var builder = new RequireJsHttpHandlerBuilder(configurations, routeData);

                var processed = builder.ProcessRequest(System.Web.HttpContext.Current);

                Assert.True(processed);
                Assert.Equal((int)System.Net.HttpStatusCode.OK, builder.StatusCode);
            }
        }

        [Fact]
        public void returnNotModifiedByETag()
        {
            using (var fake = new FakeHttpContext.FakeHttpContext())
            {
                var configurations = testConfigurations();

                var lastModified = configurations.Single().Value.LastModified;
                fake.Request.Add("If-Modified-Since", RequireJsHttpHandlerBuilder.GmtString(lastModified));
                fake.Request.Add("If-None-Match", configurations.Single().Value.Hashcode);


                var routeData = buildRoute(RequireJsRouteHandler.DEFAULT_CONFIG_NAME, "myEntrypoint");
                var builder = new RequireJsHttpHandlerBuilder(configurations, routeData);

                var processed = builder.ProcessRequest(System.Web.HttpContext.Current);

                Assert.True(processed);
                Assert.Equal((int)System.Net.HttpStatusCode.NotModified, builder.StatusCode);
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

        private static RouteData buildRoute(string configName, string entryPoint)
        {
            var routeData = new RouteData();
            routeData.Values.Add("configName", configName);
            routeData.Values.Add("entrypoint", entryPoint);
            return routeData;
        }
    }
}
