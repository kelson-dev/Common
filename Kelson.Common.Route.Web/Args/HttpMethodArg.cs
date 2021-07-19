using Kelson.Common.Route.Args;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using System;
using System.Collections.Generic;

namespace Kelson.Common.Route.Web.Args
{
    public class HttpMethodArg : TextArg<HttpContext>
    {
        private readonly HttpMethod condition;

        public HttpMethodArg(HttpMethod condition) => this.condition = condition;

        public override string Description { get; }
        public override string Syntax { get; }

        public override IEnumerable<string> Examples() => throw new NotImplementedException();

        public override bool Matches(HttpContext context, ref ReadOnlySpan<char> text, out Unit result)
        {
            result = default;
            return Enum.TryParse(context.Request.Method, ignoreCase: true, out HttpMethod parsed)
                && parsed == condition;
        }
    }
}
