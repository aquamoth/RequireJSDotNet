﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;

namespace RequireJsNet.HttpModule
{
    public class RequireJsRouteHandler : IRouteHandler
    {
        public const string DEFAULT_CONFIG_NAME = "default";

        internal IReadOnlyDictionary<string, RequireRendererConfiguration> Configurations { get { return _configurations; } }
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
            var builder = new RequireJsHttpHandlerBuilder(_configurations, requestContext.RouteData);
            return new GenericHttpHandler(builder);
        }

        #region Register Routes

        //public static RequireJsRouteHandler RegisterRoutes(RouteCollection routes, string prefix = DEFAULT_ROUTE_PREFIX)
        //{
        //    var routeHandler = new RequireJsRouteHandler(prefix);
        //    routes.Add(new Route(routeHandler.RoutePrefix + URL, routeHandler));
        //    return routeHandler;
        //}

        internal const string URL_CONFIGNAME_NAME = "configName";
        internal const string URL_ENTRYPOINT_NAME = "entrypoint";
        //const string URL = "/{" + URL_CONFIGNAME_NAME + "}/{*" + URL_ENTRYPOINT_NAME + "}";
        //const string DEFAULT_ROUTE_PREFIX = "requirejsdotnet";

        #endregion Register Routes
    }
}
