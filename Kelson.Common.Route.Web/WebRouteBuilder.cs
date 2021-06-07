using Microsoft.AspNetCore.Http;

namespace Kelson.Common.Route.Web
{
    using Args;
    using System;

    public static class WebRouter
    {
        public static RouteBuilder<HttpContext> Start => new(c => c.Request.Path.Value!);

        public static Func<HttpContext, bool> GET = context => context.Request.Method == "GET";
    }
}
