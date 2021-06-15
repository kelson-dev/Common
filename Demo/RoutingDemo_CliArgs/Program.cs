using System;
using System.Threading.Tasks;
using Kelson.Common.Route;
using static Kelson.Common.Route.CoreArgs<string[]>;

string joinedArgs(string[] cliArgs) => string.Join(" ", cliArgs);
RouteBuilder<string[]> router() => new(joinedArgs);

var route = router()
    .Named("Example CLI Router")
    .DescribedAs("This route builder constructs the conditions and paramaeter parsing for each routable command in the application.")
    .Aside(context => context.Length == 10, 
        router()
            .On(REMAINING)
            .DescribedAs("If the total args length was 10 prints that info and then continues")
            .Do(LengthWas10))
    .On("echo" & REMAINING)
        .Named("Echo")
        .DescribedAs("Returns the remaining text to the console")
        .Do(Echo)
    .On("add" & INTEGER, INTEGER)
        .Do(Add2Int)
    .If(context => context.Length == 0)
        .Route(router()
            .On(REMAINING)
            .Named("Echo if empty")
            .DescribedAs("If there are no arguments, echos the arguments")
            .Do(Echo))
    .ElseIf(context => context.Length > 0 && context[0].Length > 20)
        .Route(router()
            .When(END)
            .Named("NoOp")
            .DescribedAs("Does nothing if there is a lot of context")
            .Do((string[] context) => Task.CompletedTask))
    .Else(router())
    .End()
    .Build();

var doc = route.BuildDocs();

Console.WriteLine(doc.ToMarkdown());

await route.Handle(args);

Task LengthWas10(string[] context, string remaining) => Console.Out.WriteLineAsync($"CLI args of text {remaining} had 10 elements");

Task Echo(string[] context, string text) => Console.Out.WriteLineAsync(text);

Task Add2Int(string[] context, int a, int b) => Console.Out.WriteLineAsync($"{a} + {b} = {a + b}");