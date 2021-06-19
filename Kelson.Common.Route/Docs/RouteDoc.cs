using System.Linq;

namespace Kelson.Common.Route.Docs
{
    public record RouteDoc(
        string? Name,
        string? Description,
        string? Syntax,
        string[] Examples,
        RouteDoc[]? Subcommands)
    {
        public override string ToString()
        {
            if (Subcommands is null || Subcommands.Length == 0)
                return $"Command: {Name}\n{Description}\n{string.Join('\n', Examples)} \n";
            else
                return $"Command: {Name}\n{Description}\n{string.Join('\n', Examples)}\n{string.Join('\n', Subcommands.Select(c => c.ToString()))} \n";
        }

        public string ToMarkdown()
        {
            string exampleString = Examples.Length > 0
                ? $"\n**Examples:**\n * {string.Join("\n * ", Examples)}\n"
                : "";
            string desc = Description is string description
                ? $"*{description}*\n"
                : "";
            string syntaxString = Syntax is string syntax && syntax.Length > 0
                ? $"\n```{syntax}```\n"
                : "";
            if (Subcommands is null || Subcommands.Length == 0)
            {
                if (Name is string name)
                    return $"### {name}\n{desc}{syntaxString}{exampleString}";
                else
                    return "";
            }
            else if (Name is string name)
                return $"## {name}\n{desc}{syntaxString}{exampleString}{string.Join("\n", Subcommands.Select(c => c.ToMarkdown()))}\n";
            else
                return $"\n{string.Join("\n", Subcommands.Select(c => c.ToMarkdown()))}";
        }
    };
}
