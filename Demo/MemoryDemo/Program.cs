using Kelson.Common.Memory;
using System;

unsafe
{
    using (var @ref = new ExclusiveRef<string>(&Create))
    {
        @ref.Borrow(&AssignHolderValue);
        @ref.Borrow(&WriteString);
    }
    System.Console.WriteLine(Holder.Value);
}

static string AppendA(in string t) => t + 'a';
static string AssignHolderValue(in string t) => Holder.Value = t;
static string Create() => Guid.NewGuid().ToString();
static void WriteString(in string t) => Console.WriteLine(t);

public static class Holder
{
    public static string Value;
}
