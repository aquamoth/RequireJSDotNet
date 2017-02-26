using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RequireJsNet.HttpModule
{
    public class GenericHttpHandler : IHttpHandler
    {
        readonly IHttpHandlerBuilder builder;

        internal GenericHttpHandler(IHttpHandlerBuilder builder)
        {
            this.builder = builder;
        }

        bool IHttpHandler.IsReusable { get { return builder.IsReusable; } }

        void IHttpHandler.ProcessRequest(HttpContext context)
        {
            if (!builder.ProcessRequest(context))
                return;

            context.Response.ContentType = builder.ContentType;
            context.Response.ContentEncoding = builder.ContentEncoding;

            foreach (var header in builder.Headers)
                context.Response.AddHeader(header.Key, header.Value);

            if (context.Request.RequestType != "HEAD")
                context.Response.Write(builder.Content);

            context.Response.StatusCode = builder.StatusCode;
            context.Response.Flush();
            context.ApplicationInstance.CompleteRequest();
        }
    }
}
