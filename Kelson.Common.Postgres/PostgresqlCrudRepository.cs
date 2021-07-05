using Dapper;
using System.Linq;
using System.Threading.Tasks;
using static Kelson.Common.Postgres.PgConnectionPool;

namespace Kelson.Common.Postgres
{
    public interface IReadUpdateDeleteRepo<T>
    {
        Task<T> CreateAsync(PgTx tx, T item);
        Task<T> UpdateAsync(PgTx tx, T item);
        Task<bool> DeleteAsync(PgTx tx, T item);
    }

    public abstract class PostgresqlCrudRepository<T, TId> : IReadUpdateDeleteRepo<T>
        where T : IPostgresEntity<T, TId>, new()
    {
        protected virtual T Default() => new();
        protected T WithId(TId id, T item) => item.WithId(id);
        protected T Item { get; }

        public abstract DapperQuerySet Sql { get; }

        protected PostgresqlCrudRepository()
        {
            Item = Default();
        }

        public async Task<T> GetByIdAsync(PgConnectionPool pool, TId id)
        {
            await using var tx = await pool.StartTx();
            return await GetByIdAsync(tx, id);
        }

        public virtual Task<T> GetByIdAsync(PgTx tx, TId id) => 
            tx.Connection.QuerySingleAsync<T>(
                Sql.FullSelectByIdQuery, 
                param: WithId(id, Item), 
                transaction: tx.Transaction);
        
        public async Task<(bool found, T item)> TryGetByIdAsync(PgConnectionPool pool, TId id)
        {
            await using var tx = await pool.StartTx();
            return await TryGetByIdAsync(tx, id);
        }

        public virtual async Task<(bool found, T item)> TryGetByIdAsync(PgTx tx, TId id)
        {
            var queried = await tx.Connection.QueryAsync<T>(
                Sql.FullSelectByIdQuery,
                param: WithId(id, Item),
                transaction: tx.Transaction);
            var collected = queried.ToArray();
            bool found = collected.Length == 1;
            return (found, found ? collected[0] : Item);
        }

        public async Task<T> CreateAsync(PgConnectionPool pool, T item)
        {
            await using var tx = await pool.StartTx();
            return await CreateAsync(tx, item);
        }

        public Task<T> CreateAsync(PgTx tx, T item) => 
            tx.Connection.QuerySingleAsync<T>(Sql.FullInsertAndReturnCommand, param: item, transaction: tx.Transaction);
        
        public async Task<T> UpdateAsync(PgConnectionPool pool, T item)
        {
            await using var tx = await pool.StartTx();
            return await UpdateAsync(tx, item);
        }

        public Task<T> UpdateAsync(PgTx tx, T item) =>
            tx.Connection.QuerySingleAsync<T>(Sql.FullUpdateAndReturnCommand, param: item, transaction: tx.Transaction);

        public async Task<bool> DeleteAsync(PgConnectionPool pool, T item)
        {
            await using var tx = await pool.StartTx();
            return await DeleteAsync(tx, item);
        }

        public async Task<bool> DeleteAsync(PgTx tx, T item)
        {
            var rows = await tx.Connection.ExecuteAsync(Sql.FullDeleteById, param: item, transaction: tx.Transaction);
            if (rows > 1)
                await tx.Transaction.RollbackAsync();
            return rows == 1;
        }
            
    }
}
