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
        public override string Syntax => Matched;

        public override IEnumerable<string> Examples()
        {
            yield return $"{Matched}";
        }
    }

    public class PredicateCommandArgument<TC> : TextArg<TC>
    {
        private readonly Func<TC, bool> predicate;

        public PredicateCommandArgument(Func<TC, bool> predicate) => this.predicate = predicate;

        public override string Description => "";
        public override string Syntax => "";

        public override IEnumerable<string> Examples()
        {
            yield break;
        }

        public override bool Matches(TC context, ref ReadOnlySpan<char> text, out Unit result)
        {
            result = default;
            return predicate(context);
        }

        public static implicit operator PredicateCommandArgument<TC>(Func<TC, bool> predicate) => new(predicate);
    }
}
