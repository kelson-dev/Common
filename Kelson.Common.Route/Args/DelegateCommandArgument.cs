using System;

namespace Kelson.Common.Route.Args
{
    public class DelegateCommandArgument<TC, T> : TextArg<TC, T>
    {
        private readonly TextArgMatchDelegate<TC, T> matcher;

        public DelegateCommandArgument(TextArgMatchDelegate<TC, T> matcher) => this.matcher = matcher;

        public override bool Matches(TC context, ref ReadOnlySpan<char> text, out T result) => matcher(context, ref text, out result);
    }
}
