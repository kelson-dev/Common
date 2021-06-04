using System;

namespace Kelson.Common.Route.Options
{
    public class FlagDescriptor<TConfig>
    {
        public readonly string Name;
        public readonly ConfigMatchSetDelegate<TConfig> MatchAndSet;

        public FlagDescriptor(string name, ConfigMatchSetDelegate<TConfig> set) => (Name, MatchAndSet) = ($"--{name}", set);
    }
    
    public delegate bool ConfigMatchSetDelegate<TConfig>(ref ReadOnlySpan<char> text, ref TConfig config);
}
