using System;

namespace Kelson.Common.Route.Args
{
    public class CompositeRightArgument<TC, T1, T2> : TextArg<TC, T2>
    {
        private readonly TextArg<TC, T1> arg1;
        private readonly TextArg<TC, T2> arg2;

        public CompositeRightArgument(TextArg<TC, T1> arg1, TextArg<TC, T2> arg2) => (this.arg1, this.arg2) = (arg1, arg2);

        public override bool Matches(TC context, ref ReadOnlySpan<char> text, out T2 result)
        {
            result = default;
            return arg1.Matches(context, ref text, out T1 _)
                && arg2.Matches(context, ref text, out result);
        }
    }

    public class CompositeLeftArgument<TC, T1, T2> : TextArg<TC, T1>
    {
        private readonly TextArg<TC, T1> arg1;
        private readonly TextArg<TC, T2> arg2;

        public CompositeLeftArgument(TextArg<TC, T1> arg1, TextArg<TC, T2> arg2) => (this.arg1, this.arg2) = (arg1, arg2);

        public override bool Matches(TC context, ref ReadOnlySpan<char> text, out T1 result)
        {
            return arg1.Matches(context, ref text, out result)
                && arg2.Matches(context, ref text, out T2 _);
        }
    }

    public class CompositeConditionArgument<TC, T1, T2> : TextArg<TC>
    {
        private readonly TextArg<TC, T1> arg1;
        private readonly TextArg<TC, T2> arg2;

        public CompositeConditionArgument(TextArg<TC, T1> arg1, TextArg<TC, T2> arg2) => (this.arg1, this.arg2) = (arg1, arg2);

        public override bool Matches(TC context, ref ReadOnlySpan<char> text, out Unit result)
        {
            result = default;
            return arg1.Matches(context, ref text, out T1 _)
                && arg2.Matches(context, ref text, out T2 _);
        }
    }
}
