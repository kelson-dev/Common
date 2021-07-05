using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Kelson.Common.Postgres.Generators
{
    public static class RepositorySqlBuilders
    {
        public static PropertyDeclarationSyntax SqlQueriesProperty(RecordDeclarationSyntax rec, GeneratorTypeInfo[] columns, IdTypeInfo id)
        {
            string table = Utilities.ToTableName(rec.Identifier.Text);
            var idParamComparisons = id.typeInfos.Select(i => $"{i.PostgresIdentifier} = @{i.DotnetPropertyIdentifier}");
            string whereId = "where " + string.Join(" and ", idParamComparisons);
            var nonSerialTypes = columns.Where(c => !c.DotnetTypeName.EndsWith("Serial"));
            string insert = string.Join(", ", nonSerialTypes.Select(c => c.PostgresIdentifier));
            string parameters = string.Join(", ", nonSerialTypes.Select(c => $"@{c.DotnetPropertyIdentifier}"));
            var nonIdTypes = nonSerialTypes.Where(c => !id.typeInfos.Any(idc => idc.DotnetPropertyIdentifier == c.DotnetPropertyIdentifier));
            string update = string.Join(", ", nonIdTypes.Select(c => $"{c.PostgresIdentifier} = @{c.DotnetPropertyIdentifier}"));

            return SyntaxFactory.PropertyDeclaration(
                    SyntaxFactory.QualifiedName(
                        SyntaxFactory.QualifiedName(
                            SyntaxFactory.QualifiedName(
                                SyntaxFactory.IdentifierName("Kelson"),
                                SyntaxFactory.IdentifierName("Common")),
                            SyntaxFactory.IdentifierName("Postgres")),
                        SyntaxFactory.IdentifierName("DapperQuerySet")),
                    SyntaxFactory.Identifier("Sql"))
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        new[]{
                            SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                            SyntaxFactory.Token(SyntaxKind.OverrideKeyword)}))
                .WithExpressionBody(
                    SyntaxFactory.ArrowExpressionClause(
                        SyntaxFactory.ImplicitObjectCreationExpression()
                        .WithArgumentList(
                            SyntaxFactory.ArgumentList(
                                SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                    new SyntaxNodeOrToken[] {
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.LiteralExpression(
                                                SyntaxKind.StringLiteralExpression,
                                                SyntaxFactory.Literal(table))),
                                        SyntaxFactory.Token(SyntaxKind.CommaToken),
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.LiteralExpression(
                                                SyntaxKind.StringLiteralExpression,
                                                SyntaxFactory.Literal(whereId))),
                                        SyntaxFactory.Token(SyntaxKind.CommaToken),
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.LiteralExpression(
                                                SyntaxKind.StringLiteralExpression,
                                                SyntaxFactory.Literal(insert))),
                                        SyntaxFactory.Token(SyntaxKind.CommaToken),
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.LiteralExpression(
                                                SyntaxKind.StringLiteralExpression,
                                                SyntaxFactory.Literal(parameters))),
                                        SyntaxFactory.Token(SyntaxKind.CommaToken),
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.LiteralExpression(
                                                SyntaxKind.StringLiteralExpression,
                                                SyntaxFactory.Literal(update)))})))))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }
    }
}
