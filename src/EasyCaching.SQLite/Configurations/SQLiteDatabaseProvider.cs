﻿namespace EasyCaching.SQLite
{
    using EasyCaching.Core;
    using Microsoft.Data.Sqlite;
    using System.Collections.Concurrent;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// SQLite database provider.
    /// </summary>
    public class SQLiteDatabaseProvider : ISQLiteDatabaseProvider
    {
        /// <summary>
        /// The options.
        /// </summary>
        private readonly SQLiteDBOptions _options;
        
        public SQLiteDatabaseProvider(string name , SQLiteOptions options)
        {
            _name = name;
            _options = options.DBConfig;
            _builder = new SqliteConnectionStringBuilder
            {
                DataSource = _options.DataSource,
                Mode = _options.OpenMode,
                Cache = _options.CacheMode
            };

            _conns = new ConcurrentDictionary<int, SqliteConnection>();
        }

        private static ConcurrentDictionary<int, SqliteConnection> _conns;

        /// <summary>
        /// The builder
        /// </summary>
        private static SqliteConnectionStringBuilder _builder;

        private readonly string _name = EasyCachingConstValue.DefaultSQLiteName;

        public string DBProviderName => _name;

        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <returns>The connection.</returns>
        public SqliteConnection GetConnection()
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            var con = _conns.GetOrAdd(threadId, CreateNewConnection());

            Task.Run(async () =>
            {
                await Task.Delay(5000).ConfigureAwait(false);
                _conns.TryRemove(threadId, out var removingConn);
                if (removingConn?.State == ConnectionState.Closed)
                {
                    removingConn.Dispose();
                }
            });

            return con;
        }

        private SqliteConnection CreateNewConnection()
        {
            return new SqliteConnection(_builder.ToString());
        }
    }
}
