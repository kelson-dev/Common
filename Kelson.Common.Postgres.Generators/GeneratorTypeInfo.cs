using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Kelson.Common.Postgres.Generators
{
    public readonly struct GeneratorTypeInfo
    {
        public readonly TypeStyle Style;
        public readonly string DotnetTypeName;
        public readonly string DotnetPropertyIdentifier;
        public readonly string DotnetTupleIdentifier;
        public readonly string PostgresIdentifier;

        public GeneratorTypeInfo(ParameterSyntax parameter)
        {
            var type = parameter.Type;
            if (type is NullableTypeSyntax nullable)
                type = nullable.ElementType;
            if (type is PredefinedTypeSyntax keywordType)
            {
                DotnetPropertyIdentifier = parameter.Identifier.Text;
                DotnetTupleIdentifier = char.ToLowerInvariant(DotnetPropertyIdentifier[0]) + DotnetPropertyIdentifier.Substring(1);
                PostgresIdentifier = DotnetPropertyIdentifier.ToLowerInvariant();
                Style = TypeStyle.Keyword;
                DotnetTypeName = keywordType.Keyword.Text;
            }
            else if (type is IdentifierNameSyntax nameSyntax)
            {
                DotnetPropertyIdentifier = parameter.Identifier.Text;
                DotnetTupleIdentifier = char.ToLowerInvariant(DotnetPropertyIdentifier[0]) + DotnetPropertyIdentifier.Substring(1);
                PostgresIdentifier = DotnetPropertyIdentifier.ToLowerInvariant();
                Style = TypeStyle.Identifier;
                DotnetTypeName = nameSyntax.Identifier.Text;
            }
            else if (type is ArrayTypeSyntax arraySyntax)
            {
                DotnetPropertyIdentifier = parameter.Identifier.Text;
                DotnetTupleIdentifier = char.ToLowerInvariant(DotnetPropertyIdentifier[0]) + DotnetPropertyIdentifier.Substring(1);
                PostgresIdentifier = DotnetPropertyIdentifier.ToLowerInvariant();
                Style = TypeStyle.Identifier;
                if (arraySyntax.ElementType is PredefinedTypeSyntax arrayKeywordType)
                    DotnetTypeName = $"{arrayKeywordType.Keyword.Text}[]";
                else if (arraySyntax.ElementType is IdentifierNameSyntax arrayNameSyntax)
                    DotnetTypeName = $"{arrayNameSyntax.Identifier.Text}[]";
                else
                    throw new NotImplementedException($"Could not handle array type {arraySyntax.ElementType.GetType()}");
            }
            else
                throw new NotImplementedException($"GeneratorTypeInfo contstructor doesn't handle type syntax {type} {type.GetType()}");
        }
    }
}
