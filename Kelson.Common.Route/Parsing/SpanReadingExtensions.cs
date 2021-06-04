using System;
using System.Collections.Generic;

namespace Kelson.Common.Parsing
{
    public static class SpanReadingExtensions
    {
        /// <summary>
        /// Try to consume one word from the input text.
        /// If the expected word is found then the 'remaining' output parameter
        /// will contain the remaining text following the word.
        /// </summary>
        public static bool TryConsumeWord(this ReadOnlySpan<char> text, string word, out ReadOnlySpan<char> remaining)
        {
            TrimStart(ref text);
            if (text.StartsWith(word))
            {
                remaining = text[word.Length..].TrimStart();
                return true;
            }
            else
            {
                remaining = text;
                return false;
            }
        }

        public static bool TryConsumeLong(this ReadOnlySpan<char> text, out ulong value, out ReadOnlySpan<char> remaining)
        {
            TrimStart(ref text);
            int indexOfEnd = text.IndexOf(' ');
            if (indexOfEnd < 0)
                indexOfEnd = text.Length;
            var textToParse = text[..indexOfEnd];
            if (ulong.TryParse(textToParse, out value))
            {
                remaining = text[indexOfEnd..];
                return true;
            }
            else
            {
                remaining = text;
                return false;
            }
        }

        public static bool TryConsumeInteger(this ReadOnlySpan<char> text, out int value, out ReadOnlySpan<char> remaining)
        {
            TrimStart(ref text);
            int indexOfEnd = text.IndexOf(' ');
            if (indexOfEnd < 0)
                indexOfEnd = text.Length;
            var textToParse = text[..indexOfEnd];
            if (int.TryParse(textToParse, out value))
            {
                remaining = text[indexOfEnd..];
                return true;
            }
            else
            {
                remaining = text;
                return false;
            }
        }

        /// <summary>
        /// Attempt to read quoted text from the start of the input text. 
        /// If quoted text is found, the 'quote' output parameter will contain the quote,
        /// and the 'remaining' output parameter will contain the text following the quote
        /// </summary>
        public static bool TryConsumeQuote(this ReadOnlySpan<char> text, out ReadOnlySpan<char> quote, out ReadOnlySpan<char> remaining)
        {
            TrimStart(ref text);
            if (text.Length > 0 && text[0] == '"')
            {
                var lastQuote = text.LastIndexOf('"');
                remaining = text[(lastQuote + 1)..].TrimStart();
                quote = text[1..lastQuote];
                return true;
            }
            else
            {
                remaining = text;
                quote = "";
                return false;
            }
        }

        public static void TrimStart(ref ReadOnlySpan<char> text) => text = text.TrimStart();

        public static IEnumerable<KeyValuePair<TKey, TValue>> ToKvps<TKey, TValue>(this IEnumerable<(TKey key, TValue value)> pairs)
        {
            foreach (var (key, value) in pairs)
                yield return new KeyValuePair<TKey, TValue>(key, value);
        }

        public static IEnumerable<KeyValuePair<TKey, TValue>> ToKvps<TKey, TValue, TItem>(this IEnumerable<TItem> items, Func<TItem, (TKey key, TValue value)> selector)
        {
            foreach (var item in items)
            {
                var (key, value) = selector(item);
                yield return new KeyValuePair<TKey, TValue>(key, value);
            }
        }

        public static bool StartsWith(this ReadOnlySpan<char> span, char character) => span.Length > 0 && span[0] == character;

        public static bool StartsWith(this ReadOnlySpan<char> span, string text) =>
               span.Length >= text.Length
            && text.AsSpan().Equals(span[..text.Length], StringComparison.InvariantCulture);
    }
}
