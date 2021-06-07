using System;
using System.Threading.Tasks;

namespace Kelson.Common.Route
{
    using Args;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    public class Router<TC>
    {
        internal readonly Func<TC, string> textSelector;
        internal readonly IRouteCommand<TC>[] commands;

        internal Router(Func<TC, string> textSelector, IRouteCommand<TC>[] commands)
        {
            this.textSelector = textSelector;
            this.commands = commands;
        }

        public async Task Handle(TC context)
        {
            var text = textSelector(context);
            foreach (var command in commands)
            {
                switch (command.Query(context, text, out Func<TC, Task> execute))
                {
                    case RouteQueryResult.RUN_AND_CONTIUE:
                        await execute(context);
                        break;
                    case RouteQueryResult.RUN_AND_EXIT:
                        await execute(context);
                        return;
                    default:
                        break;
                }
            }
        }
    }

    public class RouteBuilder<TC>
    {
        internal readonly Func<TC, string> textSelector;
        internal readonly ImmutableList<IRouteCommand<TC>> commands;

        public RouteBuilder(Func<TC, string> textSelector)
        {
            this.textSelector = textSelector;
            this.commands = ImmutableList<IRouteCommand<TC>>.Empty;
        }

        private RouteBuilder(Func<TC, string> textSelector, ImmutableList<IRouteCommand<TC>> commands)
        {
            this.textSelector = textSelector;
            this.commands = commands;
        }

        public Router<TC> Build() => new Router<TC>(textSelector, commands.ToArray());

        public RouteBuilder<TC> WithCommand(IRouteCommand<TC> command) => new(textSelector, commands.Add(command));

        public RouteBuilder<TC> When(Func<TC, bool> condition, RouteBuilder<TC> innerRouteBuilder) =>
            WithCommand(
                new ConditionRouteCommand<TC>(
                    RouteQueryResult.RUN_AND_CONTIUE,
                    condition,
                    innerRouteBuilder));

        public BranchingRouteBuilder<TC> If(Func<TC, bool> condition, RouteBuilder<TC> innerRouteBuilder) =>
            new(this, condition, innerRouteBuilder);

        public Condition<TC> When(TextArg<TC> condition) =>
            new(this, condition);

        public ParameterCondition<TC, T1> On<T1>(TextArg<TC, T1> arg) =>
            new(this, arg);

        public ParameterCondition<TC, T1, T2> On<T1, T2>(TextArg<TC, T1> arg1, TextArg<TC, T2> arg2) =>
            new(this, arg1, arg2);

        public ParameterCondition<TC, T1, T2, T3> On<T1, T2, T3>(TextArg<TC, T1> arg1, TextArg<TC, T2> arg2, TextArg<TC, T3> arg3) =>
            new(this, arg1, arg2, arg3);

        public ParameterCondition<TC, T1, T2, T3, T4> On<T1, T2, T3, T4>(TextArg<TC, T1> arg1, TextArg<TC, T2> arg2, TextArg<TC, T3> arg3, TextArg<TC, T4> arg4) =>
            new(this, arg1, arg2, arg3, arg4);

        public ParameterCondition<TC, T1, T2, T3, T4, T5> On<T1, T2, T3, T4, T5>(TextArg<TC, T1> arg1, TextArg<TC, T2> arg2, TextArg<TC, T3> arg3, TextArg<TC, T4> arg4, TextArg<TC, T5> arg5) =>
            new(this, arg1, arg2, arg3, arg4, arg5);

        public ParameterCondition<TC, T1, T2, T3, T4, T5, T6> On<T1, T2, T3, T4, T5, T6>(TextArg<TC, T1> arg1, TextArg<TC, T2> arg2, TextArg<TC, T3> arg3, TextArg<TC, T4> arg4, TextArg<TC, T5> arg5, TextArg<TC, T6> arg6) =>
            new(this, arg1, arg2, arg3, arg4, arg5, arg6);
    }

    internal static class ContextFunctionPredicates
    {
        public static RouteQueryResult No<TC>(out Func<TC, Task> set)
        {
            set = default;
            return RouteQueryResult.DO_NOT_RUN;
        }

        public static RouteQueryResult Complete<TC>(Func<TC, Task> value, out Func<TC, Task> set)
        {
            set = value;
            return RouteQueryResult.RUN_AND_EXIT;
        }
        
        public static RouteQueryResult Continue<TC>(Func<TC, Task> value, out Func<TC, Task> set)
        {
            set = value;
            return RouteQueryResult.RUN_AND_CONTIUE;
        }
    }

    public class BranchingRouteBuilder<TC>
    {
        public RouteBuilder<TC> Return { get; private set; }

        private readonly Func<TC, bool> firstCondition;
        private readonly RouteBuilder<TC> firstRouter;
        private readonly ImmutableList<(Func<TC, bool>, RouteBuilder<TC>)> elseBranches;


        internal BranchingRouteBuilder(RouteBuilder<TC> outer, Func<TC, bool> firstCondition, RouteBuilder<TC> firstRouter)
        {
            Return = outer;
            elseBranches = ImmutableList<(Func<TC, bool>, RouteBuilder<TC>)>.Empty;
        }
        
        internal BranchingRouteBuilder(BranchingRouteBuilder<TC> previous, Func<TC, bool> elseCondition, RouteBuilder<TC> elseRouter)
        {
            Return = previous.Return;
            firstCondition = previous.firstCondition;
            firstRouter = previous.firstRouter;
            elseBranches = previous.elseBranches.Add((elseCondition, elseRouter));
        }

        public BranchingRouteBuilder<TC> ElseIf(Func<TC, bool> condition, RouteBuilder<TC> innerRouteBuilder) =>
            new(this, condition, innerRouteBuilder);

        public RouteBuilder<TC> Else(RouteBuilder<TC> elseRouter)
        {
            var builder = new RouteBuilder<TC>(Return.textSelector);
            builder = builder.WithCommand(
                new ConditionRouteCommand<TC>(
                    RouteQueryResult.RUN_AND_EXIT,
                    firstCondition,
                    firstRouter));
            foreach (var (condition, router) in elseBranches)
                builder = builder.WithCommand(
                    new ConditionRouteCommand<TC>(
                        RouteQueryResult.RUN_AND_EXIT, 
                        condition, 
                        router));
            builder = builder.WithCommand(new SubrouterCommand<TC>(elseRouter));
            return builder;
        }

    }

    /// <summary>
    /// Handles the inner route if the predicate passes.
    /// Returns the constructor specified RouteQueryResult when the predicate passes.
    /// </summary>
    public class ConditionRouteCommand<TC> : IRouteCommand<TC>
    {
        private readonly Func<TC, bool> predicate;
        private readonly Router<TC> router;
        private readonly RouteQueryResult onPass;

        public ConditionRouteCommand(RouteQueryResult onPass, Func<TC, bool> predicate, RouteBuilder<TC> router) => 
            (this.onPass, this.predicate, this.router) = 
            (onPass, predicate, router.Build());

        public RouteQueryResult Query(TC context, ReadOnlySpan<char> text, out Func<TC, Task> action)
        {
            action = router.Handle;
            return predicate(context)
                ? onPass
                : RouteQueryResult.DO_NOT_RUN;
        }
    }

    public class SubrouterCommand<TC> : IRouteCommand<TC>
    {
        private readonly Router<TC> router;

        public SubrouterCommand(RouteBuilder<TC> router) => this.router = router.Build();

        public RouteQueryResult Query(TC context, ReadOnlySpan<char> text, out Func<TC, Task> action)
        {
            action = router.Handle;
            return RouteQueryResult.RUN_AND_EXIT;
        }
    }
}