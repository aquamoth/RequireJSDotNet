using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using RequireJsNet;
using System.Net;

namespace RequireJsNet
{
    public class RequireJsHttpHandler : IHttpHandler
    {
        readonly RequestContext requestContext;
        readonly IReadOnlyDictionary<string, RequireRendererConfiguration> configurations;

        internal RequireJsHttpHandler(RequestContext requestContext, IReadOnlyDictionary<string, RequireRendererConfiguration> configurations)
        {
            this.requestContext = requestContext;
            this.configurations = configurations;
        }

        bool IHttpHandler.IsReusable { get { return true; } }

        void IHttpHandler.ProcessRequest(HttpContext context)
        {
            //if (!assertIsValidRequestType(context))
            //    return;

            var configName = requestContext.RouteData.GetRequiredString("configName");
            var config = configurations[configName];

            //if (!assertRequiresNewScript(context, config))
            //    return;

            respondWithScript(context, config);
        }

        //private bool assertIsValidRequestType(HttpContext context)
        //{
        //    if (new[] { "HEAD", "GET" }.Contains(context.Request.RequestType))
        //        return true;

        //    endResponseWith(context, HttpStatusCode.MethodNotAllowed);
        //    return false;
        //}

        //private bool assertRequiresNewScript(HttpContext context, RequireRendererConfiguration config)
        //{
        //    var ifNoneMatch = context.Request.Headers["If-None-Match"];
        //    if (ifNoneMatch != null)
        //    {
        //        if (ifNoneMatch != config.Hashcode)
        //            return true;
        //    }
        //    else
        //    {
        //        DateTime ifModifiedSince;
        //        if (!DateTime.TryParse(context.Request.Headers["If-Modified-Since"], out ifModifiedSince))
        //            return true;

        //        if (config.LastModified > ifModifiedSince)
        //            return true;

        //        addLastModifiedHeaderTo(context, config.LastModified);
        //    }

        //    endResponseWith(context, HttpStatusCode.NotModified);
        //    return false;
        //}

        private void respondWithScript(HttpContext context, RequireRendererConfiguration config)
        {
            var entrypoint = requestContext.RouteData.GetRequiredString("entrypoint");
            var entrypointPath = System.Web.Mvc.MvcHtmlString.Create(entrypoint);

            var httpContext = new HttpContextWrapper(context);
            var scriptContent = RequireJsHtmlHelpers.buildConfigScript(httpContext, config, entrypointPath).Render();

            context.Response.ContentType = "text/javascript";
        //    context.Response.ContentEncoding = Encoding.UTF8;
        //    addLastModifiedHeaderTo(context, config.LastModified);
        //    context.Response.AddHeader("ETag", config.Hashcode);

        //    if (context.Request.RequestType != "HEAD")
                context.Response.Write(scriptContent);

            endResponseWith(context, HttpStatusCode.OK);
        }

        //private static void addLastModifiedHeaderTo(HttpContext context, DateTime lastModified)
        //{
        //    context.Response.AddHeader("Last-Modified", lastModified.ToUniversalTime().ToString("R"));
        //}

        private static void endResponseWith(HttpContext context, HttpStatusCode statusCode)
        {
            //context.Response.StatusCode = (int)statusCode;
            //context.Response.Flush();
            //context.ApplicationInstance.CompleteRequest();
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
