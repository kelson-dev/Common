namespace Kelson.Common.Route.Options
{
    public interface IOptionsModel<TOptions>
        where TOptions : IOptionsModel<TOptions>
    {
        FlagDescriptor<TOptions>[] Setters { get; }
    }
}
