﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using NewLibCore.Data.SQL.Mapper.Cache;
using NewLibCore.Data.SQL.Mapper.Database;
using NewLibCore.Data.SQL.Mapper.EntityExtension;
using NewLibCore.Data.SQL.Mapper.ExpressionStatment;
using NewLibCore.Logger;
using NewLibCore.Validate;

namespace NewLibCore.Data.SQL.Mapper
{
    /// <summary>
    /// 将对应的操作翻译为sql并执行
    /// </summary>
    public sealed class EntityMapper : IDisposable
    {
        private readonly IServiceScope _serviceScope;

        private EntityMapper()
        {
            var services = new ServiceCollection()
               .AddTransient<ResultCache, ExecutionResultCache>()
               .AddScoped<IMapperDbContext, MapperDbContext>()
               .AddSingleton<ILogger, ConsoleLogger>();

            if (MapperConfig.MapperType == MapperType.MSSQL)
            {
                services = services.AddScoped<InstanceConfig, MsSqlInstanceConfig>();
            }
            else if (MapperConfig.MapperType == MapperType.MYSQL)
            {
                services = services.AddScoped<InstanceConfig, MySqlInstanceConfig>();
            }
            var serviceProvider = services.BuildServiceProvider();
            _serviceScope = serviceProvider.CreateScope();
        }

        /// <summary>
        /// 创建一个实体映射对象
        /// </summary>
        /// <returns></returns>
        public static EntityMapper CreateMapper()
        {
            return new EntityMapper();
        }

        /// <summary>
        /// 添加一個TModel
        /// </summary>
        /// <param name="model">要新增的对象</param>
        /// <typeparam name="TModel"></typeparam>
        /// <returns></returns>
        public TModel Add<TModel>(TModel model) where TModel : EntityBase, new()
        {
            Parameter.Validate(model);

            return RunDiagnosis.Watch(() =>
            {
                Handler handler = new InsertHandler<TModel>(model, _serviceScope.ServiceProvider);
                model.Id = handler.Execute().FirstOrDefault<Int32>();
                return model;
            });
        }

        /// <summary>
        /// 修改一個TModel
        /// </summary>
        /// <param name="model">要修改的对象</param>
        /// <param name="expression">查询条件</param>
        /// <typeparam name="TModel"></typeparam>
        /// <returns></returns>
        public Boolean Update<TModel>(TModel model, Expression<Func<TModel, Boolean>> expression) where TModel : EntityBase, new()
        {
            Parameter.Validate(model);
            Parameter.Validate(expression);

            return RunDiagnosis.Watch(() =>
            {
                Handler handler = new UpdateHandler<TModel>(model, expression, _serviceScope.ServiceProvider);
                return handler.Execute().FirstOrDefault<Int32>() > 0;
            });
        }

        /// <summary>
        /// 查询一個TModel
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <returns></returns>
        public QueryWrapper<TModel> Query<TModel>() where TModel : new()
        {
            ExpressionStore expressionStore = new ExpressionStore();
            expressionStore.AddFrom<TModel>();
            return new QueryWrapper<TModel>(expressionStore, _serviceScope.ServiceProvider);
        }

        /// <summary>
        /// 执行一個返回列表的sql语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">实体参数</param>
        /// <typeparam name="TModel"></typeparam>
        /// <returns></returns>
        public RawResult SqlQuery(String sql, IEnumerable<EntityParameter> parameters = null)
        {
            Parameter.Validate(sql);

            return RunDiagnosis.Watch(() =>
            {
                Handler handler = new DirectSqlHandler(sql, parameters, _serviceScope.ServiceProvider);
                return handler.Execute();
            });
        }

        public void Commit()
        {
            _serviceScope.ServiceProvider.GetService<IMapperDbContext>().Commit();
        }

        public void Rollback()
        {
            _serviceScope.ServiceProvider.GetService<IMapperDbContext>().Rollback();
        }

        public void OpenTransaction()
        {
            _serviceScope.ServiceProvider.GetService<IMapperDbContext>().OpenTransaction();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _serviceScope.ServiceProvider.GetService<IMapperDbContext>().Dispose();
            _serviceScope.Dispose();
        }
    }
}
