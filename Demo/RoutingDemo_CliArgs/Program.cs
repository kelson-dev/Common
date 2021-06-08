using System;

using System;
using System.Threading.Tasks;

using Kelson.Common.Route;
using Kelson.Common.Route.Args;
using static Kelson.Common.Route.CoreArgs<string[]>;

string joinedArgs(string[] cliArgs) => string.Join(" ", cliArgs);
RouteBuilder<string[]> router() => new(joinedArgs);

var route = router()
    .Aside(context => context.Length == 10,
        router().On(REMAINING).Do(LengthWas10))
    .On("echo" & REMAINING)
        .Do(Echo)
    .On("add" & INTEGER, INTEGER)
        .Do(Add2Int)
    .If(context => context.Length == 0,
        router()
            .On(REMAINING)
            .Do(Echo))
    .ElseIf(context => context.Length > 0 && context[0].Length > 20,
        router()
            .When(END)
            .Do((string[] context, Unit _) => Task.CompletedTask))
    .Else(router())
    .Build();


Task LengthWas10(string[] context, string remaining) => Console.Out.WriteLineAsync($"CLI args of text {remaining} had 10 elements");

Task Echo(string[] context, string text) => Console.Out.WriteLineAsync(text);

Task Add2Int(string[] context, int a, int b) => Console.Out.WriteLineAsync($"{a} + {b} = {a + b}");