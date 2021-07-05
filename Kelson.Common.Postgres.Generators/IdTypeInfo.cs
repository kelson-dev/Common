using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Kelson.Common.Postgres.Generators
{
    public readonly struct IdTypeInfo
    {
        public readonly GeneratorTypeInfo[] typeInfos;

        public IdTypeInfo(GeneratorTypeInfo[] columns, TypeSyntax idType)
        {
            if (idType is PredefinedTypeSyntax keywordType)
                typeInfos = new GeneratorTypeInfo[] 
                {
                    columns.First(c => c.DotnetTypeName == keywordType.Keyword.Text)
                };
            else if (idType is TupleTypeSyntax tupleType)
            {
                var tupleElements = tupleType.Elements
                    .Select(e => e.Identifier.Text.ToUpperInvariant())
                    .ToImmutableHashSet();
                typeInfos = columns.Where(c =>
                    tupleElements.Contains(c.DotnetTupleIdentifier.ToUpperInvariant()))
                    .ToArray();
            }
            else if (idType is IdentifierNameSyntax nameSyntax)
                typeInfos = new GeneratorTypeInfo[]
                {
                    columns.First(c => c.DotnetTypeName == nameSyntax.Identifier.Text)
                };
            else
                throw new NotImplementedException($"IdTypeInfo constructor doesn't know how to handle id type syntax {idType}");
        }
    }
}
