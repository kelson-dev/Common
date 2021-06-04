using System;
using System.Threading.Tasks;

namespace Kelson.Common.Route
{
    using Args;
    using static RouteBuilder;

    public interface IRouteCommand
    {
        // returns a function that will return false if the route does not match the parameter,
        // or will execute the route and then return true if the route does match the parameter
        Func<ValueTask<bool>> Query(ReadOnlySpan<char> text);
    }

    public class Command<T1> : IRouteCommand
    {
        public TextArg<T1> Condition1 { get; init; }

        public Func<T1, Task> Action { get; init; }

        public Command(Func<T1, Task> command) => Action = command;

        public Func<ValueTask<bool>> Query(ReadOnlySpan<char> text) =>
              Condition1.Matches(ref text, out T1 result1)
            ? Yes(() => Action(result1)) : No;
    }

    public class Command<T1, T2> : IRouteCommand
    {
        public TextArg<T1> Condition1 { get; init; }
        public TextArg<T2> Condition2 { get; init; }

        public Func<T1, T2, Task> Action { get; init; }

        public Command(Func<T1, T2, Task> command) => Action = command;

        public Func<ValueTask<bool>> Query(ReadOnlySpan<char> text) =>
               Condition1.Matches(ref text, out T1 result1)
            && Condition2.Matches(ref text, out T2 result2)
            ? Yes(() => Action(result1, result2)) : No;
    }

    public class Command<T1, T2, T3> : IRouteCommand
    {
        public TextArg<T1> Condition1 { get; init; }
        public TextArg<T2> Condition2 { get; init; }
        public TextArg<T3> Condition3 { get; init; }

        public Func<T1, T2, T3, Task> Action { get; init; }

        public Command(Func<T1, T2, T3, Task> command) => Action = command;

        public Func<ValueTask<bool>> Query(ReadOnlySpan<char> text) =>
               Condition1.Matches(ref text, out T1 result1)
            && Condition2.Matches(ref text, out T2 result2)
            && Condition3.Matches(ref text, out T3 result3)
            ? Yes(() => Action(result1, result2, result3)) : No;
    }


    public class Command<T1, T2, T3, T4> : IRouteCommand
    {
        public TextArg<T1> Condition1 { get; init; }
        public TextArg<T2> Condition2 { get; init; }
        public TextArg<T3> Condition3 { get; init; }
        public TextArg<T4> Condition4 { get; init; }

        public Func<T1, T2, T3, T4, Task> Action { get; init; }

        public Command(Func<T1, T2, T3, T4, Task> command) => Action = command;

        public Func<ValueTask<bool>> Query(ReadOnlySpan<char> text) =>
               Condition1.Matches(ref text, out T1 result1)
            && Condition2.Matches(ref text, out T2 result2)
            && Condition3.Matches(ref text, out T3 result3)
            && Condition4.Matches(ref text, out T4 result4)
            ? Yes(() => Action(result1, result2, result3, result4)) : No;
    }


    public class Command<T1, T2, T3, T4, T5> : IRouteCommand
    {
        public TextArg<T1> Condition1 { get; init; }
        public TextArg<T2> Condition2 { get; init; }
        public TextArg<T3> Condition3 { get; init; }
        public TextArg<T4> Condition4 { get; init; }
        public TextArg<T5> Condition5 { get; init; }

        public Func<T1, T2, T3, T4, T5, Task> Action { get; init; }

        public Command(Func<T1, T2, T3, T4, T5, Task> command) => Action = command;

        public Func<ValueTask<bool>> Query(ReadOnlySpan<char> text) =>
               Condition1.Matches(ref text, out T1 result1)
            && Condition2.Matches(ref text, out T2 result2)
            && Condition3.Matches(ref text, out T3 result3)
            && Condition4.Matches(ref text, out T4 result4)
            && Condition5.Matches(ref text, out T5 result5)
            ? Yes(() => Action(result1, result2, result3, result4, result5)) : No;
    }

    public class Command<T1, T2, T3, T4, T5, T6> : IRouteCommand
    {
        public TextArg<T1> Condition1 { get; init; }
        public TextArg<T2> Condition2 { get; init; }
        public TextArg<T3> Condition3 { get; init; }
        public TextArg<T4> Condition4 { get; init; }
        public TextArg<T5> Condition5 { get; init; }
        public TextArg<T6> Condition6 { get; init; }

        public Func<T1, T2, T3, T4, T5, T6, Task> Action { get; init; }

        public Command(Func<T1, T2, T3, T4, T5, T6, Task> command) => Action = command;

        public Func<ValueTask<bool>> Query(ReadOnlySpan<char> text) =>
               Condition1.Matches(ref text, out T1 result1)
            && Condition2.Matches(ref text, out T2 result2)
            && Condition3.Matches(ref text, out T3 result3)
            && Condition4.Matches(ref text, out T4 result4)
            && Condition5.Matches(ref text, out T5 result5)
            && Condition6.Matches(ref text, out T6 result6)
            ? Yes(() => Action(result1, result2, result3, result4, result5, result6)) : No;
    }

    public class CommandCondition<T1>
    {
        public readonly TextArg<T1> Condition1;

        public CommandCondition(TextArg<T1> condition1) => Condition1 = condition1;

        public Command<T1> Do(Func<T1, Task> command) => this | command;

        public static Command<T1> operator |(
            CommandCondition<T1> condition,
            Func<T1, Task> command) =>
        new(command)
        {
            Condition1 = condition.Condition1
        };
    }

    public class CommandCondition<T1, T2>
    {
        public TextArg<T1> Condition1 { get; init; }
        public TextArg<T2> Condition2 { get; init; }

        public Func<T1, T2, Task> Action { get; init; }

        public CommandCondition(TextArg<T1> condition1, TextArg<T2> condition2) =>
            (Condition1, Condition2) =
            (condition1, condition2);

        public Command<T1, T2> Do(Func<T1, T2, Task> command) => this | command;

        public static Command<T1, T2> operator |(
            CommandCondition<T1, T2> condition,
            Func<T1, T2, Task> command) =>
        new(command)
        {
            Condition1 = condition.Condition1,
            Condition2 = condition.Condition2
        };
    }

    public class CommandCondition<T1, T2, T3>
    {
        public TextArg<T1> Condition1 { get; init; }
        public TextArg<T2> Condition2 { get; init; }
        public TextArg<T3> Condition3 { get; init; }

        public Func<T1, T2, T3, Task> Action { get; init; }

        public CommandCondition(TextArg<T1> condition1, TextArg<T2> condition2, TextArg<T3> condition3) =>
            (Condition1, Condition2, Condition3) =
            (condition1, condition2, condition3);

        public Command<T1, T2, T3> Do(Func<T1, T2, T3, Task> command) => this | command;

        public static Command<T1, T2, T3> operator |(
            CommandCondition<T1, T2, T3> condition,
            Func<T1, T2, T3, Task> command) =>
        new(command)
        {
            Condition1 = condition.Condition1,
            Condition2 = condition.Condition2,
            Condition3 = condition.Condition3
        };
    }

    public class CommandCondition<T1, T2, T3, T4>
    {
        public TextArg<T1> Condition1 { get; init; }
        public TextArg<T2> Condition2 { get; init; }
        public TextArg<T3> Condition3 { get; init; }
        public TextArg<T4> Condition4 { get; init; }

        public Func<T1, T2, T3, T4, Task> Action { get; init; }

        public CommandCondition(TextArg<T1> condition1, TextArg<T2> condition2, TextArg<T3> condition3, TextArg<T4> condition4) =>
            (Condition1, Condition2, Condition3, Condition4) =
            (condition1, condition2, condition3, condition4);

        public Command<T1, T2, T3, T4> Do(Func<T1, T2, T3, T4, Task> command) => this | command;

        public static Command<T1, T2, T3, T4> operator |(
            CommandCondition<T1, T2, T3, T4> condition,
            Func<T1, T2, T3, T4, Task> command) =>
        new(command)
        {
            Condition1 = condition.Condition1,
            Condition2 = condition.Condition2,
            Condition3 = condition.Condition3,
            Condition4 = condition.Condition4
        };
    }

    public class CommandCondition<T1, T2, T3, T4, T5>
    {
        public TextArg<T1> Condition1 { get; init; }
        public TextArg<T2> Condition2 { get; init; }
        public TextArg<T3> Condition3 { get; init; }
        public TextArg<T4> Condition4 { get; init; }
        public TextArg<T5> Condition5 { get; init; }

        public Func<T1, T2, T3, T4, T5, Task> Action { get; init; }

        public CommandCondition(TextArg<T1> condition1, TextArg<T2> condition2, TextArg<T3> condition3, TextArg<T4> condition4, TextArg<T5> condition5) =>
            (Condition1, Condition2, Condition3, Condition4, Condition5) =
            (condition1, condition2, condition3, condition4, condition5);

        public Command<T1, T2, T3, T4, T5> Do(Func<T1, T2, T3, T4, T5, Task> command) => this | command;

        public static Command<T1, T2, T3, T4, T5> operator |(
            CommandCondition<T1, T2, T3, T4, T5> condition,
            Func<T1, T2, T3, T4, T5, Task> command) =>
        new(command)
        {
            Condition1 = condition.Condition1,
            Condition2 = condition.Condition2,
            Condition3 = condition.Condition3,
            Condition4 = condition.Condition4,
            Condition5 = condition.Condition5
        };
    }

    public class CommandCondition<T1, T2, T3, T4, T5, T6>
    {
        public TextArg<T1> Condition1 { get; init; }
        public TextArg<T2> Condition2 { get; init; }
        public TextArg<T3> Condition3 { get; init; }
        public TextArg<T4> Condition4 { get; init; }
        public TextArg<T5> Condition5 { get; init; }
        public TextArg<T6> Condition6 { get; init; }

        public Func<T1, T2, T3, T4, T5, T6, Task> Action { get; init; }

        public CommandCondition(TextArg<T1> condition1, TextArg<T2> condition2, TextArg<T3> condition3, TextArg<T4> condition4, TextArg<T5> condition5, TextArg<T6> condition6) =>
            (Condition1, Condition2, Condition3, Condition4, Condition5, Condition6) =
            (condition1, condition2, condition3, condition4, condition5, condition6);

        public Command<T1, T2, T3, T4, T5, T6> Do(Func<T1, T2, T3, T4, T5, T6, Task> command) => this | command;

        public static Command<T1, T2, T3, T4, T5, T6> operator |(
            CommandCondition<T1, T2, T3, T4, T5, T6> condition,
            Func<T1, T2, T3, T4, T5, T6, Task> command) =>
        new(command)
        {
            Condition1 = condition.Condition1,
            Condition2 = condition.Condition2,
            Condition3 = condition.Condition3,
            Condition4 = condition.Condition4,
            Condition5 = condition.Condition5,
            Condition6 = condition.Condition6
        };
    }
}
