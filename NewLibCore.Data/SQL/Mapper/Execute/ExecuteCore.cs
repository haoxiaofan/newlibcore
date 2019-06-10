﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using NewLibCore.Data.SQL.Mapper.Config;
using NewLibCore.Data.SQL.Mapper.Extension;
using NewLibCore.Data.SQL.Mapper.Translation;
using NewLibCore.Validate;

namespace NewLibCore.Data.SQL.Mapper.Execute
{
    public sealed class ExecuteCore : IDisposable
    {
        private DbConnection _connection;

        private DbTransaction _dataTransaction;

        private Boolean _disposed = false;

        private Boolean _useTransaction = false;

        internal ExecuteCore()
        {
            Parameter.Validate(MapperFactory.Instance);
            _connection = MapperFactory.Instance.GetConnectionInstance();
        }

        internal void OpenTransaction()
        {
            MapperFactory.Instance.Logger.Write("INFO", "open transaction");
            _useTransaction = true;
        }

        internal void Commit()
        {
            if (_useTransaction)
            {
                if (_dataTransaction != null)
                {
                    _dataTransaction.Commit();
                    MapperFactory.Instance.Logger.Write("INFO", "commit transaction");
                }
                return;
            }
            throw new Exception("没有启动事务，无法执行事务提交");
        }

        internal void Rollback()
        {
            if (_useTransaction)
            {
                if (_dataTransaction != null)
                {
                    _dataTransaction.Rollback();
                    MapperFactory.Instance.Logger.Write("INFO", "rollback transaction ");
                }
                return;
            }
            throw new Exception("没有启动事务，无法执行事务回滚");
        }

        internal ExecuteCoreResult Execute(ExecuteType executeType, TranslationCoreResult translationCore)
        {
            Parameter.Validate(translationCore);
            return Execute(executeType, translationCore.GetSql(), translationCore.GetParameters(), CommandType.Text);
        }

        internal ExecuteCoreResult Execute(ExecuteType executeType, String sql, IEnumerable<EntityParameter> parameters = null, CommandType commandType = CommandType.Text)
        {
            try
            {
                Parameter.Validate(sql);

                Open();
                using (var cmd = _connection.CreateCommand())
                {
                    if (_useTransaction)
                    {
                        cmd.Transaction = BeginTransaction();
                    }
                    cmd.CommandType = commandType;
                    cmd.CommandText = sql;
                    if (parameters != null && parameters.Any())
                    {
                        cmd.Parameters.AddRange(parameters.Select(s => (DbParameter)s).ToArray());
                    }
                    MapperFactory.Instance.Logger.Write("INFO", $@"ExecuteType:{executeType}");
                    MapperFactory.Instance.Logger.Write("INFO", $@"SQL:{sql}");
                    MapperFactory.Instance.Logger.Write("INFO", $@"PARAMETERS:{(parameters == null || !parameters.Any() ? "" : String.Join($@"{Environment.NewLine}", parameters.Select(s => $@"{s.Key}----{s.Value}")))}");
                    var executeResult = new ExecuteCoreResult();
                    if (executeType == ExecuteType.SELECT)
                    {
                        using (var dr = cmd.ExecuteReader())
                        {
                            var dataTable = new DataTable("tmpDt");
                            dataTable.Load(dr, LoadOption.Upsert);
                            executeResult.Value = dataTable;
                        }
                    }
                    else if (executeType == ExecuteType.UPDATE)
                    {
                        executeResult.Value = cmd.ExecuteNonQuery();
                    }
                    else if (executeType == ExecuteType.INSERT || executeType == ExecuteType.SELECT_SINGLE)
                    {
                        executeResult.Value = cmd.ExecuteScalar();
                    }
                    cmd.Parameters.Clear();

                    return executeResult;
                }
            }
            catch (Exception ex)
            {
                MapperFactory.Instance.Logger.Write("ERROR", $@"{ex}");
                throw;
            }
        }

        private void Open()
        {
            if (_connection.State == ConnectionState.Closed)
            {
                MapperFactory.Instance.Logger.Write("INFO", "open connection");
                _connection.Open();
            }
        }

        private DbTransaction BeginTransaction()
        {
            if (_useTransaction)
            {
                if (_dataTransaction == null)
                {
                    _useTransaction = true;
                    _dataTransaction = _connection.BeginTransaction();
                    MapperFactory.Instance.Logger.Write("INFO", "begin transaction");
                }
                return _dataTransaction;
            }
            throw new Exception("没有启动事务");
        }

        #region dispose

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(Boolean disposing)
        {
            MapperFactory.Instance.Logger.Write("INFO", $@"close connection {Environment.NewLine}");
            if (!_disposed)
            {
                if (!disposing)
                {
                    return;
                }

                if (_connection != null)
                {
                    if (_connection.State != ConnectionState.Closed)
                    {
                        _connection.Close();
                    }
                    _connection.Dispose();
                    _connection = null;
                }
                _disposed = true;
            }
        }

        #endregion
    }

}
