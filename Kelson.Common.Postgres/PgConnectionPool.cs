using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kelson.Common.Postgres
{
    public sealed class PgConnectionPool : IAsyncDisposable
    {
        private readonly string connectionString;

        private const int INITIAL_POOL_SIZE = 5;
        private readonly ConcurrentQueue<(DateTimeOffset, NpgsqlConnection)> queue = new();
        private bool disposed = false;

        private PgConnectionPool(string constr, NoticeEventHandler? onNoticeEvent, params NpgsqlConnection[] openConnections)
        {
            connectionString = constr;
            foreach (var con in openConnections)
            {
                if (onNoticeEvent != null)
                    con.Notice += onNoticeEvent;
                queue.Enqueue((DateTimeOffset.Now, con));
            }   
        }

        private static async Task<NpgsqlConnection> OpenNewConnection(string constr)
        {
            var connection = new NpgsqlConnection(constr);
            await connection.OpenAsync();
            return connection;
        }

        private static async Task<NpgsqlConnection[]> InitializePool(string constr)
        {
            var connections = new NpgsqlConnection[INITIAL_POOL_SIZE];

            for (int i = 0; i < INITIAL_POOL_SIZE; i++)
                connections[i] = await OpenNewConnection(constr);
            return connections;
        }

        public static async Task<PgConnectionPool> Create(string constr) => new(constr, null, await InitializePool(constr));

        public async Task<PgTx> StartTx()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(PgConnectionPool));
            async Task<PgTx> NewConTx(PgConnectionPool pool, NpgsqlConnection con)
            {
                if (disposed)
                    throw new ObjectDisposedException(nameof(PgConnectionPool));
                var newCon = await OpenNewConnection(pool.connectionString);
                var tx = newCon.BeginTransaction();
                return new PgTx(pool, DateTimeOffset.Now, newCon, tx);
            }

            if (queue.TryDequeue(out (DateTimeOffset opened, NpgsqlConnection con) result))
                return new PgTx(this, result.opened, result.con, result.con.BeginTransaction());
            else
            {
                await Task.Delay(30);
                if (queue.TryDequeue(out (DateTimeOffset opened, NpgsqlConnection con) found))
                    return new PgTx(this, result.opened, result.con, result.con.BeginTransaction());
                else
                    return await NewConTx(this, await OpenNewConnection(connectionString));
            }
        }

        public async ValueTask DisposeAsync()
        {
            disposed = true;
            foreach (var con in queue)
            {
                await con.Item2.CloseAsync();
            }
        }

        public sealed class PgTx : IAsyncDisposable
        {
            private readonly DateTimeOffset conOpened;
            private readonly PgConnectionPool pool;
            public readonly NpgsqlConnection Connection;
            public readonly NpgsqlTransaction Transaction;

            private readonly Queue<NpgsqlNoticeEventArgs> notices = new();
            private readonly Queue<NpgsqlNotificationEventArgs> notifications = new();

            public PgTx(PgConnectionPool pool, DateTimeOffset opened, NpgsqlConnection connection, NpgsqlTransaction tx)
            {
                this.pool = pool;
                this.conOpened = opened;
                this.Connection = connection;
                this.Connection.Notice += RecordNotices;
                this.Connection.Notification += RecordNotifications;
                this.Transaction = tx;
            }

            private void RecordNotices(object? item, NpgsqlNoticeEventArgs args) => notices.Enqueue(args);
            private void RecordNotifications(object? item, NpgsqlNotificationEventArgs args) => notifications.Enqueue(args);

            public async Task<T> ExecuteScalarAsync<T>(string sql)
            {
                var command = Connection.CreateCommand();
                command.CommandText = sql;
                command.Transaction = Transaction;
                var result = (await command.ExecuteScalarAsync(default))!;
                return (T)result;
            }

            public async Task<int> ExecuteNonQueryAsync(string sql)
            {
                var command = Connection.CreateCommand();
                command.CommandText = sql;
                command.Transaction = Transaction;
                var rows = await command.ExecuteNonQueryAsync();
                return rows;
            }

            public async ValueTask DisposeAsync()
            {
                try
                {
                    await Transaction.CommitAsync();
                    await Transaction.DisposeAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    throw;
                }
                finally
                {
                    Connection.Notice -= RecordNotices;
                    Connection.Notification -= RecordNotifications;
                    if (conOpened.AddHours(1) > DateTimeOffset.Now)
                    {
                        if (pool.disposed)
                            await Connection.CloseAsync();
                        else
                            pool.queue.Enqueue((conOpened, Connection));
                    }
                    else
                    {
                        try
                        {
                            await Connection.CloseAsync();
                        }
                        finally
                        {
                            if (!pool.disposed)
                                pool.queue.Enqueue((DateTimeOffset.Now, await OpenNewConnection(pool.connectionString)));
                        }
                    }
                }
            }
        }
    }
}
