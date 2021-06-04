using System;

namespace Kelson.Common.Route.Args
{
    public class CompositeCommandArgument<T1, T2> : TextArg<T2>
    {
        private readonly TextArg<T1> arg1;
        private readonly TextArg<T2> arg2;

        public CompositeCommandArgument(TextArg<T1> arg1, TextArg<T2> arg2) => (this.arg1, this.arg2) = (arg1, arg2);

        public override bool Matches(ref ReadOnlySpan<char> text, out T2 result)
        {
            result = default;
            return arg1.Matches(ref text, out T1 _)
                && arg2.Matches(ref text, out result);
        }
    }
}
