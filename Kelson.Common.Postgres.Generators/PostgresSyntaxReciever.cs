using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kelson.Common.Postgres.Generators
{
    using static Utilities;

    public class PostgresSyntaxReciever : ISyntaxReceiver
    {
        public Dictionary<string, CompilationUnitSyntax> NewRecordDeclarations { get; } = new Dictionary<string, CompilationUnitSyntax>();

        public Dictionary<string, Func<Dictionary<string, SyntaxReference>, CompilationUnitSyntax>> NewRepositoryBuilders { get; } =
            new Dictionary<string, Func<Dictionary<string, SyntaxReference>, CompilationUnitSyntax>>();

        public static bool TryGetEntityRecord(SyntaxNode syntaxNode, out RecordDeclarationSyntax recordDec, out SimpleBaseTypeSyntax bts)
        {
            if (syntaxNode is RecordDeclarationSyntax dec)
            {
                recordDec = dec;
                
                var entityInterfaceBaseType = recordDec.BaseList?.Types.Select(t => t as SimpleBaseTypeSyntax)
                    .FirstOrDefault(t => t?.Type is GenericNameSyntax nameSyntax && nameSyntax.Identifier.Text == "IPostgresEntity");

                if (entityInterfaceBaseType is SimpleBaseTypeSyntax pkType)
                {
                    bts = pkType;
                    return true;
                }
            }
            recordDec = default;
            bts = default;
            return false;
        }

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (TryGetEntityRecord(syntaxNode, out RecordDeclarationSyntax recordDec, out SimpleBaseTypeSyntax pkType))
            {
                if (!recordDec.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PartialKeyword)))
                    return;
                
                var ancestor = recordDec.FirstAncestorOrSelf<CompilationUnitSyntax>();
                var usings = ancestor.DescendantNodes()
                    .Where(n => n is UsingDirectiveSyntax)
                    .Select(node => (UsingDirectiveSyntax)node)
                    .ToArray();

                var (keyMethod, withIdMethod) = KeySetterAndWithIdMethods(recordDec, pkType);
                var count = recordDec.ParameterList?.Parameters.Count ?? 0;
                string qualifiedNames =
                    string.Join(".", recordDec.Ancestors().Where(n => n is NamespaceDeclarationSyntax).Select(n => (NamespaceDeclarationSyntax)n).Select(nsd => nsd.Name.ToString()));
                NewRecordDeclarations.Add(
                    $"{recordDec.Identifier.ValueText}DefaultCtor",
                    BuildRecord(recordDec, usings, qualifiedNames, 
                        keyMethod, 
                        withIdMethod));
            }
            else if (syntaxNode is ClassDeclarationSyntax classDec)
                if (classDec.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PartialKeyword)))
                    if (classDec.BaseList?.Types
                            .Select(t => t as SimpleBaseTypeSyntax)
                            .FirstOrDefault(t =>
                                t?.Type is GenericNameSyntax nameSyntax
                                && nameSyntax.Identifier.Text == "PostgresqlCrudRepository")
                        is SimpleBaseTypeSyntax repoPkType)
                        NewRepositoryBuilders.Add($"{classDec.Identifier.Text}RepoAutoImpl", MakeContextAwareBuilder(classDec, repoPkType));
        }

        private CompilationUnitSyntax BuildRecord(RecordDeclarationSyntax recordDec, UsingDirectiveSyntax[] usings, string namespaceQualifiedNames, params MemberDeclarationSyntax[] members) =>
            SyntaxFactory.CompilationUnit()
                    .WithUsings(SyntaxFactory.List(usings))
                    .WithMembers(
                        SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                            SyntaxFactory.NamespaceDeclaration(
                                SyntaxFactory.ParseName(namespaceQualifiedNames))
                            .WithMembers(SyntaxFactory.List(
                                new MemberDeclarationSyntax[] {
                                    SyntaxFactory.RecordDeclaration(
                                        SyntaxFactory.Token(SyntaxKind.RecordKeyword),
                                        recordDec.Identifier)
                                    .WithModifiers(
                                        SyntaxFactory.TokenList(
                                            new[]{
                                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                                    SyntaxFactory.Token(SyntaxKind.PartialKeyword)}))
                                    .WithOpenBraceToken(
                                        SyntaxFactory.Token(SyntaxKind.OpenBraceToken))
                                    .WithMembers(SyntaxFactory.List(members.Append(
                                        SyntaxFactory.ConstructorDeclaration(recordDec.Identifier)
                                            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                            .WithInitializer(
                                                SyntaxFactory.ConstructorInitializer(
                                                    SyntaxKind.ThisConstructorInitializer,
                                                    SyntaxFactory.ArgumentList(
                                                        SyntaxFactory.SeparatedList(
                                                            Enumerable.Range(0, recordDec.ParameterList?.Parameters.Count ?? 0).Select(i =>
                                                                SyntaxFactory.Argument(
                                                                    SyntaxFactory.LiteralExpression(
                                                                        SyntaxKind.DefaultLiteralExpression,
                                                                        SyntaxFactory.Token(SyntaxKind.DefaultKeyword))))
                                                            .ToArray()))))
                                            .WithBody(SyntaxFactory.Block()))))
                            .WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken))}))))
                    .NormalizeWhitespace();

        private Func<Dictionary<string, SyntaxReference>, CompilationUnitSyntax> MakeContextAwareBuilder(ClassDeclarationSyntax classDec, SimpleBaseTypeSyntax pkType) =>
            (Dictionary<string, SyntaxReference> context) =>
            {
                var entityName = (pkType.Type as GenericNameSyntax).TypeArgumentList.Arguments.First();
                var idType = (pkType.Type as GenericNameSyntax).TypeArgumentList.Arguments.Last();

                if (context.TryGetValue(entityName.ToString(), out SyntaxReference @ref))
                {
                    var record = (RecordDeclarationSyntax)@ref.GetSyntax();
                    var table = ToTableName(record.Identifier.Text);
                    var parameters = record.ParameterList.Parameters.Select(p => (p.Type, p.Identifier.Text, ToColumnName(p.Identifier.Text))).ToArray();

                    var ancestor = classDec.FirstAncestorOrSelf<CompilationUnitSyntax>();
                    var usings = ancestor.DescendantNodes()
                        .Where(n => n is UsingDirectiveSyntax)
                        .Select(node => (UsingDirectiveSyntax)node)
                        .ToArray();

                    GeneratorTypeInfo[] columns = record?.ParameterList.Parameters.Select(p => new GeneratorTypeInfo(p)).ToArray() ?? Array.Empty<GeneratorTypeInfo>();
                    var idTypes = new IdTypeInfo(columns, idType);
                    string qualifiedNames =
                    string.Join(".", classDec.Ancestors().Where(n => n is NamespaceDeclarationSyntax).Select(n => (NamespaceDeclarationSyntax)n).Select(nsd => nsd.Name.ToString()));

                    return SyntaxFactory.CompilationUnit()
                        .WithUsings(SyntaxFactory.List(usings))
                        .WithMembers(
                         SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                            SyntaxFactory.NamespaceDeclaration(
                                SyntaxFactory.ParseName(qualifiedNames))
                            .WithMembers(
                                SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                                    SyntaxFactory.ClassDeclaration(classDec.Identifier)
                                    .WithModifiers(SyntaxFactory.TokenList(new[] { SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword) }))
                                    .WithMembers(
                                        SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                                            RepositorySqlBuilders.SqlQueriesProperty(record, columns, idTypes)))))))
                        .NormalizeWhitespace();
                }
                else
                {
                    throw new NotImplementedException($"Could not find entity record for {classDec}");
                }
            };

        

        private (MethodDeclarationSyntax getKey, MethodDeclarationSyntax withId) KeySetterAndWithIdMethods(RecordDeclarationSyntax rec, SimpleBaseTypeSyntax bts)
        {
            GeneratorTypeInfo[] columns = rec?.ParameterList.Parameters.Select(p => new GeneratorTypeInfo(p)).ToArray() ?? Array.Empty<GeneratorTypeInfo>();
            var idType = (bts.Type as GenericNameSyntax).TypeArgumentList.Arguments.Last();
            var idTypes = new IdTypeInfo(columns, idType);

            if (idType is TupleTypeSyntax tupleType)
                return (EntityRecordMethodBuilders.BuildKeyGetter(rec, tupleType),
                        EntityRecordMethodBuilders.BuildWithIdMethod(rec, tupleType));
            else if (idType is PredefinedTypeSyntax keywordType)
                return (EntityRecordMethodBuilders.BuildKeyGetter(rec, keywordType),
                            EntityRecordMethodBuilders.BuildWithIdMethod(rec, keywordType));

            else if (idType is IdentifierNameSyntax idName)
                return (EntityRecordMethodBuilders.BuildKeyGetter(rec, idName),
                            EntityRecordMethodBuilders.BuildWithIdMethod(rec, idName));
            else
                throw new NotImplementedException(idType.GetType().ToString());
        }
    }
}
