﻿using System;
using NewLibCore.Data.SQL.Mapper.Extension;

namespace NewLibCore.Data.SQL.Mapper.Config
{
    public class MapperFactory
    {
        private MapperFactory() { }

        internal static Boolean ExpressionCache { get; private set; } = false;

        internal static Boolean StatementCache { get; private set; } = false;

        internal static ILogger Logger { get; private set; }

        internal static MapperInstance Instance { get; private set; }

        public static MapperFactory Factory { get; } = new MapperFactory();

        public MapperFactory InitLogger(ILogger logger = null)
        {
            Logger = logger ?? new ConsoleLogger();
            return this;
        }

        public MapperFactory SwitchToMySql()
        {
            SwitchTo(DatabaseType.MYSQL);
            return this;
        }

        public MapperFactory SwitchToMsSql()
        {
            SwitchTo(DatabaseType.MSSQL);
            return this;
        }

        public MapperFactory UseExpressionCache()
        {
            ExpressionCache = true;
            return this;
        }

        public MapperFactory UseStatementCache()
        {
            StatementCache = true;
            return this;
        }

        private static void SwitchTo(DatabaseType database)
        {
            switch (database)
            {
                case DatabaseType.MSSQL:
                {
                    Instance = new MsSqlInstance();
                    break;
                }
                case DatabaseType.MYSQL:
                {
                    Instance = new MySqlInstance();
                    break;
                }
                default:
                    break;
            }
        }
    }

}
