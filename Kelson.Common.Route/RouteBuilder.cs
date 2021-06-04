using System;
using System.Threading.Tasks;

namespace Kelson.Common.Route
{
    using Args;

    public static class RouteBuilder
    {
        public static Func<TContext, Task> Route<TContext>(
            Func<TContext, string> textSelector,
            params IRouteCommand<TContext>[] commands) => async (context) =>
        {
            var text = textSelector(context);
            foreach (var command in commands)
                if (await command.Query(context, text)())
                    return;
        };

        public readonly struct ContextType<TC> { }

        public static CommandCondition<TC, T1> On<TC, T1>(ContextType<TC> _, TextArg<T1> arg) =>
            new(arg);

        public static CommandCondition<TC, T1, T2> On<TC, T1, T2>(ContextType<TC> _, TextArg<T1> arg1, TextArg<T2> arg2) =>
            new(arg1, arg2);

        public static CommandCondition<TC, T1, T2, T3> On<TC, T1, T2, T3>(ContextType<TC> _, TextArg<T1> arg1, TextArg<T2> arg2, TextArg<T3> arg3) =>
            new(arg1, arg2, arg3);

        public static CommandCondition<TC, T1, T2, T3, T4> On<TC, T1, T2, T3, T4>(ContextType<TC> _, TextArg<T1> arg1, TextArg<T2> arg2, TextArg<T3> arg3, TextArg<T4> arg4) =>
            new(arg1, arg2, arg3, arg4);

        public static CommandCondition<TC, T1, T2, T3, T4, T5> On<TC, T1, T2, T3, T4, T5>(ContextType<TC> _, TextArg<T1> arg1, TextArg<T2> arg2, TextArg<T3> arg3, TextArg<T4> arg4, TextArg<T5> arg5) =>
            new(arg1, arg2, arg3, arg4, arg5);

        public static CommandCondition<TC, T1, T2, T3, T4, T5, T6> On<TC, T1, T2, T3, T4, T5, T6>(ContextType<TC> _, TextArg<T1> arg1, TextArg<T2> arg2, TextArg<T3> arg3, TextArg<T4> arg4, TextArg<T5> arg5, TextArg<T6> arg6) =>
            new(arg1, arg2, arg3, arg4, arg5, arg6);

        public static ValueTask<bool> No() => new(false);

        public static Func<ValueTask<bool>> Yes(Func<Task> action) => async () =>
        {
            await action();
            return true;
        };
    }
}