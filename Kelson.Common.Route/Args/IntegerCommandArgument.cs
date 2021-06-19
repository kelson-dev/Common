using System;
using System.Collections.Generic;
using Kelson.Common.Parsing;

namespace Kelson.Common.Route.Args
{
    /// <summary>
    /// Accepts:
    ///     an integer
    /// or
    ///     any 32 bit signed integer
    /// </summary>
    public class IntegerCommandArgument<TC> : TextArg<TC, int>
    {
        public override bool Matches(TC context, ref ReadOnlySpan<char> text, out int result) =>
            text.TryConsumeInteger(out result, out text);

        public override string Description => $"Matches on whole numbers [{int.MinValue}, {int.MaxValue}]";
        public override string Syntax => $"[signed 32 bit integer]";

        public override IEnumerable<string> Examples()
        {
            yield return "1";
            yield return "493814981";
            yield return "-2";
        }
    }
}
