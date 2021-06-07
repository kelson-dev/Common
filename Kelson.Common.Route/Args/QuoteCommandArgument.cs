﻿using System;
using Kelson.Common.Parsing;

namespace Kelson.Common.Route.Args
{
    /// <summary>
    /// Accepts:
    ///     a string that begins with " and has at least one " following,
    ///     returning all the text between the first and LAST "
    /// </summary>
    public class QuoteCommandArgument<TC> : TextArg<TC, string>
    {
        public override bool Matches(TC context, ref ReadOnlySpan<char> text, out string result)
        {
            bool passed = text.TryConsumeQuote(out ReadOnlySpan<char> quote, out text);
            result = passed ? quote.ToString() : string.Empty;
            return passed;
        }
    }
}
