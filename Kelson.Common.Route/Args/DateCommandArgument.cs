using System;
using System.Collections.Generic;

namespace Kelson.Common.Route.Args
{
    /// <summary>
    /// Accepts:
    ///     a date and time string, returning that date and time
    /// Examples:
    ///     Thu April 10, 2021
    ///     03/26/2022
    ///     11:36 PM
    ///     03/26/2022 +7
    ///     12/21/2012 12:00 PM -08:00
    /// </summary>
    public class DateCommandArgument<TC> : TextArg<TC, DateTimeOffset>
    {
        public override bool Matches(TC context, ref ReadOnlySpan<char> text, out DateTimeOffset result)
        {
            bool passed = DateTimeOffset.TryParse(text, out result);
            text = passed ? string.Empty : text;
            return passed;
        }

        public override string Description => $"Matches dates and times";
        public override string Syntax => "[datetime]";

        public override IEnumerable<string> Examples()
        {
            yield return "Thu April 10, 2021";
            yield return "03/26/2022";
            yield return "11:36 PM";
            yield return "03/26/2022 +7";
            yield return "12/21/2012 12:00 PM -08:00";
        }
    }
}
