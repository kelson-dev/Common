using System;
using Kelson.Common.Parsing;

namespace Kelson.Common.Route.Args
{
    /// <summary>
    /// Accepts:
    ///     an integer
    /// or
    ///     any 32 bit signed integer
    /// </summary>
    public class IntegerCommandArgument : TextArg<int>
    {
        public override bool Matches(ref ReadOnlySpan<char> text, out int result) =>
            text.TryConsumeInteger(out result, out text);
    }
}
