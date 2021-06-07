using System;
using System.Collections.Generic;
using Kelson.Common.Parsing;

namespace Kelson.Common.Route.Args
{
    /// <summary>
    /// Accepts:
    ///     A comma delimted list of one or more parameters, returning an array of those parameters
    /// </summary>
    public class OneOrMoreCommandArgument<TC, T> : TextArg<TC, T[]>
    {
        private readonly TextArg<TC, T> arg;

        public OneOrMoreCommandArgument(TextArg<TC, T> arg) => this.arg = arg;

        public override bool Matches(TC context, ref ReadOnlySpan<char> text, out T[] result)
        {
            if (arg.Matches(context, ref text, out T item))
            {
                List<T> items = new() { item };
                text = text.TrimStart();
                while (text.StartsWith(",") && arg.Matches(context, ref text, out item))
                    items.Add(item);
                result = items.ToArray();
                return true;
            }
            result = Array.Empty<T>();
            return false;
        }
    }
}
