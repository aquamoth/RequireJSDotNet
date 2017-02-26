using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;

namespace RequireJsNet
{
    public class RequireJsRouteHandler : IRouteHandler
    {
        public const string DEFAULT_CONFIG_NAME = "default";

        readonly Dictionary<string, RequireRendererConfiguration> _configurations;

        public string RoutePrefix { get; private set; }

        internal RequireJsRouteHandler(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                throw new ArgumentException(nameof(prefix));

            this.RoutePrefix = prefix;

            _configurations = new Dictionary<string, RequireRendererConfiguration>();
            RegisterConfig(DEFAULT_CONFIG_NAME, DefaultConfig());
        }

        internal static RequireRendererConfiguration DefaultConfig()
        {
            return new RequireRendererConfiguration
            {
                RequireJsUrl = "/Scripts/require.js",
                BaseUrl = "/Scripts/",
                ConfigCachingPolicy = Configuration.ConfigCachingPolicy.None
            };
        }

        public void RegisterConfig(string name, RequireRendererConfiguration config)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (config == null)
                throw new ArgumentNullException(nameof(config));

            //if (config.ConfigurationFiles == null || !config.ConfigurationFiles.Any())
            //    throw new ArgumentNullException(nameof(config.ConfigurationFiles));

            _configurations.Add(name, config);
        }

        internal RequireRendererConfiguration Get(string name)
        {
            return _configurations[name];
        }

        IHttpHandler IRouteHandler.GetHttpHandler(RequestContext requestContext)
        {
            return new RequireJsHttpHandler(requestContext, _configurations);
        }
    }
}
