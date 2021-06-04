using System;
using Kelson.Common.Parsing;

namespace Kelson.Common.Route.Args
{
    /// <summary>
    /// Accepts:
    ///     text that when trimmed begins with [textToMatch],
    ///     returning the remaining text that follows [textToMatch]
    /// </summary>
    public class TextCommandArgument : TextArg<string>
    {
        private readonly string match;

        public TextCommandArgument(string textToMatch) => match = textToMatch;

        public override bool Matches(ref ReadOnlySpan<char> text, out string result)
        {
            bool passed = text.TryConsumeWord(match, out text);
            result = passed ? text.ToString() : string.Empty;
            return passed;
        }

        public static implicit operator TextCommandArgument(string text) => new(text);
    }
}
