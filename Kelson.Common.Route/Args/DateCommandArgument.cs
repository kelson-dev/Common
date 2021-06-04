using System;

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
    public class DateCommandArgument : TextArg<DateTimeOffset>
    {
        public override bool Matches(ref ReadOnlySpan<char> text, out DateTimeOffset result)
        {
            bool passed = DateTimeOffset.TryParse(text, out result);
            text = passed ? string.Empty : text;
            return passed;
        }
    }
}
