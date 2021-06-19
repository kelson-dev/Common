using System;
using System.Collections.Generic;
using System.Linq;

namespace Kelson.Common.Route.Args
{
    public class DelegateCommandArgument<TC, T> : TextArg<TC, T>
    {
        private readonly TextArgMatchDelegate<TC, T> matcher;

        private string _syntax = $"<custom delegate>";
        public override string Syntax => _syntax;

        public DelegateCommandArgument(TextArgMatchDelegate<TC, T> matcher, string description = null) => 
            (this.matcher, _description) = (matcher, description ?? "[Descrpition for this parameter has not been configured]");

        public override bool Matches(TC context, ref ReadOnlySpan<char> text, out T result) => matcher(context, ref text, out result);

        private readonly string _description;
        public override string Description => _description;
        public string[] ExampleStrings { private get;  set; }
        public override IEnumerable<string> Examples() => ExampleStrings.AsEnumerable();
    }
}
