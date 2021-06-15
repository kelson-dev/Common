using System;
using System.Collections.Generic;

namespace Kelson.Common.Route.Args
{
    /// <summary>
    /// Accepts:
    ///     empty text, returning the remaining (empty) text
    /// </summary>
    public class EmptyCommandArgument<TC> : TextArg<TC>
    {
        public override bool Matches(TC context, ref ReadOnlySpan<char> text, out Unit result)
        {
            result = default;
            return text.IsEmpty;
        }

        public override string Description => $"Matches on the end of input";

        public override IEnumerable<string> Examples()
        {
            yield return "";
        }
    }
}
