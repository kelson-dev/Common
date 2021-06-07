using System;

namespace Kelson.Common.Route
{
    using Args;
    using Kelson.Common.Route.Options;

    public static class CoreArgs<TC>
    {
        public static readonly DateCommandArgument<TC> DATE = new();
        public static readonly LongCommandArgument<TC> LONG = new();
        public static readonly IntegerCommandArgument<TC> INTEGER = new();
        public static readonly QuoteCommandArgument<TC> QUOTE = new();
        public static readonly RemainingTextArgument<TC> REMAINING = new();
        public static readonly EmptyCommandArgument<TC> END = new();
        public static readonly DelegateCommandArgument<TC, bool> ANY = new((TC context, ref ReadOnlySpan<char> text, out bool result) => result = true);

        public static TextCommandArgument<TC> Text(string text) => new(text);

        public static TextArg<TC, T> Param<T>(string text, TextArg<TC, T> arg) => new CompositeRightArgument<TC, Unit, T>(Text(text), arg);

        public static OneOrMoreCommandArgument<TC, T> OneOrMore<T>(TextArg<TC, T> of) => new(of);

        public static DelegateCommandArgument<TC, (T1, T2)> Tuple<T1, T2>(TextArg<TC, T1> item1, TextArg<TC, T2> item2) => new((TC context, ref ReadOnlySpan<char> text, out (T1, T2) result) =>
        {
            result = default;
            ReadOnlySpan<char> start = text;
            if (item1.Matches(context, ref text, out T1 t1Result)
             && item2.Matches(context, ref text, out T2 t2Result))
            {
                result = (t1Result, t2Result);
                return true;
            }
            text = start;
            return false;
        });

        //public static OptionsCommandArgument<TConfig> Options<TConfig>(Func<TConfig> defaultConfig) where TConfig : IOptionsModel<TConfig> => new(defaultConfig);

        //public static FlagDescriptor<TConfig> Flag<TConfig, TArg>(string name, TextArg<TArg> arg, Func<TConfig, TArg, TConfig> set)
        //{
        //    bool MatchAndSet(ref ReadOnlySpan<char> text, ref TConfig result) 
        //    {
        //        if (arg.Matches(ref text, out TArg parameter))
        //        {
        //            result = set(result, parameter);
        //            return true;
        //        }
        //        return false;
        //    }

        //    return new FlagDescriptor<TConfig>(name, MatchAndSet);
        //}
    }
}
