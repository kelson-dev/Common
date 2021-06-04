using Kelson.Common.DataStructures.Text;
using Kelson.Common.Parsing;
using System;
using System.Linq;

namespace Kelson.Common.Route.Args
{
    public class TrieStoreArgument<T> : TextArg<T>
    {
        private readonly string delimeter;
        private readonly Trie<T> _trie = new();

        public TrieStoreArgument(params (string key, T value)[] items)
            : this(" ", items)
        {
            
        }

        public TrieStoreArgument(string delimeter, params (string key, T value)[] items)
        {
            this.delimeter = " ";
            foreach (var (key, value) in items)
            {
                if (key.Contains(delimeter))
                    throw new ArgumentException("No keys of the trie can contain the delimeter (defaults to \" \"), specify delimeter parameter or change keys.");
                _trie.Add(key, value);
            }
        }

        public override bool Matches(ref ReadOnlySpan<char> text, out T result)
        {
            SpanReadingExtensions.TrimStart(ref text);
            int length = text.IndexOf(delimeter);
            length = length > 0 ? length : text.Length;
            var word = text[..length];
            var found = _trie[word.ToString()].FirstOrDefault();
            if (found is T present)
            {
                result = found;
                text = text[length..];
                return true;
            }
            result = default;
            return false;
        }
    }
}
