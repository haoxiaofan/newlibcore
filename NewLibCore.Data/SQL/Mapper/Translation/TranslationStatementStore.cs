﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NewLibCore.Data.SQL.Mapper.Extension;
using NewLibCore.Validate;

namespace NewLibCore.Data.SQL.Mapper.Translation
{
    internal abstract class Statement
    {
        protected internal Expression Expression { get; set; }

        protected internal IList<KeyValuePair<String, String>> AliaNameMapper { get; set; }
    }

    internal class JoinStatement : Statement
    {
        protected internal String MainTable { get; set; }

        protected internal JoinType JoinType { get; set; }
    }

    internal class OrderStatement : Statement
    {
        protected internal OrderByType OrderBy { get; set; }
    }

    internal class SimpleStatement : Statement
    {

    }

    internal class PageStatement : Statement
    {
        internal Int32 Index { get; set; }

        internal Int32 Size { get; set; }
    }

    internal class TranslationStatementStore
    {
        internal ExecuteType ExecuteType { get; set; }

        internal OrderStatement Order { get; private set; }

        internal SimpleStatement Field { get; private set; }

        internal SimpleStatement Where { get; private set; }

        internal PageStatement Page { get; private set; }

        internal IList<JoinStatement> Joins { get; private set; } = new List<JoinStatement>();

        internal void AddOrderBy<TModel, TKey>(Expression<Func<TModel, TKey>> order, OrderByType orderByType)
        {
            Parameter.Validate(order);
            Order = new OrderStatement
            {
                Expression = order,
                OrderBy = orderByType
            };
        }

        internal void Add<TModel, TJoin>(Expression<Func<TModel, TJoin, Boolean>> expression, JoinType joinType = JoinType.NONE) where TModel : PropertyMonitor, new() where TJoin : PropertyMonitor, new()
        {
            Parameter.Validate(expression);
            var parameters = expression.Parameters.Select(s => new KeyValuePair<String, String>(s.Name, s.Type.GetAliasName())).ToList();
            Joins.Add(new JoinStatement
            {
                Expression = expression,
                JoinType = joinType,
                AliaNameMapper = parameters,
                MainTable = typeof(TModel).GetAliasName()
            });
        }

        internal void Add<TModel>(Expression<Func<TModel, Boolean>> expression) where TModel : PropertyMonitor, new()
        {
            Parameter.Validate(expression);
            Where = new SimpleStatement
            {
                Expression = expression,
                AliaNameMapper = expression.Parameters.Select(s => new KeyValuePair<String, String>(s.Name, s.Type.GetAliasName())).ToList()
            };
        }

        internal void Add<TModel>(Expression<Func<TModel, dynamic>> expression) where TModel : PropertyMonitor, new()
        {
            Parameter.Validate(expression);
            Field = new SimpleStatement
            {
                Expression = expression
            };
        }

        internal void Add<TModel, T>(Expression<Func<TModel, T, dynamic>> expression) where TModel : PropertyMonitor, new()
        where T : PropertyMonitor, new()
        {
            Parameter.Validate(expression);
            Field = new SimpleStatement
            {
                Expression = expression
            };
        }

        internal void AddPage(Int32 pageIndex, Int32 pageSize)
        {
            Parameter.Validate(pageIndex);
            Parameter.Validate(pageSize);
            Page = new PageStatement
            {
                Index = pageIndex,
                Size = pageSize
            };
        }

        internal List<KeyValuePair<String, String>> MergeAliasMapper()
        {
            var newAliasMapper = new List<KeyValuePair<String, String>>();
            if (Where != null)
            {
                newAliasMapper.AddRange(Where.AliaNameMapper);
            }

            if (Joins.Any())
            {
                newAliasMapper.AddRange(Joins.SelectMany(s => s.AliaNameMapper));
            }
            newAliasMapper = newAliasMapper.Select(s => s).Distinct().ToList();
            return newAliasMapper;
        }
    }
}