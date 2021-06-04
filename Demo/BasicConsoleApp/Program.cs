using System;
using System.Threading.Tasks;

using static Kelson.Common.Route.RouteBuilder;
using static Kelson.Common.Route.Core;
using Kelson.Common.Route.Args;
using Kelson.Common.Parsing;
using Kelson.Common.Route.Options;

try
{
    var c = default(ContextType<string[]>);
    Route<string[]>(
        a => string.Join(" ", a),
        On(c, "echo" & REMAINING)
            | EchoCommand,
        On(c, "add" & Tuple(INTEGER, INTEGER))
            | AddCommand)
        (args);
}
catch (Exception e)
{
    Console.WriteLine(e);
}

Task EchoCommand(string[] context, string remaining) => Console.Out.WriteLineAsync(remaining);

Task AddCommand(string[] context, (int a, int b) numbers) => Console.Out.WriteLineAsync($"{numbers.a} + {numbers.b} = {numbers.a + numbers.b}");