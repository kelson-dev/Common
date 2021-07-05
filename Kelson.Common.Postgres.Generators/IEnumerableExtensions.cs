using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Kelson.Common.Postgres.Generators
{
    public static class IEnumerableExtensions
    {
        public static SeparatedSyntaxList<T> ToCommaSeperatedList<T>(this IEnumerable<T> args)
            where T : SyntaxNode
        {
            var argsList = args.ToArray();
            return SyntaxFactory.SeparatedList(
                argsList,
                Enumerable.Range(0, argsList.Length - 1).Select(i => SyntaxFactory.Token(SyntaxKind.CommaToken)));

        }

        public static IEnumerable<T> JoinWithDelimeter<T>(this IEnumerable<T> items, T join)
        {
            bool first = true;
            foreach (var item in items)
            {
                if (!first)
                    yield return join;
                first = false;
                yield return item;
            }
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> items, T next)
        {
            foreach (var item in items)
                yield return item;
            yield return next;
        }
    }

    public static class SyntaxExtensions
    {
        public static T WithModifiers<T>(this T type, params SyntaxKind[] keywords) where T : TypeDeclarationSyntax =>
            (T)type.WithModifiers(SyntaxFactory.TokenList(keywords.Select(k => SyntaxFactory.Token(k))));
    }
}
