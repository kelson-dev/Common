using System;

namespace Kelson.Common.Route.Args
{
    /// <summary>
    /// Accepts:
    ///     empty text, returning the remaining (empty) text
    /// </summary>
    public class EmptyCommandArgument : TextArg<Unit>
    {
        public override bool Matches(ref ReadOnlySpan<char> text, out Unit result)
        {
            result = default;
            return text.IsEmpty;
        }
    }
}
