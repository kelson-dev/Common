using System;
using System.Threading.Tasks;

namespace Kelson.Common.Route
{
    using Args;
    using static ContextFunctionPredicates;

    public enum RouteQueryResult
    {
        DO_NOT_RUN,
        RUN_AND_EXIT,
        RUN_AND_CONTIUE
    }

    public interface IRouteCommand<TC>
    {
        // returns a function that will return false if the route does not match the parameter,
        // or will execute the route and then return true if the route does match the parameter
        RouteQueryResult Query(TC context, ReadOnlySpan<char> text, out Func<TC, Task> action);
    }

    public class Filter<TC> : IRouteCommand<TC>
    {
        public TextArg<TC> Condition { get; init; }

        public Func<TC, Task> Action { get; init; }

        public RouteQueryResult OnPass { get; init; }

        public Filter(Func<TC, Task> command, TextArg<TC> condition, RouteQueryResult onPass) => 
            (Action, Condition, OnPass) = (command, condition, onPass);

        public RouteQueryResult Query(TC context, ReadOnlySpan<char> text, out Func<TC, Task> action)
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
    }

    public class ParameterFilter<TC, T1> : IRouteCommand<TC>
    {
        public TextArg<TC, T1> Condition1 { get; init; }

        public Func<TC, T1, Task> Action { get; init; }

        public ParameterFilter(Func<TC, T1, Task> command) => Action = command;

        public RouteQueryResult Query(TC context, ReadOnlySpan<char> text, out Func<TC, Task> action) =>
              Condition1.Matches(context, ref text, out T1 r1)
            ? Complete(c => Action(c, r1), out action) : No(out action);
    }

    public class ParameterFilter<TC, T1, T2> : IRouteCommand<TC>
    {
        public TextArg<TC, T1> Condition1 { get; init; }
        public TextArg<TC, T2> Condition2 { get; init; }

        public Func<TC, T1, T2, Task> Action { get; init; }

        public ParameterFilter(Func<TC, T1, T2, Task> command) => Action = command;

        public RouteQueryResult Query(TC context, ReadOnlySpan<char> text, out Func<TC, Task> action) =>
               Condition1.Matches(context, ref text, out T1 r1)
            && Condition2.Matches(context, ref text, out T2 r2)
            ? Complete(c => Action(c, r1, r2), out action) : No(out action);
    }

    public class ParameterFilter<TC, T1, T2, T3> : IRouteCommand<TC>
    {
        public TextArg<TC, T1> Condition1 { get; init; }
        public TextArg<TC, T2> Condition2 { get; init; }
        public TextArg<TC, T3> Condition3 { get; init; }

        public Func<TC, T1, T2, T3, Task> Action { get; init; }

        public ParameterFilter(Func<TC, T1, T2, T3, Task> command) => Action = command;

        public RouteQueryResult Query(TC context, ReadOnlySpan<char> text, out Func<TC, Task> action) =>
               Condition1.Matches(context, ref text, out T1 result1)
            && Condition2.Matches(context, ref text, out T2 result2)
            && Condition3.Matches(context, ref text, out T3 result3)
            ? Complete(c => Action(c, result1, result2, result3), out action) : No(out action);
    }


    public class ParamterFilter<TC, T1, T2, T3, T4> : IRouteCommand<TC>
    {
        public TextArg<TC, T1> Condition1 { get; init; }
        public TextArg<TC, T2> Condition2 { get; init; }
        public TextArg<TC, T3> Condition3 { get; init; }
        public TextArg<TC, T4> Condition4 { get; init; }

        public Func<TC, T1, T2, T3, T4, Task> Action { get; init; }

        public ParamterFilter(Func<TC, T1, T2, T3, T4, Task> command) => Action = command;

        public RouteQueryResult Query(TC context, ReadOnlySpan<char> text, out Func<TC, Task> action) =>
               Condition1.Matches(context, ref text, out T1 r1)
            && Condition2.Matches(context, ref text, out T2 r2)
            && Condition3.Matches(context, ref text, out T3 r3)
            && Condition4.Matches(context, ref text, out T4 r4)
            ? Complete(c => Action(c, r1, r2, r3, r4), out action) : No(out action);
    }


    public class ParameterFilter<TC, T1, T2, T3, T4, T5> : IRouteCommand<TC>
    {
        public TextArg<TC, T1> Condition1 { get; init; }
        public TextArg<TC, T2> Condition2 { get; init; }
        public TextArg<TC, T3> Condition3 { get; init; }
        public TextArg<TC, T4> Condition4 { get; init; }
        public TextArg<TC, T5> Condition5 { get; init; }

        public Func<TC, T1, T2, T3, T4, T5, Task> Action { get; init; }

        public ParameterFilter(Func<TC, T1, T2, T3, T4, T5, Task> command) => Action = command;

        public RouteQueryResult Query(TC context, ReadOnlySpan<char> text, out Func<TC, Task> action) =>
               Condition1.Matches(context, ref text, out T1 r1)
            && Condition2.Matches(context, ref text, out T2 r2)
            && Condition3.Matches(context, ref text, out T3 r3)
            && Condition4.Matches(context, ref text, out T4 r4)
            && Condition5.Matches(context, ref text, out T5 r5)
            ? Complete(c => Action(c, r1, r2, r3, r4, r5), out action) : No(out action);
    }

    public class ParameterFilter<TC, T1, T2, T3, T4, T5, T6> : IRouteCommand<TC>
    {
        public TextArg<TC, T1> Condition1 { get; init; }
        public TextArg<TC, T2> Condition2 { get; init; }
        public TextArg<TC, T3> Condition3 { get; init; }
        public TextArg<TC, T4> Condition4 { get; init; }
        public TextArg<TC, T5> Condition5 { get; init; }
        public TextArg<TC, T6> Condition6 { get; init; }

        public Func<TC, T1, T2, T3, T4, T5, T6, Task> Action { get; init; }

        public ParameterFilter(Func<TC, T1, T2, T3, T4, T5, T6, Task> command) => Action = command;

        public RouteQueryResult Query(TC context, ReadOnlySpan<char> text, out Func<TC, Task> action) =>
               Condition1.Matches(context, ref text, out T1 r1)
            && Condition2.Matches(context, ref text, out T2 r2)
            && Condition3.Matches(context, ref text, out T3 r3)
            && Condition4.Matches(context, ref text, out T4 r4)
            && Condition5.Matches(context, ref text, out T5 r5)
            && Condition6.Matches(context, ref text, out T6 r6)
            ? Complete(c => Action(c, r1, r2, r3, r4, r5, r6), out action) : No(out action);
    }

    public interface ICondition<TC>
    {
        RouteBuilder<TC> Router { get; init; }
        RouteQueryResult OnPass { get; init; }
    }

    public class Condition<TC> : ICondition<TC>
    {
        public RouteBuilder<TC> Router { get; init; }
        public TextArg<TC> Condition1 { get; init; }
        public RouteQueryResult OnPass { get; init; }

        public Condition(RouteBuilder<TC> router, TextArg<TC> condition, RouteQueryResult onPass = RouteQueryResult.RUN_AND_EXIT) => 
            (Router, Condition1, OnPass) = (router, condition, onPass);

        public virtual RouteBuilder<TC> Do(Func<TC, Task> command) =>
            Router.WithCommand(new Filter<TC>(command, Condition1, OnPass));

        public ParameterCondition<TC, T1> On<T1>(TextArg<TC, T1> arg) =>
            new(Router, Condition1.Then(arg)) { OnPass = OnPass };

        public ParameterCondition<TC, T1, T2> On<T1, T2>(TextArg<TC, T1> arg1, TextArg<TC, T2> arg2) =>
            new(Router, Condition1.Then(arg1), arg2) { OnPass = OnPass };

        public ParameterCondition<TC, T1, T2, T3> On<T1, T2, T3>(TextArg<TC, T1> arg1, TextArg<TC, T2> arg2, TextArg<TC, T3> arg3) =>
            new(Router, Condition1.Then(arg1), arg2, arg3) { OnPass = OnPass };

        public ParameterCondition<TC, T1, T2, T3, T4> On<T1, T2, T3, T4>(TextArg<TC, T1> arg1, TextArg<TC, T2> arg2, TextArg<TC, T3> arg3, TextArg<TC, T4> arg4) =>
            new(Router, Condition1.Then(arg1), arg2, arg3, arg4) { OnPass = OnPass };

        public ParameterCondition<TC, T1, T2, T3, T4, T5> On<T1, T2, T3, T4, T5>(TextArg<TC, T1> arg1, TextArg<TC, T2> arg2, TextArg<TC, T3> arg3, TextArg<TC, T4> arg4, TextArg<TC, T5> arg5) =>
            new(Router, Condition1.Then(arg1), arg2, arg3, arg4, arg5) { OnPass = OnPass };

        public ParameterCondition<TC, T1, T2, T3, T4, T5, T6> On<T1, T2, T3, T4, T5, T6>(TextArg<TC, T1> arg1, TextArg<TC, T2> arg2, TextArg<TC, T3> arg3, TextArg<TC, T4> arg4, TextArg<TC, T5> arg5, TextArg<TC, T6> arg6) =>
            new(Router, Condition1.Then(arg1), arg2, arg3, arg4, arg5, arg6) { OnPass = OnPass };
    }


    public class ParameterCondition<TC, T1> : ICondition<TC>
    {
        public RouteBuilder<TC> Router { get; init; }
        public readonly TextArg<TC, T1> Condition1;
        public RouteQueryResult OnPass { get; init; } = RouteQueryResult.RUN_AND_EXIT;

        public ParameterCondition(RouteBuilder<TC> router, TextArg<TC, T1> condition1) => (Router, Condition1) = (router, condition1);

        //public ParameterConditionMap<TC, TC2, T1> Select<TC2>(Func<TC, TC2> map) => new(map, this);

        //public ParameterConditionFilter<TC, TC, T1> When(Func<TC, T1, bool> filter) => new(filter, this);

        public virtual RouteBuilder<TC> Do(Func<TC, T1, Task> command) =>
            Router.WithCommand(
                new ParameterFilter<TC, T1>(command)
                {
                    Condition1 = Condition1
                });
    }

    public class ParameterCondition<TC, T1, T2> : ICondition<TC>
    {
        public RouteBuilder<TC> Router { get; init; }
        public TextArg<TC, T1> Condition1 { get; init; }
        public TextArg<TC, T2> Condition2 { get; init; }
        public RouteQueryResult OnPass { get; init; } = RouteQueryResult.RUN_AND_EXIT;

        public Func<T1, T2, Task> Action { get; init; }

        public ParameterCondition(RouteBuilder<TC> router, TextArg<TC, T1> condition1, TextArg<TC, T2> condition2) =>
            (Router, Condition1, Condition2) =
            (router, condition1, condition2);

        public RouteBuilder<TC> Do(Func<TC, T1, T2, Task> command) =>
            Router.WithCommand(
                new ParameterFilter<TC, T1, T2>(command)
                {
                    Condition1 = Condition1,
                    Condition2 = Condition2
                });        
    }

    public class ParameterCondition<TC, T1, T2, T3> : ICondition<TC>
    {
        public RouteBuilder<TC> Router { get; init; }
        public TextArg<TC, T1> Condition1 { get; init; }
        public TextArg<TC, T2> Condition2 { get; init; }
        public TextArg<TC, T3> Condition3 { get; init; }
        public RouteQueryResult OnPass { get; init; } = RouteQueryResult.RUN_AND_EXIT;
        public Func<TC, T1, T2, T3, Task> Action { get; init; }

        public ParameterCondition(RouteBuilder<TC> router, TextArg<TC, T1> condition1, TextArg<TC, T2> condition2, TextArg<TC, T3> condition3) =>
            (Router, Condition1, Condition2, Condition3) =
            (router, condition1, condition2, condition3);

        public RouteBuilder<TC> Do(Func<TC, T1, T2, T3, Task> command) => 
			Router.WithCommand(
                new ParameterFilter<TC, T1, T2, T3>(command)
                {
                    Condition1 = Condition1,
                    Condition2 = Condition2,
                    Condition3 = Condition3
                });
    }

    public class ParameterCondition<TC, T1, T2, T3, T4> : ICondition<TC>
    {
        public RouteBuilder<TC> Router { get; init; }
        public TextArg<TC, T1> Condition1 { get; init; }
        public TextArg<TC, T2> Condition2 { get; init; }
        public TextArg<TC, T3> Condition3 { get; init; }
        public TextArg<TC, T4> Condition4 { get; init; }
        public RouteQueryResult OnPass { get; init; } = RouteQueryResult.RUN_AND_EXIT;
        public Func<TC, T1, T2, T3, T4, Task> Action { get; init; }

        public ParameterCondition(RouteBuilder<TC> router, TextArg<TC, T1> condition1, TextArg<TC, T2> condition2, TextArg<TC, T3> condition3, TextArg<TC, T4> condition4) =>
            (Router, Condition1, Condition2, Condition3, Condition4) =
            (router, condition1, condition2, condition3, condition4);

        public RouteBuilder<TC> Do(Func<TC, T1, T2, T3, T4, Task> command) => 
			Router.WithCommand(
                new ParamterFilter<TC, T1, T2, T3, T4>(command)
                {
                    Condition1 = Condition1,
                    Condition2 = Condition2,
                    Condition3 = Condition3,
                    Condition4 = Condition4
                });
    }

    public class ParameterCondition<TC, T1, T2, T3, T4, T5> : ICondition<TC>
    {
        public RouteBuilder<TC> Router { get; init; }
        public TextArg<TC, T1> Condition1 { get; init; }
        public TextArg<TC, T2> Condition2 { get; init; }
        public TextArg<TC, T3> Condition3 { get; init; }
        public TextArg<TC, T4> Condition4 { get; init; }
        public TextArg<TC, T5> Condition5 { get; init; }
        public RouteQueryResult OnPass { get; init; } = RouteQueryResult.RUN_AND_EXIT;
        public Func<TC, T1, T2, T3, T4, T5, Task> Action { get; init; }

        public ParameterCondition(RouteBuilder<TC> router, TextArg<TC, T1> condition1, TextArg<TC, T2> condition2, TextArg<TC, T3> condition3, TextArg<TC, T4> condition4, TextArg<TC, T5> condition5) =>
            (Router, Condition1, Condition2, Condition3, Condition4, Condition5) =
            (router, condition1, condition2, condition3, condition4, condition5);

        public RouteBuilder<TC> Do(Func<TC, T1, T2, T3, T4, T5, Task> command) => 
			Router.WithCommand(
                new ParameterFilter<TC, T1, T2, T3, T4, T5>(command)
                {
                    Condition1 = Condition1,
                    Condition2 = Condition2,
                    Condition3 = Condition3,
                    Condition4 = Condition4,
                    Condition5 = Condition5
                });
    }

    public class ParameterCondition<TC, T1, T2, T3, T4, T5, T6> : ICondition<TC>
    {
        public RouteBuilder<TC> Router { get; init; }
        public TextArg<TC, T1> Condition1 { get; init; }
        public TextArg<TC, T2> Condition2 { get; init; }
        public TextArg<TC, T3> Condition3 { get; init; }
        public TextArg<TC, T4> Condition4 { get; init; }
        public TextArg<TC, T5> Condition5 { get; init; }
        public TextArg<TC, T6> Condition6 { get; init; }
        public RouteQueryResult OnPass { get; init; } = RouteQueryResult.RUN_AND_EXIT;
        public Func<TC, T1, T2, T3, T4, T5, T6, Task> Action { get; init; }

        public ParameterCondition(RouteBuilder<TC> router, TextArg<TC, T1> condition1, TextArg<TC, T2> condition2, TextArg<TC, T3> condition3, TextArg<TC, T4> condition4, TextArg<TC, T5> condition5, TextArg<TC, T6> condition6) =>
            (Router, Condition1, Condition2, Condition3, Condition4, Condition5, Condition6) =
            (router, condition1, condition2, condition3, condition4, condition5, condition6);

        public RouteBuilder<TC> Do(Func<TC, T1, T2, T3, T4, T5, T6, Task> command) => 
			Router.WithCommand(
                new ParameterFilter<TC, T1, T2, T3, T4, T5, T6>(command)
                {
                    Condition1 = Condition1,
                    Condition2 = Condition2,
                    Condition3 = Condition3,
                    Condition4 = Condition4,
                    Condition5 = Condition5,
                    Condition6 = Condition6
                });
    }
}
