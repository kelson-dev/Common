using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Kelson.Common.DataStructures.Text
{
    public class Trie : IEnumerable<string>
    {
        public int Count { get; protected set; }

        private protected IEnumerable<TrieNode> prefixes() => nodes.SelectMany(n => n.Value.Prefixes());
        private protected IEnumerable<TrieNode> prefixes(string prefix) => string.IsNullOrEmpty(prefix) ? Enumerable.Empty<TrieNode>() : nodes[prefix[0]].Prefixes(prefix);
        public IEnumerable<string> Prefixes() => prefixes().Select(v => v.Prefix);
        public IEnumerable<string> Prefixes(string prefix) => string.IsNullOrEmpty(prefix) ? Enumerable.Empty<string>() : prefixes(prefix).Select(v => v.Prefix);
        private protected IEnumerable<TrieNode> values() => nodes.SelectMany(n => n.Value.Values());
        private protected IEnumerable<TrieNode> values(string key) => string.IsNullOrEmpty(key) ? Enumerable.Empty<TrieNode>() : nodes[key[0]].Values(key);
        public IEnumerable<string> Values() => values().Select(v => v.Prefix);
        public IEnumerable<string> Values(string key) => this[key];

        public bool IsReadOnly => false;

        public IEnumerable<string> this[string key]
        {
            get => string.IsNullOrEmpty(key) ? Enumerable.Empty<string>() : values(key).Select(v => v.Prefix);
        }

        public void Add(string key)
        {
            if (!validate(key))
                return;
            if (!nodes.ContainsKey(key[0]))
            {
                nodes[key[0]] = new TrieNode(key, 0);
                Count++;
            }
            else
            {
                if (nodes[key[0]].Append(key, 0))
                    Count++;
            }
            return;
        }

        public void Remove(string key)
        {
            if (nodes.ContainsKey(key[0]))
                if (nodes[key[0]].Remove(key, 0).decrementTerminals)
                    Count--;
        }

        public IEnumerator<string> GetEnumerator() => Values().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private protected readonly SortedList<char, TrieNode> nodes = new SortedList<char, TrieNode>();
        protected readonly Predicate<string> validate;

        public Trie(Predicate<string> validation = null)
        {
            validate = validation ?? (key => true);
        }
    }
}
