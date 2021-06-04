using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kelson.Common.Route.Web
{
    using Args;

    public class WebRouteBuilder
    {
    }

    public interface IWebRouteCommand
    {
        bool Query(HttpContext context);
        Task Invoke(HttpContext context);
    }

    public delegate Func<T, Task> WebFunc<T>(HttpContext context);

    public class WebCommand<T1> : IWebRouteCommand
    {
        public WebArg<T1> Condition1 { get; init; }
        public WebFunc<T1> Action { get; init; }

        public bool Query(HttpContext context) =>
            Condition1.Matches(context, out Condition1.Value);

        public Task Invoke(HttpContext context) =>
            Action(context)(Condition1.Value);
    }
}
