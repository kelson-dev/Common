using System;

namespace Kelson.Common.Route.Args
{
    using Kelson.Common.Route.Options;
    using System.Linq;

    public class OptionsCommandArgument<TConfig> : TextArg<TConfig> where TConfig : IOptionsModel<TConfig>
    {
        private readonly Func<TConfig> defaultFactory;
        private readonly TextArg<FlagDescriptor<TConfig>> flagTrie;
        //private readonly FlagDescriptor<TConfig>[] setters;

        public OptionsCommandArgument(Func<TConfig> defaultFactory)
        {
            this.defaultFactory = defaultFactory;
            this.flagTrie = new TrieStoreArgument<FlagDescriptor<TConfig>>(
                defaultFactory()
                    .Setters
                    .Select(desc => (desc.Name, desc))
                    .ToArray());
            //for (int i = 0;  i < setters.Length; i++)
        }

        public override bool Matches(ref ReadOnlySpan<char> text, out TConfig config)
        {
            config = defaultFactory();
            while (text.Length > 0 && flagTrie.Matches(ref text, out FlagDescriptor<TConfig> desc))
            {
                desc.MatchAndSet(ref text, ref config);
            }
            return true;
        }
    }

    //public class FlagCommandArgument<T> : Arg<T>
    //{
    //    public readonly string Name;
    //    public readonly string Prefix;
    //}
}
