using System;

namespace Kelson.Common.Route.Args
{
    public class EitherCommandArguement<TC, T> : TextArg<TC, T>
    {
        private readonly TextArg<TC, T> option1;
        private readonly TextArg<TC, T> option2;

        public EitherCommandArguement(TextArg<TC, T> option1, TextArg<TC, T> option2) => (this.option1, this.option2) = (option1, option2);

        public override bool Matches(TC context, ref ReadOnlySpan<char> text, out T result)
        {
            var content = text;
            var passed =
                option1.Matches(context, ref content, out result)
             || option2.Matches(context, ref content, out result);
            text = passed ? text : content;
            return passed;
        }
    }
}
