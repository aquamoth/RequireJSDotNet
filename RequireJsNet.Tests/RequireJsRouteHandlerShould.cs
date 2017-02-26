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
            Assert.Throws<ArgumentException>(() => new RequireJsRouteHandler(null));
            Assert.Throws<ArgumentException>(() => new RequireJsRouteHandler(""));
        }

        [Fact]
        public void initializeWithPrefix()
        {
            var expected = "testPrefix";

            var handler = new RequireJsRouteHandler(expected);

            Assert.IsAssignableFrom<IRouteHandler>(handler);
            Assert.Equal(expected, handler.RoutePrefix);
        }

        [Fact]
        public void containDefaultConfig()
        {
            var handler = new RequireJsRouteHandler("unimportant-prefix");
            var config = handler.Get(RequireJsRouteHandler.DEFAULT_CONFIG_NAME);

            Assert.Equal("/Scripts/require.js", config.RequireJsUrl);
            Assert.Equal("/Scripts/", config.BaseUrl);
            Assert.Equal(Configuration.ConfigCachingPolicy.None, config.ConfigCachingPolicy);
        }

        [Fact]
        public void storeRegisteredConfigs()
        {
            var expectedName = "myconfig";
            var expectedConfig = new RequireRendererConfiguration();
            var handler = new RequireJsRouteHandler("unimportant-prefix");

            Assert.Throws<ArgumentNullException>(() => handler.RegisterConfig(null, expectedConfig));
            Assert.Throws<ArgumentNullException>(() => handler.RegisterConfig(expectedName, null));

            handler.RegisterConfig(expectedName, expectedConfig);
            var actualConfig = handler.Get(expectedName);

            Assert.Same(expectedConfig, actualConfig);
        }

        [Fact]
        public void provideHttpHandlerForRequest()
        {
            IRouteHandler handler = new RequireJsRouteHandler("unimportant-prefix");

            var httpHandler = handler.GetHttpHandler(new RequestContext());

            Assert.NotNull(httpHandler);
        }

    }
}
