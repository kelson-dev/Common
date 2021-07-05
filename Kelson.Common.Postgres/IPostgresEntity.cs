namespace Kelson.Common.Postgres
{
    public interface IPostgresEntity<TSelf, TId>
        where TSelf : IPostgresEntity<TSelf, TId>
    {
        TId Key();
        TSelf WithId(TId id);
    }
}

