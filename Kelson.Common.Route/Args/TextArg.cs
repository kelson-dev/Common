using System;
using System.Collections.Generic;

namespace Kelson.Common.Route.Args
{
    public delegate bool TextArgMatchDelegate<TC, T>(TC context, ref ReadOnlySpan<char> text, out T result);

    public interface ITextArg
    {
        string Name => GetType().Name;
        string Description { get; }
        string Syntax { get; }
        IEnumerable<string> Examples();
    }

    public abstract class TextArg<TC> : TextArg<TC, Unit>
    {
        public static TextArg<TC> operator &(TextArg<TC> arg, string text) =>
            arg.Before(new TextCommandArgument<TC>(text));

        public static TextArg<TC> operator &(string text, TextArg<TC> arg) =>
            new CompositeConditionArgument<TC, Unit, Unit>(
                new TextCommandArgument<TC>(text).Then(arg),
                arg);

        public new TextArg<TC> Before<T2>(TextArg<TC, T2> suffix) =>
            new CompositeConditionArgument<TC, Unit, T2>(this, suffix);

        public static TextArg<TC> operator &(TextArg<TC> condition, TextArg<TC> arg) =>
            new CompositeConditionArgument<TC, Unit, Unit>(
                condition,
                arg);
    }

    public abstract class TextArg<TC, T> : ITextArg
    {
        public static string CORE_ARG_DELIMETER = " ";

        public abstract bool Matches(
            TC context,
            ref ReadOnlySpan<char> text,
            out T result);

        public virtual string Name => GetType().Name;
        public abstract string Description { get; }
        public abstract string Syntax { get; }
        public abstract IEnumerable<string> Examples();
        
        public static TextArg<TC, T> operator &(string text, TextArg<TC, T> arg) =>
            new CompositeRightArgument<TC, Unit, T>(
                new TextCommandArgument<TC>(text),
                arg);

        public static TextArg<TC, T> operator &(TextArg<TC, T> arg, string text) =>
            new CompositeLeftArgument<TC, T, Unit>(
                arg,
                new TextCommandArgument<TC>(text));

        public static TextArg<TC, T> operator &(TextArg<TC> condition, TextArg<TC, T> arg) =>
            new CompositeRightArgument<TC, Unit, T>(
                condition,
                arg);

        public TextArg<TC, T> Before<T2>(TextArg<TC, T2> suffix) =>
            new CompositeLeftArgument<TC, T, T2>(this, suffix);

        public TextArg<TC, T2> Then<T2>(TextArg<TC, T2> suffix) =>
            new CompositeRightArgument<TC, T, T2>(this, suffix);

        public static TextArg<TC, T> operator |(TextArg<TC, T> arg1, TextArg<TC, T> arg2) =>
            new EitherCommandArguement<TC, T>(arg1, arg2);
    }
}
