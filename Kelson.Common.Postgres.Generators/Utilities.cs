using System.Text.RegularExpressions;

namespace Kelson.Common.Postgres.Generators
{
    public static class Utilities
    {
        private static readonly Regex CapitalSplittingRegex = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

        public static string ToTableName(string name) => 
            CapitalSplittingRegex.Replace(name, "_").ToLowerInvariant();

        public static string ToColumnName(string name) => $"\"{name.ToLowerInvariant()}\"";
    }
}
