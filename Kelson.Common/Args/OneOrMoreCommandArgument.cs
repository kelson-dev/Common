using System;
using System.Collections.Generic;
using Kelson.Common.Parsing;

namespace Kelson.Common.Route.Args
{
    /// <summary>
    /// Accepts:
    ///     A comma delimted list of one or more parameters, returning an array of those parameters
    /// </summary>
    public class OneOrMoreCommandArgument<T> : TextArg<T[]>
    {
        private readonly TextArg<T> arg;

        public OneOrMoreCommandArgument(TextArg<T> arg) => this.arg = arg;

        public override bool Matches(ref ReadOnlySpan<char> text, out T[] result)
        {
            if (arg.Matches(ref text, out T item))
            {
                List<T> items = new() { item };
                text = text.TrimStart();
                while (text.StartsWith(",") && arg.Matches(ref text, out item))
                    items.Add(item);
                result = items.ToArray();
                return true;
            }
            result = Array.Empty<T>();
            return false;
        }
    }
}
