﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NewLibCore.Data.SQL.Mapper.EntityExtension;
using NewLibCore.Data.SQL.Mapper.ExpressionStatment;
using NewLibCore.Validate;

namespace NewLibCore.Data.SQL.Mapper
{
    /// <summary>
    /// 查询处理类
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    internal class QueryHandler<TModel> : Handler where TModel : new()
    {
        private SegmentManager _segmentManager;

        internal override void AddSegmentManager(SegmentManager segmentManager)
        {
            Parameter.Validate(segmentManager);
            _segmentManager = segmentManager;
        }

        internal override RawExecuteResult Execute()
        {
            var (Fields, AliasName) = StatementParse(_segmentManager.SelectField);

            var segment = TranslationSegment.CreateTranslation(_segmentManager);
            var mainTable = _segmentManager.From.AliaNameMapper[0];
            segment.Result.Append(String.Format(Instance.SelectTemplate, Fields, mainTable.Key, mainTable.Value));
            segment.Translate();

            var aliasMapper = _segmentManager.MergeAliasMapper();

            //当出现查询但张表不加Where条件时，则强制将IsDeleted=0添加到后面
            if (_segmentManager.Where == null)
            {
                segment.Result.Append($@"{RelationType.AND.ToString()} {mainTable.Value}.IsDeleted = 0");
            }
            else
            {
                foreach (var aliasItem in aliasMapper)
                {
                    segment.Result.Append($@"{RelationType.AND} {aliasItem.Value.ToLower()}.IsDeleted = 0");
                }
            }
            if (_segmentManager.Order != null)
            {
                var (fields, tableName) = StatementParse(_segmentManager.Order);
                var orderTemplate = Instance.OrderByBuilder(_segmentManager.Order.OrderBy, $@"{tableName}.{fields}");
                segment.Result.Append(orderTemplate);
            }

            if (_segmentManager.Pagination != null)
            {
                var pageIndex = (_segmentManager.Pagination.Size * (_segmentManager.Pagination.Index - 1)).ToString();
                var pageSize = _segmentManager.Pagination.Size.ToString();
                segment.Result.Append(Instance.Extension.Page.Replace("{value}", pageIndex).Replace("{pageSize}", pageSize));
            }

            return segment.Result.Execute();
        }

        /// <summary>
        ///判断表达式语句类型并转换为相应的sql
        /// </summary>
        /// <param name="expressionMapper">表达式分解后的对象</param>
        /// <returns></returns>
        private static (String Fields, String AliasName) StatementParse(ExpressionMapper expressionMapper)
        {
            var modelAliasName = new List<String>();
            if (expressionMapper == null) //如果表达式语句为空则表示需要翻译为SELECT a.xxx,a.xxx,a.xxx 类型的语句
            {
                var modelType = typeof(TModel);
                var f = new List<String>();
                {
                    var aliasName = modelType.GetTableName().AliasName;
                    modelAliasName.Add(aliasName);
                    var mainModelPropertys = modelType.GetProperties().Where(w => w.GetCustomAttributes<PropertyValidate>().Any()).ToList();
                    foreach (var item in mainModelPropertys)
                    {
                        f.Add($@"{aliasName}.{item.Name}");
                    }
                }
                return (String.Join(",", f), modelAliasName.FirstOrDefault());
            }

            var fields = (LambdaExpression)expressionMapper.Expression;
            foreach (var item in fields.Parameters)
            {
                modelAliasName.Add(item.Type.GetTableName().AliasName);
            }

            if (fields.Body.NodeType == ExpressionType.Constant)
            {
                var constant = (ConstantExpression)fields.Body;
                return (constant.Value + "", modelAliasName.FirstOrDefault());
            }

            if (fields.Body.NodeType == ExpressionType.MemberAccess)
            {
                var members = (fields.Body as MemberExpression);
                return (members.Member.Name, modelAliasName.FirstOrDefault());
            }

            var anonymousObjFields = new List<String>();
            var bodyArguments = (fields.Body as NewExpression).Arguments;
            foreach (var item in bodyArguments)
            {
                var member = (MemberExpression)item;
                var fieldName = ((ParameterExpression)member.Expression).Type.GetTableName().AliasName;
                anonymousObjFields.Add($@"{fieldName}.{member.Member.Name}");
            }
            return (String.Join(",", anonymousObjFields), modelAliasName.FirstOrDefault());
        }
    }
}