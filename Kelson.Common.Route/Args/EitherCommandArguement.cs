using System;
using System.Collections.Generic;

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

        public override string Description => $"Matches either {{left}} or {{right}}\nLeft: {option1.Description}\nRight:{option2.Description}";
        public override string Syntax => $"[{option1.Syntax}|{option2.Syntax}]";

        public override IEnumerable<string> Examples()
        {
            foreach (var eL in option1.Examples())
                yield return eL;
            foreach (var eR in option2.Examples())
                yield return eR;
        }
    }
}
