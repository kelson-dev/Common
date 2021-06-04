using System;

namespace Kelson.Common.Route.Args
{
    /// <summary>
    /// Accepts:
    ///     any text, returning that text
    /// </summary>
    public class RemainingTextArgument : TextArg<string>
    {
        public override bool Matches(ref ReadOnlySpan<char> text, out string result)
        {
            result = text.ToString();
            return true;
        }
    }
}
