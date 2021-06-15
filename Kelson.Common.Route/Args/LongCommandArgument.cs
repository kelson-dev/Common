using System;
using System.Collections.Generic;
using Kelson.Common.Parsing;

namespace Kelson.Common.Route.Args
{
    /// <summary>
    /// Accepts:
    ///     a ulong ID
    /// or
    ///     any ulong ID, returning that ID
    /// </summary>
    public class LongCommandArgument<TC> : TextArg<TC, ulong>
    {
        public override bool Matches(TC context, ref ReadOnlySpan<char> text, out ulong result) =>
            text.TryConsumeLong(out result, out text);

        public override string Description => $"Matches on large positive whole numbers [0, {int.MaxValue}]";

        public override IEnumerable<string> Examples()
        {
            yield return "0";
            yield return $"{ulong.MaxValue}";
        }
    }
}
