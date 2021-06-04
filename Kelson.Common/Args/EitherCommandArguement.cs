using System;

namespace Kelson.Common.Route.Args
{
    public class EitherCommandArguement<T> : TextArg<T>
    {
        private readonly TextArg<T> option1;
        private readonly TextArg<T> option2;

        public EitherCommandArguement(TextArg<T> option1, TextArg<T> option2) => (this.option1, this.option2) = (option1, option2);

        public override bool Matches(ref ReadOnlySpan<char> text, out T result)
        {
            var content = text;
            var passed =
                option1.Matches(ref content, out result)
             || option2.Matches(ref content, out result);
            text = passed ? text : content;
            return passed;
        }
    }
}
