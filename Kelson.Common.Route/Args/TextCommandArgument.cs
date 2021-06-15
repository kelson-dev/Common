using System;
using System.Collections.Generic;
using Kelson.Common.Parsing;

namespace Kelson.Common.Route.Args
{
    /// <summary>
    /// Accepts:
    ///     text that when trimmed begins with [textToMatch],
    ///     returning the remaining text that follows [textToMatch]
    /// </summary>
    public class TextCommandArgument<TC> : TextArg<TC>
    {
        public readonly string Matched;

        public TextCommandArgument(string textToMatch) => Matched = textToMatch;

        public override bool Matches(TC context, ref ReadOnlySpan<char> text, out Unit result)
        {
            result = default;
            bool passed = text.TryConsumeWord(Matched, out text);
            return passed;
        }

        public static implicit operator TextCommandArgument<TC>(string text) => new(text);

        public override string Description => $"Matches on the text {Matched}";

        public override IEnumerable<string> Examples()
        {
            yield return $"{Matched}";
        }
    }
}
