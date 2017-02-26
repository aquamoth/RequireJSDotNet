using System.Collections.Generic;
using System.Text;
using System.Web;

namespace RequireJsNet.HttpModule
{
    public interface IHttpHandlerBuilder
    {
        string Content { get; }
        Encoding ContentEncoding { get; }
        string ContentType { get; }
        IReadOnlyDictionary<string, string> Headers { get; }
        bool IsReusable { get; }
        int StatusCode { get; }

        bool ProcessRequest(HttpContext context);
    }
}