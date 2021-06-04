using System;

namespace Kelson.Common.Route.Args
{
    public delegate bool TextArgMatchDelegate<T>(ref ReadOnlySpan<char> text, out T result);

    public abstract class TextArg<T>
    {
        public abstract bool Matches(
            ref ReadOnlySpan<char> text,
            out T result);

        public static TextArg<T> operator &(string text, TextArg<T> arg) =>
            new CompositeCommandArgument<string, T>(
                new TextCommandArgument(text),
                arg);

        public static TextArg<T> operator &(TextArg<Unit> condition, TextArg<T> arg) =>
            new CompositeCommandArgument<Unit, T>(
                condition,
                arg);

        public static TextArg<T> operator |(TextArg<T> arg1, TextArg<T> arg2) =>
            new EitherCommandArguement<T>(arg1, arg2);

        //public static implicit operator TextArg<T>(TextArgMatchDelegate<T> matcher) => new DelegateCommandArgument<T>(matcher);
    }

    public abstract class ContextArg<T> : TextArg<T>
    {
        public abstract 

        public abstract bool Matches(
            ref ReadOnlySpan<char> text,
            out T result);
    }
}
