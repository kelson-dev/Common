using System;
using Kelson.Common.Parsing;

namespace Kelson.Common.Route.Args
{
    /// <summary>
    /// Accepts:
    ///     a ulong ID
    /// or
    ///     any ulong ID, returning that ID
    /// </summary>
    public class LongCommandArgument : TextArg<ulong>
    {
        public override bool Matches(ref ReadOnlySpan<char> text, out ulong result) =>
            text.TryConsumeLong(out result, out text);
    }
}
