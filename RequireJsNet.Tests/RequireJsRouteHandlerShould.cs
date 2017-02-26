using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;
using Xunit;

namespace RequireJsNet.Tests
{
    public class RequireJsRouteHandlerShould
    {
        [Fact]
        public void requiresPrefixToInitialize()
        {
            Assert.Throws<ArgumentException>(() => new HttpModule.RequireJsRouteHandler(null));
            Assert.Throws<ArgumentException>(() => new HttpModule.RequireJsRouteHandler(""));
        }

        [Fact]
        public void initializeWithPrefix()
        {
            var expected = "testPrefix";

            var handler = new HttpModule.RequireJsRouteHandler(expected);

            Assert.IsAssignableFrom<IRouteHandler>(handler);
            Assert.Equal(expected, handler.RoutePrefix);
        }

        [Fact]
        public void containDefaultConfig()
        {
            var handler = new HttpModule.RequireJsRouteHandler("unimportant-prefix");
            var config = handler.Get(HttpModule.RequireJsRouteHandler.DEFAULT_CONFIG_NAME);

            Assert.Equal("/Scripts/require.js", config.RequireJsUrl);
            Assert.Equal("/Scripts/", config.BaseUrl);
            Assert.Equal(Configuration.ConfigCachingPolicy.None, config.ConfigCachingPolicy);
        }

        [Fact]
        public void storeRegisteredConfigs()
        {
            var expectedName = "myconfig";
            var expectedConfig = new RequireRendererConfiguration();
            var handler = new HttpModule.RequireJsRouteHandler("unimportant-prefix");

            Assert.Throws<ArgumentNullException>(() => handler.RegisterConfig(null, expectedConfig));
            Assert.Throws<ArgumentNullException>(() => handler.RegisterConfig(expectedName, null));

            handler.RegisterConfig(expectedName, expectedConfig);
            var actualConfig = handler.Get(expectedName);

            Assert.Same(expectedConfig, actualConfig);
        }

        [Fact]
        public void provideHttpHandlerForRequest()
        {
            IRouteHandler handler = new HttpModule.RequireJsRouteHandler("unimportant-prefix");

            var httpHandler = handler.GetHttpHandler(new RequestContext());

            Assert.NotNull(httpHandler);
        }

        [Fact]
        public void registerDefaultRoute()
        {
            var expectedUrl = $"{HttpModule.RequireJsRouteHandler.DEFAULT_ROUTE_PREFIX}/{{{HttpModule.RequireJsRouteHandler.URL_CONFIGNAME_NAME}}}/{{*{HttpModule.RequireJsRouteHandler.URL_ENTRYPOINT_NAME}}}";

            var routes = new RouteCollection();
            var expectedHandler = HttpModule.RequireJsRouteHandler.RegisterRoutes(routes);

            Assert.Equal(1, routes.Count);
            var actualRoute = routes[0] as Route;
            Assert.Equal(expectedUrl, actualRoute.Url);
            Assert.Equal(expectedHandler, actualRoute.RouteHandler);
        }
    }
}
