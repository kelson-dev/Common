using Kelson.Common.Route.Args;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace Kelson.Common.Route.Web.Args
{
    public class PathArg : TextArg<HttpContext>
    {
        private readonly string routeText;

        public PathArg(string text) => routeText = text;

        public override string Description { get; }
        public override string Syntax { get; }

        public override IEnumerable<string> Examples() => throw new NotImplementedException();

        public override bool Matches(HttpContext context, ref ReadOnlySpan<char> text, out Unit result)
        {
            throw new NotImplementedException();
        }
    }
}
