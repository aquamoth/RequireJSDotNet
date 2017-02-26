using System.Web;
using System.Web.Routing;

namespace RequireJsNet.Examples
{
    public class RequireJsConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            var routeHandler = HttpModule.RequireJsRouteHandler.RegisterRoutes(routes);

            routeHandler.RegisterConfig("complex", complexConfig());
        }

        private static RequireRendererConfiguration complexConfig()
        {
            return new RequireRendererConfiguration
            {
                // the url from where require.js will be loaded
                RequireJsUrl = VirtualPathUtility.ToAbsolute("~/Scripts/require.js"),
                // baseUrl to be passed to require.js, will be used when composing urls for scripts
                BaseUrl = VirtualPathUtility.ToAbsolute("~/Scripts/"),
                // a list of all the configuration files you want to load
                ConfigurationFiles = new[] { "~/RequireJS.complex.json" },
                // root folder for your scripts, will be used for composing paths to entrypoint
                EntryPointRoot = "~/Scripts/",
                // whether we should load overrides or not, used for autoBundles
                LoadOverrides = false,
                // compute the value you want locale to have, used for i18n
                LocaleSelector = httpContext => System.Threading.Thread.CurrentThread.CurrentUICulture.Name.Split('-')[0],
                // instance of IRequireJsLogger
                Logger = null,
                // extensability point for the config object
                ProcessConfig = config => { },
                // extensability point for the options object
                ProcessOptions = options => { },
                // value for urlArgs to be passed to require.js
                UrlArgs = ""
            };
        }
    }
}
