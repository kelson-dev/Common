using System;
using System.Collections.Generic;
using System.Linq;

namespace Kelson.Common.DataStructures.Text
{
    /// <summary>
    /// This internal type represents an element of a Trie tree
    /// Each TrieNode maps a single character to 0 or 1 child nodes
    /// If a node is 'Terminal' that indicates that the sequence of characters traversed to reach that node represents a string that exists in the trie
    /// Terminal nodes may contain children, for example the Trie containing 'dog' and 'doggo' would contain nodes
    /// 'd' [ 'o' [ 'g' (terminal) [ 'g' [ 'o' (terminal) ] ] ] ]
    /// </summary>
    internal class TrieNode
    {
        internal readonly int Index;
        internal readonly char C;
        internal readonly string Source;
        internal readonly int PrefixLength;
        internal string Prefix => Source.Substring(0, PrefixLength);
        internal readonly SortedList<char, TrieNode> Children;
        internal object Payload;
        internal bool IsTerminal;
        internal int TerminalDescendents;

        internal TrieNode(string data, int index, object payload = null)
        {
            Index = index;
            C = data[index];
            Source = data;
            PrefixLength = index + 1;
            Children = new SortedList<char, TrieNode>();
            if (index < data.Length - 1)
            {
                Children.Add(data[index + 1], new TrieNode(data, index + 1, payload));
                TerminalDescendents = 1;
                Payload = null;
                IsTerminal = false;
            }
            else
            {
                TerminalDescendents = 0;
                IsTerminal = true;
                Payload = payload;
            }
        }

        /// <summary>
        /// Appends a string to the Trie, returning true if a new terminal descendent has been added to the parent node
        /// </summary>
        internal bool Append(string data, int index, object payload = null)
        {
            if (index == data.Length - 1)
            {
                if (Payload != null && payload != null)
                    throw new ArgumentException("An item with the same key has already been added");
                Payload = payload;
                return IsTerminal != (IsTerminal = true);
            }
            else
            {
                if (Children.ContainsKey(data[index + 1]))
                {
                    var add = Children[data[index + 1]].Append(data, index + 1, payload);
                    if (add)
                        TerminalDescendents++;
                    return add;
                }
                else
                {
                    Children.Add(data[index + 1], new TrieNode(data, index + 1, payload));
                    TerminalDescendents++;
                    return true;
                }
            }
        }

        internal (bool decrementTerminals, bool remove) Remove(string data, int index)
        {
            if (index == data.Length - 1)
            {
                if (IsTerminal)
                {
                    IsTerminal = false;
                    Payload = null;
                    if (Children.Count == 0)
                        return (true, true);
                    else
                        return (true, false);
                }
                else
                    return (false, false);
            }
            else
            {
                var (decrementTerminals, remove) = Children[data[index + 1]].Remove(data, index + 1);
                if (decrementTerminals)
                {
                    TerminalDescendents--;
                    if (remove)
                        Children.Remove(data[index + 1]);
                    return (true, false);
                }
                else
                    return (false, false);
            }
        }

        private bool TraversePrefix(string prefix, out TrieNode node, int prefixIndex = 0)
        {
            node = this;
            prefixIndex++;
            while (prefixIndex < prefix.Length)
            {
                if (node.Children.ContainsKey(prefix[prefixIndex]))
                    node = node.Children[prefix[prefixIndex]];
                else
                    return false;
                prefixIndex++;
            }
            return true;
        }

        public IEnumerable<TrieNode> Values() => this.DepthFirstTraverse(node => node.Children.Values, node => node.IsTerminal);

        public IEnumerable<TrieNode> Values(string prefix, int prefixIndex = 0) => TraversePrefix(prefix, out TrieNode node, prefixIndex) ? node.Values() : Enumerable.Empty<TrieNode>();

        private static bool isUniquePrefix(TrieNode node) => node.TerminalDescendents == 1 || !node.Children.Any(c => c.Value.TerminalDescendents > 0);
        private static readonly List<TrieNode> noNodes = new List<TrieNode>();

        public IEnumerable<TrieNode> Prefixes() => this.DepthFirstTraverse(node => isUniquePrefix(node) ? noNodes : node.Children.Values, isUniquePrefix);

        public IEnumerable<TrieNode> Prefixes(string prefix, int index = 0) => TraversePrefix(prefix, out TrieNode node, index) ? node.Prefixes() : Enumerable.Empty<TrieNode>();

        public override string ToString()
        {
            string children = Children.Any() ? $"({string.Join(",", Children.Values)})" : "";
            string name = IsTerminal ? $"{TerminalDescendents}[{C}]" : $"{TerminalDescendents} {C}";
            return $" {name} {children}";
        }
    }
}
