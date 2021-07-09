using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Kelson.Common.Postgres
{
    public sealed class PgConnectionPool : IAsyncDisposable
    {
        private readonly string connectionString;

        private const int INITIAL_POOL_SIZE = 1;
        private readonly ConcurrentQueue<(DateTimeOffset, NpgsqlConnection)> queue = new();
        private bool disposed = false;
        private ulong txId = 0UL;

        public event Action<string, ulong>? OnTxStarted;
        public event Action<string, ulong>? OnTxCompleted;
        public event Action<int>? OnPoolExpanded;
        public event Action<string, ulong, TimeSpan>? OnTxDiagnostic;
        public event Action<string, ulong, Exception>? OnTxError;
        public bool DiagnosticsEnabled { get; set; } = false;


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

        private static async Task<NpgsqlConnection[]> InitializePool(string constr, int intialPoolSize = INITIAL_POOL_SIZE)
        {
            var connections = new NpgsqlConnection[intialPoolSize];

            for (int i = 0; i < connections.Length; i++)
                connections[i] = await OpenNewConnection(constr);
            return connections;
        }

        public static async Task<PgConnectionPool> Create(string constr, int initialPoolSize = INITIAL_POOL_SIZE) => new(constr, null, await InitializePool(constr, initialPoolSize));

        public async Task<PgTx> StartTx([CallerMemberName] string? callsight = null)
        {
            ulong id = Interlocked.Increment(ref txId);
            if (disposed)
                throw new ObjectDisposedException(nameof(PgConnectionPool));
            async Task<PgTx> NewConTx(PgConnectionPool pool, NpgsqlConnection con)
            {
                if (disposed)
                    throw new ObjectDisposedException(nameof(PgConnectionPool));
                var newCon = await OpenNewConnection(pool.connectionString);
                OnPoolExpanded?.Invoke(queue.Count);
                return new PgTx(pool, DateTimeOffset.Now, newCon, id, callsight!, DiagnosticsEnabled);
            }

            if (queue.TryDequeue(out (DateTimeOffset opened, NpgsqlConnection con) result))
                return new PgTx(this, result.opened, result.con, id, callsight!, DiagnosticsEnabled);
            else
            {
                await Task.Delay(30);
                if (queue.TryDequeue(out (DateTimeOffset opened, NpgsqlConnection con) found))
                    return new PgTx(this, result.opened, result.con, id, callsight!, DiagnosticsEnabled);
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
            private readonly ulong id;
            private readonly string callsight;
            private readonly Stopwatch? stopwatch;

            private readonly Queue<NpgsqlNoticeEventArgs> notices = new();
            private readonly Queue<NpgsqlNotificationEventArgs> notifications = new();

            public PgTx(PgConnectionPool pool, DateTimeOffset opened, NpgsqlConnection connection, ulong id, string callsight, bool withDiagnostics)
            {
                this.pool = pool;
                this.conOpened = opened;
                this.Connection = connection;
                this.Connection.Notice += RecordNotices;
                this.Connection.Notification += RecordNotifications;
                this.Transaction = this.Connection.BeginTransaction(IsolationLevel.Snapshot);
                this.callsight = callsight;
                this.id = id;
                pool.OnTxStarted?.Invoke(callsight, id);
                if (withDiagnostics)
                {
                    stopwatch = new();
                    stopwatch.Start();
                }
            }

            private void RecordNotices(object? item, NpgsqlNoticeEventArgs args) => notices.Enqueue(args);
            private void RecordNotifications(object? item, NpgsqlNotificationEventArgs args) => notifications.Enqueue(args);

            public async ValueTask DisposeAsync()
            {
                try
                {
                    await Transaction.CommitAsync();
                    await Transaction.DisposeAsync();
                }
                catch (Exception e)
                {
                    pool.OnTxError?.Invoke(callsight, id, e);
                    throw;
                }
                finally
                {
                    Connection.Notice -= RecordNotices;
                    Connection.Notification -= RecordNotifications;
                    pool.OnTxCompleted?.Invoke(callsight, id);
                    if (stopwatch is not null)
                    {
                        stopwatch.Stop();
                        pool.OnTxDiagnostic?.Invoke(callsight, id, stopwatch.Elapsed);
                    }
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
