using System;
using System.Threading.Tasks;

namespace Kelson.Common.Route
{
    using Args;
    using Kelson.Common.Route.Docs;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using static ContextFunctionPredicates;

    public enum RouteQueryResult
    {
        DO_NOT_RUN,
        RUN_AND_EXIT,
        RUN_AND_CONTIUE
    }

    public abstract class RouteCommand<TC>
    {
        // returns a function that will return false if the route does not match the parameter,
        // or will execute the route and then return true if the route does match the parameter
        public abstract RouteQueryResult Query(TC context, ReadOnlySpan<char> text, out Func<TC, Task> action);

        public string? Name { get; set; }
        public string? Description { get; set; }
        public abstract IEnumerable<string> Examples();

        protected static IEnumerable<string> Combine(params IEnumerable<string>[] sets) => Combine(TextArg<Unit, Unit>.CORE_ARG_DELIMETER, sets);

        protected static IEnumerable<string> Combine(string delimeter, params IEnumerable<string>[] columns)
        {
            var sets = columns.Select(column => column.ToArray()).ToArray();
            var count = (ulong)sets.Select(column => column.Length).Aggregate(1, (l, a) => l * a);
            var buffer = new string[sets.Length];
            for (ulong combo = 0UL; combo < count; combo++)
            {
                ulong w = combo; // mutable copy of combo
                for (int column = sets.Length - 1; column >= 0; column--)
                {
                    var set = sets[column];
                    ulong len = (ulong)set.Length;
                    buffer[column] = set[w % len];
                    w /= len;
                }
                yield return string.Join(delimeter, buffer);
            }
        }

        public virtual RouteDoc BuildDoc() => new(Name, Description, Examples().ToArray(), null);


    }

    public class Filter<TC> : RouteCommand<TC>
    {
        public TextArg<TC> Condition { get; init; }

        public Func<TC, Task> Action { get; init; }

        public RouteQueryResult OnPass { get; init; }

        public Filter(Func<TC, Task> command, TextArg<TC> condition, RouteQueryResult onPass) =>
            (Action, Condition, OnPass) = (command, condition, onPass);

        public override RouteQueryResult Query(TC context, ReadOnlySpan<char> text, out Func<TC, Task> action)
        {
            if (Condition.Matches(context, ref text, out Unit _))
            {
                action = Action;
                return OnPass;
            }
            else
            {
                action = default;
                return RouteQueryResult.DO_NOT_RUN;
            }
        }

        public override IEnumerable<string> Examples() => Condition.Examples();

    }

    public class ParameterFilter<TC, T1> : RouteCommand<TC>
    {
        public TextArg<TC, T1> Condition1 { get; init; }

        public Func<TC, T1, Task> Action { get; init; }

        public ParameterFilter(Func<TC, T1, Task> command) => Action = command;

        public override RouteQueryResult Query(TC context, ReadOnlySpan<char> text, out Func<TC, Task> action) =>
              Condition1.Matches(context, ref text, out T1 r1)
            ? Complete(c => Action(c, r1), out action) : No(out action);

        public override IEnumerable<string> Examples() => Condition1.Examples();
    }

    public class ParameterFilter<TC, T1, T2> : RouteCommand<TC>
    {
        public TextArg<TC, T1> Condition1 { get; init; }
        public TextArg<TC, T2> Condition2 { get; init; }

        public Func<TC, T1, T2, Task> Action { get; init; }

        public ParameterFilter(Func<TC, T1, T2, Task> command) => Action = command;

        public override RouteQueryResult Query(TC context, ReadOnlySpan<char> text, out Func<TC, Task> action) =>
               Condition1.Matches(context, ref text, out T1 r1)
            && Condition2.Matches(context, ref text, out T2 r2)
            ? Complete(c => Action(c, r1, r2), out action) : No(out action);

        public override IEnumerable<string> Examples() =>
            Combine(
                Condition1.Examples(),
                Condition2.Examples());

    }

    public class ParameterFilter<TC, T1, T2, T3> : RouteCommand<TC>
    {
        public TextArg<TC, T1> Condition1 { get; init; }
        public TextArg<TC, T2> Condition2 { get; init; }
        public TextArg<TC, T3> Condition3 { get; init; }

        public Func<TC, T1, T2, T3, Task> Action { get; init; }

        public ParameterFilter(Func<TC, T1, T2, T3, Task> command) => Action = command;

        public override RouteQueryResult Query(TC context, ReadOnlySpan<char> text, out Func<TC, Task> action) =>
               Condition1.Matches(context, ref text, out T1 result1)
            && Condition2.Matches(context, ref text, out T2 result2)
            && Condition3.Matches(context, ref text, out T3 result3)
            ? Complete(c => Action(c, result1, result2, result3), out action) : No(out action);

        public override IEnumerable<string> Examples() =>
            Combine(
                Condition1.Examples(),
                Condition2.Examples(),
                Condition3.Examples());
    }


    public class ParamterFilter<TC, T1, T2, T3, T4> : RouteCommand<TC>
    {
        public TextArg<TC, T1> Condition1 { get; init; }
        public TextArg<TC, T2> Condition2 { get; init; }
        public TextArg<TC, T3> Condition3 { get; init; }
        public TextArg<TC, T4> Condition4 { get; init; }

        public Func<TC, T1, T2, T3, T4, Task> Action { get; init; }

        public ParamterFilter(Func<TC, T1, T2, T3, T4, Task> command) => Action = command;

        public override RouteQueryResult Query(TC context, ReadOnlySpan<char> text, out Func<TC, Task> action) =>
               Condition1.Matches(context, ref text, out T1 r1)
            && Condition2.Matches(context, ref text, out T2 r2)
            && Condition3.Matches(context, ref text, out T3 r3)
            && Condition4.Matches(context, ref text, out T4 r4)
            ? Complete(c => Action(c, r1, r2, r3, r4), out action) : No(out action);

        public override IEnumerable<string> Examples() =>
            Combine(
                Condition1.Examples(),
                Condition2.Examples(),
                Condition3.Examples(),
                Condition4.Examples());
    }


    public class ParameterFilter<TC, T1, T2, T3, T4, T5> : RouteCommand<TC>
    {
        public TextArg<TC, T1> Condition1 { get; init; }
        public TextArg<TC, T2> Condition2 { get; init; }
        public TextArg<TC, T3> Condition3 { get; init; }
        public TextArg<TC, T4> Condition4 { get; init; }
        public TextArg<TC, T5> Condition5 { get; init; }

        public Func<TC, T1, T2, T3, T4, T5, Task> Action { get; init; }

        public ParameterFilter(Func<TC, T1, T2, T3, T4, T5, Task> command) => Action = command;

        public override RouteQueryResult Query(TC context, ReadOnlySpan<char> text, out Func<TC, Task> action) =>
               Condition1.Matches(context, ref text, out T1 r1)
            && Condition2.Matches(context, ref text, out T2 r2)
            && Condition3.Matches(context, ref text, out T3 r3)
            && Condition4.Matches(context, ref text, out T4 r4)
            && Condition5.Matches(context, ref text, out T5 r5)
            ? Complete(c => Action(c, r1, r2, r3, r4, r5), out action) : No(out action);

        public override IEnumerable<string> Examples() =>
            Combine(
                Condition1.Examples(),
                Condition2.Examples(),
                Condition3.Examples(),
                Condition4.Examples(),
                Condition5.Examples());
    }

    public class ParameterFilter<TC, T1, T2, T3, T4, T5, T6> : RouteCommand<TC>
    {
        public TextArg<TC, T1> Condition1 { get; init; }
        public TextArg<TC, T2> Condition2 { get; init; }
        public TextArg<TC, T3> Condition3 { get; init; }
        public TextArg<TC, T4> Condition4 { get; init; }
        public TextArg<TC, T5> Condition5 { get; init; }
        public TextArg<TC, T6> Condition6 { get; init; }

        public Func<TC, T1, T2, T3, T4, T5, T6, Task> Action { get; init; }

        public ParameterFilter(Func<TC, T1, T2, T3, T4, T5, T6, Task> command) => Action = command;

        public override RouteQueryResult Query(TC context, ReadOnlySpan<char> text, out Func<TC, Task> action) =>
               Condition1.Matches(context, ref text, out T1 r1)
            && Condition2.Matches(context, ref text, out T2 r2)
            && Condition3.Matches(context, ref text, out T3 r3)
            && Condition4.Matches(context, ref text, out T4 r4)
            && Condition5.Matches(context, ref text, out T5 r5)
            && Condition6.Matches(context, ref text, out T6 r6)
            ? Complete(c => Action(c, r1, r2, r3, r4, r5, r6), out action) : No(out action);

        public override IEnumerable<string> Examples() =>
            Combine(
                Condition1.Examples(),
                Condition2.Examples(),
                Condition3.Examples(),
                Condition4.Examples(),
                Condition5.Examples(),
                Condition6.Examples());
    }

    public class RouteCondition<TC, TSelf>
        where TSelf : RouteCondition<TC, TSelf>
    {
        public RouteBuilder<TC> Router { get; init; }
        public RouteQueryResult OnPass { get; init; } = RouteQueryResult.RUN_AND_EXIT;
        public string? EndpointName { get; set; }
        public TSelf Named(string name)
        {
            EndpointName = name;
            return (TSelf)this;
        }

        public string? Description { get; set; }
        public TSelf DescribedAs(string description)
        {
            Description = description;
            return (TSelf)this;
        }

        protected RouteCondition(RouteBuilder<TC> router, RouteQueryResult onPass) =>
            (Router, OnPass) = (router, onPass);
    }

    public class Condition<TC> : RouteCondition<TC, Condition<TC>>
    {
        public TextArg<TC> Condition1 { get; init; }

        public Condition(RouteBuilder<TC> router, TextArg<TC> condition, RouteQueryResult onPass = RouteQueryResult.RUN_AND_EXIT)
            : base(router, onPass) =>
            Condition1 = condition;

        public virtual RouteBuilder<TC> Do(Func<TC, Task> command) =>
            Router.WithCommand(new Filter<TC>(command, Condition1, OnPass) { Name = EndpointName, Description = Description });

        public ParameterCondition<TC, T1> On<T1>(TextArg<TC, T1> arg) =>
            new(Router, Condition1.Then(arg)) { OnPass = OnPass, EndpointName = EndpointName, Description = Description };

        public ParameterCondition<TC, T1, T2> On<T1, T2>(TextArg<TC, T1> arg1, TextArg<TC, T2> arg2) =>
            new(Router, Condition1.Then(arg1), arg2) { OnPass = OnPass, EndpointName = EndpointName, Description = Description };

        public ParameterCondition<TC, T1, T2, T3> On<T1, T2, T3>(TextArg<TC, T1> arg1, TextArg<TC, T2> arg2, TextArg<TC, T3> arg3) =>
            new(Router, Condition1.Then(arg1), arg2, arg3) { OnPass = OnPass, EndpointName = EndpointName, Description = Description };

        public ParameterCondition<TC, T1, T2, T3, T4> On<T1, T2, T3, T4>(TextArg<TC, T1> arg1, TextArg<TC, T2> arg2, TextArg<TC, T3> arg3, TextArg<TC, T4> arg4) =>
            new(Router, Condition1.Then(arg1), arg2, arg3, arg4) { OnPass = OnPass, EndpointName = EndpointName, Description = Description };

        public ParameterCondition<TC, T1, T2, T3, T4, T5> On<T1, T2, T3, T4, T5>(TextArg<TC, T1> arg1, TextArg<TC, T2> arg2, TextArg<TC, T3> arg3, TextArg<TC, T4> arg4, TextArg<TC, T5> arg5) =>
            new(Router, Condition1.Then(arg1), arg2, arg3, arg4, arg5) { OnPass = OnPass, EndpointName = EndpointName, Description = Description };

        public ParameterCondition<TC, T1, T2, T3, T4, T5, T6> On<T1, T2, T3, T4, T5, T6>(TextArg<TC, T1> arg1, TextArg<TC, T2> arg2, TextArg<TC, T3> arg3, TextArg<TC, T4> arg4, TextArg<TC, T5> arg5, TextArg<TC, T6> arg6) =>
            new(Router, Condition1.Then(arg1), arg2, arg3, arg4, arg5, arg6) { OnPass = OnPass, EndpointName = EndpointName, Description = Description };
    }


    public class ParameterCondition<TC, T1> : RouteCondition<TC, ParameterCondition<TC, T1>>
    {
        public readonly TextArg<TC, T1> Condition1;

        public ParameterCondition(RouteBuilder<TC> router, TextArg<TC, T1> condition1)
            : base(router, RouteQueryResult.RUN_AND_EXIT) =>
            Condition1 = condition1;

        //public ParameterConditionMap<TC, TC2, T1> Select<TC2>(Func<TC, TC2> map) => new(map, this);

        //public ParameterConditionFilter<TC, TC, T1> When(Func<TC, T1, bool> filter) => new(filter, this);

        public virtual RouteBuilder<TC> Do(Func<TC, T1, Task> command, string? name = null) =>
            Router.WithCommand(
                new ParameterFilter<TC, T1>(command)
                {
                    Name = name ?? EndpointName ?? command?.Method?.ToName() ?? "Unkown Command",
                    Description = Description,
                    Condition1 = Condition1
                });
    }

    public class ParameterCondition<TC, T1, T2> : RouteCondition<TC, ParameterCondition<TC, T1, T2>>
    {
        public TextArg<TC, T1> Condition1 { get; init; }
        public TextArg<TC, T2> Condition2 { get; init; }

        public ParameterCondition(RouteBuilder<TC> router, TextArg<TC, T1> condition1, TextArg<TC, T2> condition2)
            : base(router, RouteQueryResult.RUN_AND_EXIT) =>
            (Condition1, Condition2) = (condition1, condition2);

        public RouteBuilder<TC> Do(Func<TC, T1, T2, Task> command, string? name = null) =>
            Router.WithCommand(
                new ParameterFilter<TC, T1, T2>(command)
                {
                    Name = name ?? EndpointName ?? command?.Method?.ToName() ?? "Unkown Command",
                    Description = Description,
                    Condition1 = Condition1,
                    Condition2 = Condition2
                });
    }

    public class ParameterCondition<TC, T1, T2, T3> : RouteCondition<TC, ParameterCondition<TC, T1, T2, T3>>
    {
        public TextArg<TC, T1> Condition1 { get; init; }
        public TextArg<TC, T2> Condition2 { get; init; }
        public TextArg<TC, T3> Condition3 { get; init; }

        public ParameterCondition(RouteBuilder<TC> router, TextArg<TC, T1> condition1, TextArg<TC, T2> condition2, TextArg<TC, T3> condition3)
            : base(router, RouteQueryResult.RUN_AND_EXIT) =>
            (Condition1, Condition2, Condition3) =
            (condition1, condition2, condition3);

        public RouteBuilder<TC> Do(Func<TC, T1, T2, T3, Task> command, string? name = null) =>
            Router.WithCommand(
                new ParameterFilter<TC, T1, T2, T3>(command)
                {
                    Name = name ?? EndpointName ?? command?.Method?.ToName() ?? "Unkown Command",
                    Description = Description,
                    Condition1 = Condition1,
                    Condition2 = Condition2,
                    Condition3 = Condition3
                });
    }

    public class ParameterCondition<TC, T1, T2, T3, T4> : RouteCondition<TC, ParameterCondition<TC, T1, T2, T3, T4>>
    {
        public TextArg<TC, T1> Condition1 { get; init; }
        public TextArg<TC, T2> Condition2 { get; init; }
        public TextArg<TC, T3> Condition3 { get; init; }
        public TextArg<TC, T4> Condition4 { get; init; }

        public ParameterCondition(RouteBuilder<TC> router, TextArg<TC, T1> condition1, TextArg<TC, T2> condition2, TextArg<TC, T3> condition3, TextArg<TC, T4> condition4)
            : base(router, RouteQueryResult.RUN_AND_EXIT) =>
            (Condition1, Condition2, Condition3, Condition4) =
            (condition1, condition2, condition3, condition4);

        public RouteBuilder<TC> Do(Func<TC, T1, T2, T3, T4, Task> command, string? name = null) =>
            Router.WithCommand(
                new ParamterFilter<TC, T1, T2, T3, T4>(command)
                {
                    Name = name ?? EndpointName ?? command?.Method?.ToName() ?? "Unkown Command",
                    Description = Description,
                    Condition1 = Condition1,
                    Condition2 = Condition2,
                    Condition3 = Condition3,
                    Condition4 = Condition4
                });
    }

    public class ParameterCondition<TC, T1, T2, T3, T4, T5> : RouteCondition<TC, ParameterCondition<TC, T1, T2, T3, T4, T5>>
    {
        public TextArg<TC, T1> Condition1 { get; init; }
        public TextArg<TC, T2> Condition2 { get; init; }
        public TextArg<TC, T3> Condition3 { get; init; }
        public TextArg<TC, T4> Condition4 { get; init; }
        public TextArg<TC, T5> Condition5 { get; init; }

        public ParameterCondition(RouteBuilder<TC> router, TextArg<TC, T1> condition1, TextArg<TC, T2> condition2, TextArg<TC, T3> condition3, TextArg<TC, T4> condition4, TextArg<TC, T5> condition5)
            : base(router, RouteQueryResult.RUN_AND_EXIT) =>
            (Condition1, Condition2, Condition3, Condition4, Condition5) =
            (condition1, condition2, condition3, condition4, condition5);

        public RouteBuilder<TC> Do(Func<TC, T1, T2, T3, T4, T5, Task> command, string? name = null) =>
            Router.WithCommand(
                new ParameterFilter<TC, T1, T2, T3, T4, T5>(command)
                {
                    Name = name ?? EndpointName ?? command?.Method?.ToName() ?? "Unkown Command",
                    Description = Description,
                    Condition1 = Condition1,
                    Condition2 = Condition2,
                    Condition3 = Condition3,
                    Condition4 = Condition4,
                    Condition5 = Condition5
                });
    }

    public class ParameterCondition<TC, T1, T2, T3, T4, T5, T6> : RouteCondition<TC, ParameterCondition<TC, T1, T2, T3, T4, T5, T6>>
    {
        public TextArg<TC, T1> Condition1 { get; init; }
        public TextArg<TC, T2> Condition2 { get; init; }
        public TextArg<TC, T3> Condition3 { get; init; }
        public TextArg<TC, T4> Condition4 { get; init; }
        public TextArg<TC, T5> Condition5 { get; init; }
        public TextArg<TC, T6> Condition6 { get; init; }

        public ParameterCondition(RouteBuilder<TC> router, TextArg<TC, T1> condition1, TextArg<TC, T2> condition2, TextArg<TC, T3> condition3, TextArg<TC, T4> condition4, TextArg<TC, T5> condition5, TextArg<TC, T6> condition6)
            : base(router, RouteQueryResult.RUN_AND_EXIT) =>
            (Condition1, Condition2, Condition3, Condition4, Condition5, Condition6) =
            (condition1, condition2, condition3, condition4, condition5, condition6);

        public RouteBuilder<TC> Do(Func<TC, T1, T2, T3, T4, T5, T6, Task> command, string? name = null) =>
            Router.WithCommand(
                new ParameterFilter<TC, T1, T2, T3, T4, T5, T6>(command)
                {
                    Name = name ?? EndpointName ?? command?.Method?.ToName() ?? "Unkown Command",
                    Description = Description,
                    Condition1 = Condition1,
                    Condition2 = Condition2,
                    Condition3 = Condition3,
                    Condition4 = Condition4,
                    Condition5 = Condition5,
                    Condition6 = Condition6
                });
    }

    internal static class MethodInfoExtensions
    {
        internal static string? ToName(this MethodInfo? method)
        {
            if (method == null)
                return null;
            int __index = method.Name.IndexOf("__") + 2;
            if (__index > 1)
            {
                int barIndex = method.Name.LastIndexOf("|");
                if (barIndex >= 0)
                    return method.Name[__index..barIndex];
            }
            else if (char.IsLetter(method.Name[0]))
                return method.Name;
            return null;
        }
    }
}
