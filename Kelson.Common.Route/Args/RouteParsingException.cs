using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kelson.Common.Route.Args
{
    public class RouteParsingException : Exception
    {
        public RouteParsingException(string message) : base(message) { }
    }
}
