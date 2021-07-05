using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Kelson.Common.Postgres.Generators
{

    public static class EntityRecordMethodBuilders
    {
        public static MethodDeclarationSyntax BuildKeyGetter(RecordDeclarationSyntax rec, TupleTypeSyntax tupleType)
        {
            (SyntaxToken name, TypeSyntax type)[] tupleIdentifiers = tupleType.Elements.Select(e => (e.Identifier, e.Type)).ToArray();
            TupleElementSyntax TupleElement((SyntaxToken name, TypeSyntax type) ti) => SyntaxFactory.TupleElement(ti.type).WithIdentifier(ti.name);
            SyntaxToken RecordPropIdentifier((SyntaxToken name, TypeSyntax type) ti) =>
                rec.ParameterList.Parameters.Single(p => p.Identifier.Text.ToUpperInvariant() == ti.name.Text.ToUpperInvariant()).Identifier;

            return SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.TupleType(
                        tupleIdentifiers.Select(TupleElement)
                        .ToCommaSeperatedList()),
                    SyntaxFactory.Identifier("Key"))
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithExpressionBody(
                    SyntaxFactory.ArrowExpressionClause(
                        SyntaxFactory.TupleExpression(
                            tupleIdentifiers.Select(ti =>
                                SyntaxFactory.Argument(
                                    SyntaxFactory.IdentifierName(RecordPropIdentifier(ti))))
                                .ToCommaSeperatedList())))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        public static MethodDeclarationSyntax BuildWithIdMethod(RecordDeclarationSyntax rec, TupleTypeSyntax tupleType)
        {
            (SyntaxToken name, TypeSyntax type)[] tupleIdentifiers = tupleType.Elements.Select(e => (e.Identifier, e.Type)).ToArray();
            TupleElementSyntax TupleElement((SyntaxToken name, TypeSyntax type) ti) => SyntaxFactory.TupleElement(ti.type).WithIdentifier(ti.name);
            SyntaxToken RecordPropIdentifier((SyntaxToken name, TypeSyntax type) ti) =>
                rec.ParameterList.Parameters.Single(p => p.Identifier.Text.ToUpperInvariant() == ti.name.Text.ToUpperInvariant()).Identifier;

            ExpressionSyntax Assignment((SyntaxToken name, TypeSyntax type) ti) =>
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(RecordPropIdentifier(ti)),
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName("id"),
                        SyntaxFactory.IdentifierName(ti.name)));

            return SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.IdentifierName(rec.Identifier),
                    SyntaxFactory.Identifier("WithId"))
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(
                    SyntaxFactory.ParameterList(
                        SyntaxFactory.SingletonSeparatedList<ParameterSyntax>(
                            SyntaxFactory.Parameter(
                                SyntaxFactory.Identifier("id"))
                            .WithType(
                                SyntaxFactory.TupleType(tupleIdentifiers.Select(TupleElement).ToCommaSeperatedList())))))
                .WithExpressionBody(
                    SyntaxFactory.ArrowExpressionClause(
                        SyntaxFactory.WithExpression(
                            SyntaxFactory.ThisExpression(),
                            SyntaxFactory.InitializerExpression(
                                SyntaxKind.WithInitializerExpression,
                                tupleIdentifiers.Select(Assignment).ToCommaSeperatedList()))))
                .WithSemicolonToken(
                    SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        public static MethodDeclarationSyntax BuildKeyGetter(RecordDeclarationSyntax rec, PredefinedTypeSyntax keywordType)
        {
            var propName = rec.ParameterList.Parameters.First(p => p.Type.ToString() == keywordType.Keyword.Text).Identifier.Text;
            var keyMethod = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(keywordType.Keyword.Kind())),
                    SyntaxFactory.Identifier("Key"));
            keyMethod = keyMethod.WithModifiers(
                SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
            keyMethod = keyMethod.WithExpressionBody(
                SyntaxFactory.ArrowExpressionClause(SyntaxFactory.IdentifierName(propName)));
            keyMethod = keyMethod.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
            return keyMethod;
        }

        public static MethodDeclarationSyntax BuildWithIdMethod(RecordDeclarationSyntax rec, PredefinedTypeSyntax keywordType)
        {
            var propName = rec.ParameterList.Parameters.First(p => p.Type.ToString() == keywordType.Keyword.Text).Identifier.Text;
            var withIdMethod = SyntaxFactory.MethodDeclaration(
                        SyntaxFactory.IdentifierName(rec.Identifier.Text),
                        SyntaxFactory.Identifier("WithId"));
            withIdMethod = withIdMethod.WithModifiers(
                SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
            withIdMethod = withIdMethod.WithParameterList(
                SyntaxFactory.ParameterList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Parameter(
                            SyntaxFactory.Identifier("id"))
                        .WithType(
                            SyntaxFactory.PredefinedType(
                                SyntaxFactory.Token(keywordType.Keyword.Kind()))))));

            withIdMethod = withIdMethod.WithExpressionBody(
                SyntaxFactory.ArrowExpressionClause(
                    SyntaxFactory.WithExpression(
                        SyntaxFactory.ThisExpression(),
                        SyntaxFactory.InitializerExpression(
                            SyntaxKind.WithInitializerExpression,
                            SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                SyntaxFactory.AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    SyntaxFactory.IdentifierName(propName),
                                    SyntaxFactory.IdentifierName("id")))))));

            withIdMethod = withIdMethod.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
            return withIdMethod;
        }

        public static MethodDeclarationSyntax BuildKeyGetter(RecordDeclarationSyntax rec, IdentifierNameSyntax idName)
        {
            var propName = rec.ParameterList.Parameters.First(p => p.Type.ToString() == idName.Identifier.Text).Identifier.Text;
            var keyMethod = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.IdentifierName(idName.Identifier),
                    SyntaxFactory.Identifier("Key"));
            keyMethod = keyMethod.WithModifiers(
                SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
            keyMethod = keyMethod.WithExpressionBody(
                SyntaxFactory.ArrowExpressionClause(SyntaxFactory.IdentifierName(propName)));
            keyMethod = keyMethod.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
            return keyMethod;
        }

        public static MethodDeclarationSyntax BuildWithIdMethod(RecordDeclarationSyntax rec, IdentifierNameSyntax idName)
        {
            var propName = rec.ParameterList.Parameters.First(p => p.Type.ToString() == idName.Identifier.Text).Identifier.Text;
            var withIdMethod = SyntaxFactory.MethodDeclaration(
                        SyntaxFactory.IdentifierName(rec.Identifier.Text),
                        SyntaxFactory.Identifier("WithId"));
            withIdMethod = withIdMethod.WithModifiers(
                SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
            withIdMethod = withIdMethod.WithParameterList(
                SyntaxFactory.ParameterList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Parameter(
                            SyntaxFactory.Identifier("id"))
                        .WithType(
                            SyntaxFactory.IdentifierName(idName.Identifier)))));

            withIdMethod = withIdMethod.WithExpressionBody(
                SyntaxFactory.ArrowExpressionClause(
                    SyntaxFactory.WithExpression(
                        SyntaxFactory.ThisExpression(),
                        SyntaxFactory.InitializerExpression(
                            SyntaxKind.WithInitializerExpression,
                            SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                SyntaxFactory.AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    SyntaxFactory.IdentifierName(propName),
                                    SyntaxFactory.IdentifierName("id")))))));
            withIdMethod = withIdMethod.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
            return withIdMethod;
        }
    }
}
