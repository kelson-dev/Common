using System;
using System.Collections.Generic;

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

        public override string Description => $"Requires {{left}} then {{right}}, and uses output of {{right}}.\nLeft: {arg1.Description}\nRight:{arg2.Description}";

        public override IEnumerable<string> Examples()
        {
            foreach (var eL in arg1.Examples())
                foreach (var eR in arg2.Examples())
                    yield return $"{eL}{CORE_ARG_DELIMETER}{eR}";
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

        public override string Description => $"Requires {{left}} then {{right}}, and uses output of {{left}}.\nLeft: {arg1.Description}\nRight:{arg2.Description}";

        public override IEnumerable<string> Examples()
        {
            foreach (var eL in arg1.Examples())
                foreach (var eR in arg2.Examples())
                    yield return $"{eL}{CORE_ARG_DELIMETER}{eR}";
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

        public override string Description => $"Requires {{left}} then {{right}}.\nLeft: {arg1.Description}\nRight:{arg2.Description}";

        public override IEnumerable<string> Examples()
        {
            foreach (var eL in arg1.Examples())
                foreach (var eR in arg2.Examples())
                    yield return $"{eL}{CORE_ARG_DELIMETER}{eR}";
        }
    }
}
