using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kelson.Common.Route.Web.Args
{
    public delegate bool WebArgMatchDelegate<T>(HttpContext context, out T result);

    public abstract class WebArg<T>
    {
        public T Value = default;

        public abstract bool Matches(
            HttpContext context,
            out T result);

        //public static WebArg<T> operator &(string text, WebArg<T> arg) =>
        //    new CompositeCommandArgument<string, T>(
        //        new TextCommandArgument(text),
        //        arg);

        //public static WebArg<T> operator &(TextArg<Unit> condition, WebArg<T> arg) =>
        //    new CompositeCommandArgument<Unit, T>(
        //        condition,
        //        arg);

        //public static WebArg<T> operator |(WebArg<T> arg1, WebArg<T> arg2) =>
        //    new EitherCommandArguement<T>(arg1, arg2);

        //public static implicit operator WebArg<T>(WebArgMatchDelegate<T> matcher) => 
        //    new DelegateCommandArgument<T>(matcher);
    }
}
