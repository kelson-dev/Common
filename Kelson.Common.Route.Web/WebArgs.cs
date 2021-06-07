using Kelson.Common.Route.Args;
using Kelson.Common.Route.Web.Args;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using System.Text.Json;

namespace Kelson.Common.Route.Web
{
    public delegate bool WebArgMatchDelegate<T>(HttpContext context, out T result);

    public static class WebArgs
    {
        public static HttpMethodArg Method(HttpMethod method) => new(method);

        public static JsonArg<T> Json<T>(JsonSerializerOptions options = default) => new(options);

        public static PathArg Path(string text) => new(text);
    }
}
