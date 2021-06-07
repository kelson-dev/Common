using System;

namespace Kelson.Common.Route.Args
{
    /// <summary>
    /// Accepts:
    ///     any text, returning that text
    /// </summary>
    public class RemainingTextArgument<TC> : TextArg<TC, string>
    {
        public override bool Matches(TC context, ref ReadOnlySpan<char> text, out string result)
        {
            result = text.ToString();
            return true;
        }
    }
}
