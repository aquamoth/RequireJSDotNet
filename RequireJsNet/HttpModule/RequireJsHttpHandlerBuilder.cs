using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using RequireJsNet;
using System.Net;

namespace RequireJsNet.HttpModule
{
    public class RequireJsHttpHandlerBuilder : IHttpHandlerBuilder
    {
        readonly internal IReadOnlyDictionary<string, RequireRendererConfiguration> configurations;
        readonly RouteData routeData;

        public bool IsReusable { get { return true; } }
        public string Content { get; private set; }
        public string ContentType { get; private set; }
        public Encoding ContentEncoding { get; private set; }
        public int StatusCode { get; private set; }
        public IReadOnlyDictionary<string, string> Headers { get { return _headers; } }
        private Dictionary<string, string> _headers = new Dictionary<string, string>();

        internal RequireJsHttpHandlerBuilder(IReadOnlyDictionary<string, RequireRendererConfiguration> configurations, RouteData routeData)
        {
            this.configurations = configurations;
            this.routeData = routeData;
        }

        public bool ProcessRequest(HttpContext context)
        {
            if (IsMethodNotAllowed(context))
                return true;

            var configName = this.routeData.GetRequiredString("configName");
            var config = this.configurations[configName];

            if (IsNotModified(context, config))
                return true;

            var entrypoint = this.routeData.GetRequiredString("entrypoint");
            var entrypointPath = System.Web.Mvc.MvcHtmlString.Create(entrypoint);
            var httpContext = new HttpContextWrapper(context);
            
            this.Content = RequireJsHtmlHelpers.buildConfigScript(httpContext, config, entrypointPath).Render();
            this.ContentType = "text/javascript";
            this.ContentEncoding = Encoding.UTF8;
            this.StatusCode = (int)HttpStatusCode.OK;
            this._headers.Add("Last-Modified", GmtString(config.LastModified));
            this._headers.Add("ETag", config.Hashcode);

            return true;
        }

        internal static string GmtString(DateTime dateTime)
        {
            return dateTime.ToUniversalTime().ToString("R");
        }

        private bool IsMethodNotAllowed(HttpContext context)
        {
            if (new[] { "HEAD", "GET" }.Contains(context.Request.RequestType))
                return false;

            this.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            return true;
        }

        private bool IsNotModified(HttpContext context, RequireRendererConfiguration config)
        {
            var ifNoneMatch = context.Request.Headers["If-None-Match"];
            if (ifNoneMatch != null)
            {
                if (ifNoneMatch != config.Hashcode)
                    return false;
            }
            else
            {
                DateTime ifModifiedSince;
                if (!DateTime.TryParse(context.Request.Headers["If-Modified-Since"], out ifModifiedSince))
                    return false;

                if (config.LastModified > ifModifiedSince)
                    return false;

                //this._headers.Add("Last-Modified", GmtString(config.LastModified));
            }

            this.StatusCode = (int)HttpStatusCode.NotModified;
            return true;
        }

        #region Register Routes

        //public static RequireJsRouteHandler RegisterRoutes(RouteCollection routes, string prefix = DEFAULT_ROUTE_PREFIX)
        //{
        //    var routeHandler = new RequireJsRouteHandler(prefix);

        //    var route = routeHandler.RoutePrefix + "/{configName}/{*entrypoint}";
        //    routes.Add(new Route(route, routeHandler));

        //    return routeHandler;
        //}

        //const string DEFAULT_ROUTE_PREFIX = "requirejsdotnet";

        #endregion Register Routes
    }
}
