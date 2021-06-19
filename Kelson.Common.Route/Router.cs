using System;
using System.Threading.Tasks;

namespace Kelson.Common.Route
{
    using Args;
    using Kelson.Common.Route.Docs;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    public class Router<TC>
    {
        internal readonly Func<TC, string> textSelector;
        internal readonly RouteCommand<TC>[] commands;
        private readonly string? name;
        private readonly string? description;

        internal Router(Func<TC, string> textSelector, RouteCommand<TC>[] commands, string? name, string? description)
        {
            this.textSelector = textSelector;
            this.commands = commands;
            this.name = name;
            this.description = description;
        }

        public async Task Handle(TC context, int start = 0)
        {
            var text = textSelector(context);
            text = text[start..];
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

        public RouteDoc BuildDocs() => new(
            name,
            description,
            "",
            Array.Empty<string>(),
            commands.Select(c => c.BuildDoc()).ToArray());
        
    }

    public class RouteBuilder<TC>
    {
        internal readonly Func<TC, string> textSelector;
        internal readonly ImmutableList<RouteCommand<TC>> commands;

        private string? name;
        private string? description;

        public RouteBuilder(Func<TC, string> textSelector)
        {
            this.textSelector = textSelector;
            this.commands = ImmutableList<RouteCommand<TC>>.Empty;
        }

        private RouteBuilder(Func<TC, string> textSelector, ImmutableList<RouteCommand<TC>> commands)
        {
            this.textSelector = textSelector;
            this.commands = commands;
        }

        public RouteBuilder<TC> Named(string name)
        {
            this.name = name;
            return this;
        }

        public RouteBuilder<TC> DescribedAs(string desc)
        {
            this.description = desc;
            return this;
        }


        public Router<TC> Build() => new(textSelector, commands.ToArray(), name, description);

        public RouteBuilder<TC> WithCommand(RouteCommand<TC> command) => new RouteBuilder<TC>(textSelector, commands.Add(command))
                .Named(name)
                .DescribedAs(description);

        /// <summary>
        /// If the condition predicate passes, the inner route builder will be executed before the outer handler continues execution.
        /// An aside will not shortcircuit the routing.
        /// </summary>
        public RouteBuilder<TC> Aside(Func<TC, bool> condition, RouteBuilder<TC> route, string? name = null, string? description = null) =>
            WithCommand(
                new ConditionRouteCommand<TC>(
                    RouteQueryResult.RUN_AND_CONTIUE,
                    new PredicateCommandArgument<TC>(condition),
                    route.Build(),
                    name ?? route.name,
                    description ?? route.description));

        public IfBranchRouteSelector<TC> If(Func<TC, bool> condition) => new(this, condition);

        public IfBranchRouteSelector<TC> If(TextArg<TC> condition) => new(this, condition);


        public Condition<TC> When(TextArg<TC> condition) =>
            new(this, condition);

        /// <summary>
        /// If the condition argument is a match the inner operation will be executed before the outer handler continues execution.
        /// An aside will not shortcircuit the routing.
        /// </summary>
        public Condition<TC> Aside(TextArg<TC> condition) =>
            new(this, condition, RouteQueryResult.RUN_AND_CONTIUE);

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

    public record IfBranchRouteSelector<TC>(RouteBuilder<TC> Parent, TextArg<TC> Condition)
    {
        public IfBranchRouteSelector(RouteBuilder<TC> parent, Func<TC, bool> condition) : this(parent, new PredicateCommandArgument<TC>(condition)){}

        public IfBranchDocSelector<TC> Route(RouteBuilder<TC> route) =>
            new(Parent, Condition, route.Build());
    }

    public record IfBranchDocSelector<TC>(
        RouteBuilder<TC> Parent,
        TextArg<TC> Condition, 
        Router<TC> Router, 
        string? Name = null, 
        string? Description = null, 
        string[]? Examples = null)
    {
        public ElseIfBranchRouteSelector<TC> ElseIf(Func<TC, bool> condition) => new(Parent, this, null, new PredicateCommandArgument<TC>(condition));

        public ElseBranchDocSelector<TC> Else(RouteBuilder<TC> router) => new(Parent, this, null, router.Build());

        public RouteBuilder<TC> End()
        {
            var builder = Parent;
            builder = builder.WithCommand(
                new ConditionRouteCommand<TC>(
                    RouteQueryResult.RUN_AND_EXIT,
                    Condition,
                    Router,
                    Name,
                    Description));

            return builder;
        }

        public IfBranchDocSelector<TC> Named(string name) => this with { Name = name };
        public IfBranchDocSelector<TC> DescribedAs(string desc) => this with { Description = desc };
        public IfBranchDocSelector<TC> WithExamples(params string[] examples) => this with { Examples = examples };
    }

    public record ElseIfBranchRouteSelector<TC>(
        RouteBuilder<TC> Parent,
        IfBranchDocSelector<TC> First,
        ElseIfBranchDocSelector<TC>? Previous,
        TextArg<TC> Condition)
    {
        public ElseIfBranchDocSelector<TC> Route(RouteBuilder<TC> router) => new(Parent, First, Previous, Condition, router.Build());
    };

    public record ElseIfBranchDocSelector<TC>(
        RouteBuilder<TC> Parent,
        IfBranchDocSelector<TC> First,
        ElseIfBranchDocSelector<TC>? Previous,
        TextArg<TC> Condition,
        Router<TC> Router,
        string? Name = null,
        string? Description = null,
        string[]? Examples = null)
    {
        public ElseIfBranchRouteSelector<TC> ElseIf(Func<TC, bool> condition) => new(Parent, First, this, new PredicateCommandArgument<TC>(condition));

        public ElseBranchDocSelector<TC> Else(RouteBuilder<TC> router) => new(Parent, First, this, router.Build());

        public RouteBuilder<TC> End()
        {
            var builder = Parent;
            builder = builder.WithCommand(
                new ConditionRouteCommand<TC>(
                    RouteQueryResult.RUN_AND_EXIT,
                    First.Condition,
                    First.Router,
                    First.Name,
                    First.Description));

            static void AddElseIfBranch(ElseIfBranchDocSelector<TC>? selector, ref RouteBuilder<TC> builder)
            {
                if (selector == null)
                    return;
                AddElseIfBranch(selector.Previous, ref builder);
                builder = builder.WithCommand(
                    new ConditionRouteCommand<TC>(
                        RouteQueryResult.RUN_AND_EXIT,
                        selector.Condition,
                        selector.Router,
                        selector.Name,
                        selector.Description));
            }

            AddElseIfBranch(Previous, ref builder);

            return builder;
        }

        public ElseIfBranchDocSelector<TC> Named(string name) => this with { Name = name };
        public ElseIfBranchDocSelector<TC> DescribedAs(string desc) => this with { Description = desc };
        public ElseIfBranchDocSelector<TC> WithExamples(params string[] examples) => this with { Examples = examples };
    };

    public record ElseBranchDocSelector<TC>(
        RouteBuilder<TC> Parent,
        IfBranchDocSelector<TC> First, 
        ElseIfBranchDocSelector<TC>? Previous, 
        Router<TC> Router, 
        string? Name = null, 
        string? Description = null, 
        string[]? Examples = null)
    {
        public RouteBuilder<TC> End()
        {
            var builder = Parent;
            builder = builder.WithCommand(
                new ConditionRouteCommand<TC>(
                    RouteQueryResult.RUN_AND_EXIT,
                    First.Condition,
                    First.Router,
                    First.Name,
                    First.Description));

            static void AddElseIfBranch(ElseIfBranchDocSelector<TC>? selector, ref RouteBuilder<TC> builder)
            {
                if (selector == null)
                    return;
                AddElseIfBranch(selector.Previous, ref builder);
                builder = builder.WithCommand(
                    new ConditionRouteCommand<TC>(
                        RouteQueryResult.RUN_AND_EXIT,
                        selector.Condition,
                        selector.Router,
                        selector.Name,
                        selector.Description));
            }

            AddElseIfBranch(Previous, ref builder);

            builder = builder.WithCommand(new SubrouterCommand<TC>(Router, Name, Description));
            return builder;
        }

        public ElseBranchDocSelector<TC> Named(string name) => this with { Name = name };
        public ElseBranchDocSelector<TC> DescribedAs(string desc) => this with { Description = desc };
        public ElseBranchDocSelector<TC> WithExamples(params string[] examples) => this with { Examples = examples };
    };

    /// <summary>
    /// Handles the inner route if the predicate passes.
    /// Returns the constructor specified RouteQueryResult when the predicate passes.
    /// </summary>
    public class ConditionRouteCommand<TC> : RouteCommand<TC>
    {
        private readonly TextArg<TC> predicate;
        private readonly Router<TC> router;
        private readonly RouteQueryResult onPass;
        private readonly List<string> _examples = new();

        public ConditionRouteCommand(RouteQueryResult onPass, TextArg<TC> predicate, Router<TC> router, string? name, string? description) => 
            (this.onPass, this.predicate, this.router, Name, Description) = 
            (onPass, predicate, router, name, description);

        public override IEnumerable<string> Examples()
        {
            yield break;
        }

        public override RouteQueryResult Query(TC context, ReadOnlySpan<char> text, out Func<TC, Task> action)
        {
            
            var initial = text;
            var result = predicate.Matches(context, ref text, out Unit _)
                ? onPass
                : RouteQueryResult.DO_NOT_RUN;
            var skipped = initial.Length - text.Length;
            action = skipped == 0
                ? (c) => router.Handle(c)
                : (c) => router.Handle(c, skipped);
            return result;
        }

        public override RouteDoc BuildDoc()
        {
            var inner = router.BuildDocs();
            return new RouteDoc(
                Name ?? inner.Name,
                Description ?? inner.Description,
                AutoArgSyntax(),
                inner.Examples,
                inner.Subcommands);
        }

        public override IEnumerable<ITextArg> Args()
        {
            yield break;
        }

        
    }

    public class SubrouterCommand<TC> : RouteCommand<TC>
    {
        private readonly Router<TC> router;

        public SubrouterCommand(Router<TC> router, string? name, string? description) => 
            (this.router, Name, Description) = (router, name, description);

        public override IEnumerable<string> Examples()
        {
            yield break;
        }

        public override RouteDoc BuildDoc()
        {
            var inner = router.BuildDocs();
            return new RouteDoc(
                Name ?? inner.Name,
                Description ?? inner.Description,
                AutoArgSyntax(),
                inner.Examples,
                inner.Subcommands);
        }

        public override IEnumerable<ITextArg> Args()
        {
            yield break;
        }

        public override RouteQueryResult Query(TC context, ReadOnlySpan<char> text, out Func<TC, Task> action)
        {
            action = (c) => router.Handle(c);
            return RouteQueryResult.RUN_AND_EXIT;
        }
    }
}