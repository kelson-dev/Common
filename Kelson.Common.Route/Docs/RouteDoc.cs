using System.Linq;

namespace Kelson.Common.Route.Docs
{
    public record RouteDoc(
        string? Name,
        string? Description,
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
                ? $" * {string.Join("\n * ", Examples)}\n"
                : "";
            string desc = Description is string description
                ? $"*{description}*\n"
                : "";
            if (Subcommands is null || Subcommands.Length == 0)
            {
                if (Name is string name)
                    return $"## {name}\n{desc}{exampleString}";
                else
                    return "";
            }
            else if (Name is string name)
                return $"## {name}\n{desc}{exampleString}{string.Join("\n", Subcommands.Select(c => c.ToMarkdown()))}\n";
            else
                return $"\n{string.Join("\n", Subcommands.Select(c => c.ToMarkdown()))}";
        }
    };
}
