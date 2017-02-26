using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;
using Xunit;

namespace RequireJsNet.Tests
{
    public class RequireJsHttpHandlerShould
    {
        [Fact]
        public void returnConfigScriptFor()
        {
            using (new FakeHttpContext.FakeHttpContext())
            {
                var actualContent = new StringBuilder();
                System.Web.HttpContext.Current.Response.Output = new System.IO.StringWriter(actualContent);



                var expectedContent = @"<script>
requireConfig = {""locale"":""sv"",""pageOptions"":{},""websiteOptions"":{}};
require = {""baseUrl"":""/Scripts/"",""locale"":""sv"",""urlArgs"":null,""waitSeconds"":7,""paths"":{},""packages"":[],""shim"":{},""map"":{}};
</script>";

                var routeHandler = new RequireJsRouteHandler("unimportant-prefix");
                routeHandler.Get(RequireJsRouteHandler.DEFAULT_CONFIG_NAME).ConfigurationFiles[0] = "..\\..\\TestData\\RequireJsHttpHandlerShould\\init.json";


                var routeData = buildRoute(RequireJsRouteHandler.DEFAULT_CONFIG_NAME, "myEntrypoint");
                var httpContext = System.Web.HttpContext.Current;
                var httpContextWrapper = new System.Web.HttpContextWrapper(httpContext);
                var requestContext = new RequestContext(httpContextWrapper, routeData);
                var httpHandler = ((IRouteHandler)routeHandler).GetHttpHandler(requestContext);

                httpHandler.ProcessRequest(httpContext);


                Assert.Equal("text/javascript", httpContext.Response.ContentType);
                Assert.Equal(Encoding.UTF8, httpContext.Response.ContentEncoding);
                Assert.Equal((int)System.Net.HttpStatusCode.OK, httpContext.Response.StatusCode);
                Assert.Equal(expectedContent, actualContent.ToString());
            }
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
