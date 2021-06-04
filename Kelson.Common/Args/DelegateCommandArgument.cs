using System;

namespace Kelson.Common.Route.Args
{
    public class DelegateCommandArgument<T> : TextArg<T>
    {
        private readonly TextArgMatchDelegate<T> matcher;

        public DelegateCommandArgument(TextArgMatchDelegate<T> matcher) => this.matcher = matcher;

        public override bool Matches(ref ReadOnlySpan<char> text, out T result) => matcher(ref text, out result);
    }
}
