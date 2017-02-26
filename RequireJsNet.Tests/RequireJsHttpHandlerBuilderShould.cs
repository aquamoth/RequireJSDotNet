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
        //[Fact]
        //public void onlyProcessGETandHEADrequests()
        //{
        //    var httpApplication = new MyHttpApplication();
        //    using (new FakeHttpContext.FakeHttpContext())
        //    {
        //        var routeHandler = new RequireJsRouteHandler("unimportant-prefix");
        //        //routeHandler.Get(RequireJsRouteHandler.DEFAULT_CONFIG_NAME).ConfigurationFiles[0] = "..\\..\\TestData\\RequireJsHttpHandlerBuilderShould\\init.json";


        //        var routeData = buildRoute(RequireJsRouteHandler.DEFAULT_CONFIG_NAME, "myEntrypoint");
        //        var httpContext = System.Web.HttpContext.Current;
        //        var httpContextWrapper = new System.Web.HttpContextWrapper(httpContext);
        //        var requestContext = new RequestContext(httpContextWrapper, routeData);
        //        var httpHandler = ((IRouteHandler)routeHandler).GetHttpHandler(requestContext);

        //        httpContext.ApplicationInstance = httpApplication;
        //        httpHandler.ProcessRequest(httpContext);

        //        Assert.False(httpApplication.CompleteRequestCalled);



        //        Assert.Equal((int)System.Net.HttpStatusCode.OK, httpContext.Response.StatusCode);
        //    }
        //}

        [Fact]
        public void returnConfigScriptForValidRequest()
        {
            using (new FakeHttpContext.FakeHttpContext())
            {
                var expectedContent = @"<script>
requireConfig = {""locale"":""sv"",""pageOptions"":{},""websiteOptions"":{}};
require = {""baseUrl"":""/Scripts/"",""locale"":""sv"",""urlArgs"":null,""waitSeconds"":7,""paths"":{},""packages"":[],""shim"":{},""map"":{}};
</script>";
                var configurations = testConfigurations();
                var routeData = buildRoute(RequireJsRouteHandler.DEFAULT_CONFIG_NAME, "myEntrypoint");
                var builder = new RequireJsHttpHandlerBuilder(configurations, routeData);

                var success = builder.ProcessRequest(System.Web.HttpContext.Current);

                Assert.True(success);
                Assert.Equal("text/javascript", builder.ContentType);
                Assert.Equal(Encoding.UTF8, builder.ContentEncoding);
                Assert.Equal((int)System.Net.HttpStatusCode.OK, builder.StatusCode);
                Assert.Equal(expectedContent, builder.Content);
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
