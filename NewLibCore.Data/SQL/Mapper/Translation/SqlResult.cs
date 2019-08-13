﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NewLibCore.Data.SQL.Mapper.Config;
using NewLibCore.Data.SQL.Mapper.Database;
using NewLibCore.Data.SQL.Mapper.EntityExtension;
using NewLibCore.Validate;

namespace NewLibCore.Data.SQL.Mapper
{
    /// <summary>
    /// 存储表达式的翻译后的sql语句
    /// </summary>
    internal sealed class SqlResult
    {
        private readonly StringBuilder _originSql;
        private readonly IList<EntityParameter> _parameters;

        private SqlResult()
        {
            _originSql = new StringBuilder();
            _parameters = new List<EntityParameter>();
        }

        internal static SqlResult CreateSqlResult()
        {
            return new SqlResult();
        }

        internal ExecuteType ExecuteType { get; set; }

        /// <summary>
        /// 获取EntityParameter列表
        /// </summary>
        /// <returns></returns>
        internal IList<EntityParameter> GetParameters()
        {
            return _parameters;
        }

        /// <summary>
        /// 追加一个sql语句和一组EntityParameter对象
        /// </summary>
        /// <param name="entityParameters"></param>
        internal SqlResult Append(String sql, IEnumerable<EntityParameter> entityParameters = null)
        {
            Parameter.Validate(sql);

            _originSql.Append($@" {sql} ");
            if (entityParameters != null)
            {
                foreach (var item in entityParameters)
                {
                    _parameters.Add(item);
                }
            }

            return this;
        }

        /// <summary>
        /// 追加一组EntityParameter对象
        /// </summary>
        /// <param name="entityParameters"></param>
        internal void Append(params EntityParameter[] entityParameters)
        {
            Append(entityParameters.ToList());
        }

        /// <summary>
        /// 追加一组EntityParameter对象
        /// </summary>
        /// <param name="entityParameters"></param>
        internal void Append(IEnumerable<EntityParameter> entityParameters)
        {
            if (entityParameters != null)
            {
                foreach (var item in entityParameters)
                {
                    _parameters.Add(item);
                }
            }
        }

        /// <summary>
        /// 获取表达式段执行之后的结果
        /// </summary>
        /// <returns></returns>
        internal RawExecuteResult GetExecuteResult()
        {
            return Execute();
        }

        internal void Clear()
        {
            _originSql.Clear();
            _parameters.Clear();
        }

        private RawExecuteResult Execute()
        {
            if (ExecuteType == default)
            {
                var rawSql = ToString();
                Enum.TryParse<ExecuteType>(rawSql.Substring(0, rawSql.IndexOf(" ")).ToUpper(), out var executeType);
                ExecuteType = executeType;

                if (rawSql.Contains("COUNT(*)"))
                {
                    ExecuteType = ExecuteType.SELECT_SINGLE;
                }
            }

            var executeResult = GetCache();
            if (executeResult == null)
            {
                executeResult = ExecutionCore.CreateExecutionCore().Execute(this);
                SetCache(executeResult);
            }

            return executeResult;
        }

        /// <summary>
        /// 获取作为要缓存的sql语句的key
        /// </summary>
        /// <param name="entityParameters"></param>
        private String PrepareCacheKey()
        {
            Parameter.Validate(_originSql);
            var cacheKey = ToString();
            foreach (var item in GetParameters())
            {
                cacheKey = cacheKey.Replace(item.Key, item.Value.ToString());
            }
            return MD.GetMD5(cacheKey);
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="executeResult"></param>
        private void SetCache(RawExecuteResult executeResult)
        {
            if (MapperConfig.Instance.Cache != null)
            {
                MapperConfig.Instance.Cache.Add(PrepareCacheKey(), executeResult);
            }
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <returns></returns>
        private RawExecuteResult GetCache()
        {
            if (MapperConfig.Instance.Cache != null)
            {
                var cacheResult = MapperConfig.Instance.Cache.Get(PrepareCacheKey());
                if (cacheResult != null)
                {
                    return (RawExecuteResult)cacheResult;
                }
            }
            return default;
        }

        /// <summary>
        /// 将sql语句中多余的空格去掉
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private String ReformatSql(String sql)
        {
            Parameter.Validate(sql);
            sql = sql.Replace("   ", " ").Replace("  ", " ");
            return sql.Trim();
        }

        public override String ToString()
        {
            return ReformatSql(_originSql.ToString());
        }
    }
}
