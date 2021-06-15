using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly string delimeter;

        public OneOrMoreCommandArgument(TextArg<TC, T> arg, string delimeter = ",") => 
            (this.arg, this.delimeter) = (arg, delimeter);

        public override bool Matches(TC context, ref ReadOnlySpan<char> text, out T[] result)
        {
            if (arg.Matches(context, ref text, out T item))
            {
                List<T> items = new() { item };
                text = text.TrimStart();
                while (text.StartsWith(delimeter) && arg.Matches(context, ref text, out item))
                    items.Add(item);
                result = items.ToArray();
                return true;
            }
            result = Array.Empty<T>();
            return false;
        }

        public override string Description => $"Matches on one or more of {{arg}}.\nArg:{arg.Description}";

        public override IEnumerable<string> Examples()
        {
            var inner = arg.Examples().ToArray();
            yield return inner[0];
            if (inner.Length > 1)
                yield return string.Join(delimeter, inner);
            else
                yield return $"{inner[0]}{delimeter}{inner[0]}";
        }
    }
}
