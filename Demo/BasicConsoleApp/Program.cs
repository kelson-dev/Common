using System;
using System.Threading.Tasks;

using static Kelson.Common.Route.RouteBuilder;
using static Kelson.Common.Route.Core;
using Kelson.Common.Route.Args;
using Kelson.Common.Parsing;
using Kelson.Common.Route.Options;
using System.Linq.Expressions;

try
{
    Route(
        On("echo" & REMAINING)
            | EchoCommand,
        On("add" & Tuple(INTEGER, INTEGER))
            | AddCommand,
        On("add" & Options<AdditionOptions>(() => new()), Tuple(INTEGER, INTEGER))
            | AddWithOptionsCommand)
        (args);
}
catch (Exception e)
{
    Console.WriteLine(e);
}


Task EchoCommand(string remaining) => Console.Out.WriteLineAsync(remaining);

Task AddCommand((int a, int b) numbers) => Console.Out.WriteLineAsync($"{numbers.a} + {numbers.b} = {numbers.a + numbers.b}");

async Task AddWithOptionsCommand(AdditionOptions options, (int a, int b) numbers)
{
    int result = options.IsSubtraction
        ? numbers.a - numbers.b
        : numbers.a + numbers.b;
    ConsoleColor previousColor = Console.ForegroundColor;
    if (options.Color is ConsoleColor assigned)
        Console.ForegroundColor = assigned;
    await Console.Out.WriteLineAsync($"{numbers.a} + {numbers.b} = {numbers.a + numbers.b}");
    if (options.Color is not null)
        Console.ForegroundColor = previousColor;
}

public class ConsoleColorArgument : TextArg<ConsoleColor>
{
    public override bool Matches(ref ReadOnlySpan<char> text, out ConsoleColor result)
    {
        result = default;
        SpanReadingExtensions.TrimStart(ref text);
        int delimeter = text.IndexOf(' ');
        if (delimeter <= 0)
            return false;
        string name = text[..delimeter].ToString();
        text = text[delimeter..];
        return Enum.TryParse(name, out result);
    }
}

record AdditionOptions(bool IsSubtraction = false, ConsoleColor? Color = null) : IOptionsModel<AdditionOptions>
{
    public FlagDescriptor<AdditionOptions>[] Setters => new FlagDescriptor<AdditionOptions>[]
    {
        Flag<AdditionOptions, bool>("subtraction", ANY, (o, sub) => o with { IsSubtraction = sub }),
        Flag<AdditionOptions, ConsoleColor>("color", new ConsoleColorArgument(), (o, color) => o with { Color = color }),
    };
}