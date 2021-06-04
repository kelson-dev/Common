using System;

namespace Kelson.Common.Route
{
    using Args;
    using Kelson.Common.Route.Options;

    public static class Core
    {
        public static readonly DateCommandArgument DATE = new();
        public static readonly LongCommandArgument LONG = new();
        public static readonly IntegerCommandArgument INTEGER = new();
        public static readonly QuoteCommandArgument QUOTE = new();
        public static readonly RemainingTextArgument REMAINING = new();
        public static readonly EmptyCommandArgument END = new();
        public static readonly DelegateCommandArgument<bool> ANY = new((ref ReadOnlySpan<char> text, out bool result) => result = true);

        public static TextCommandArgument Text(string text) => new(text);

        public static CompositeCommandArgument<string, T> Param<T>(string text, TextArg<T> arg) => new(Text(text), arg);

        public static OneOrMoreCommandArgument<T> OneOrMore<T>(TextArg<T> of) => new(of);

        public static DelegateCommandArgument<(T1, T2)> Tuple<T1, T2>(TextArg<T1> item1, TextArg<T2> item2) => new((ref ReadOnlySpan<char> text, out (T1, T2) result) =>
        {
            result = default;
            ReadOnlySpan<char> start = text;
            if (item1.Matches(ref text, out T1 t1Result)
             && item2.Matches(ref text, out T2 t2Result))
            {
                result = (t1Result, t2Result);
                return true;
            }
            text = start;
            return false;
        });

        public static OptionsCommandArgument<TConfig> Options<TConfig>(Func<TConfig> defaultConfig) where TConfig : IOptionsModel<TConfig> => new(defaultConfig);

        public static FlagDescriptor<TConfig> Flag<TConfig, TArg>(string name, TextArg<TArg> arg, Func<TConfig, TArg, TConfig> set)
        {
            bool MatchAndSet(ref ReadOnlySpan<char> text, ref TConfig result) 
            {
                if (arg.Matches(ref text, out TArg parameter))
                {
                    result = set(result, parameter);
                    return true;
                }
                return false;
            }

            return new FlagDescriptor<TConfig>(name, MatchAndSet);
        }
    }
}
