using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Kelson.Common.Route.Web
{
    public static class HttpContextRouterAppliactionBuilderExtensions
    {
        public static void RunRequestRouter(this IApplicationBuilder app, RouteBuilder<HttpContext> router)
        {
            var route = router.Build();
            app.Run(context => route.Handle(context));
        }

        public static IApplicationBuilder UseRequestRouter(this IApplicationBuilder app, RouteBuilder<HttpContext> router)
        {
            var route = router.Build();
            return app.Use(async (context, next) =>
            {
                await route.Handle(context);
                await next();
            });
        }
    }
}
