using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kelson.Common.Postgres.Generators
{
    [Generator]
    public class PostgresGenerator : ISourceGenerator
    {
        private Dictionary<string, SyntaxReference> entityRecords = null;

        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                // One-time creation of entity record dictionary
                entityRecords = entityRecords ?? context.Compilation.SyntaxTrees.SelectMany(s =>
                s.GetRoot()
                 .DescendantNodes()
                 .Where(n => PostgresSyntaxReciever.TryGetEntityRecord(n, out _, out _))
                 .Select(n => (RecordDeclarationSyntax)n))
                 .ToDictionary(node => node.Identifier.Text, node => node.GetReference());
            
                if (context.SyntaxReceiver is PostgresSyntaxReciever handler)
                {
                    foreach (var entry in handler.NewRecordDeclarations)
                    {
                        var name = entry.Key;
                        var unit = entry.Value;
                        string text = unit.GetText().ToString();
                        context.AddSource($"{name}.cs", text);
                    }
                    foreach (var entry in handler.NewRepositoryBuilders)
                    {
                        var name = entry.Key;
                        var unit = entry.Value(entityRecords);
                        string text = unit.GetText().ToString();
                        context.AddSource($"{name}.cs", text);
                    }
                }
            }
            catch (Exception e)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        Guid.NewGuid().ToString(),
                        $"{nameof(PostgresGenerator)} Generator Exception",
                        e.ToString(),
                        "Generator Exception",
                        DiagnosticSeverity.Error,
                        false), null));
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new PostgresSyntaxReciever());
        }
    }
}
