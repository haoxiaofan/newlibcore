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
    internal class SelectHandler<TModel> : Handler where TModel : new()
    {
        private readonly SegmentManager _segmentManager;

        /// <summary>
        /// 初始化一个SelectHandler类的实例
        /// </summary>
        /// <param name="segmentManager">表达式分解后的对象</param>
        internal SelectHandler(SegmentManager segmentManager)
        {
            Parameter.Validate(segmentManager);
            _segmentManager = segmentManager;
        }

        protected override TranslationResult ExecuteTranslate()
        {
            var (Fields, AliasName) = StatementParse(_segmentManager.Field);

            var segment = TranslationSegment.CreateTranslation(_segmentManager);
            segment.Result.Append(ReplacePlaceholder(Fields, AliasName));
            segment.Translate();

            var aliasMapper = _segmentManager.MergeAliasMapper();

            //当出现查询但张表不加Where条件时，则强制将IsDeleted=0添加到后面
            if (_segmentManager.Where == null)
            {
                segment.Result.Append($@"{RelationType.AND.ToString()} {AliasName}.IsDeleted = 0");
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

            if (_segmentManager.Page != null)
            {
                var pageIndex = (_segmentManager.Page.Size * (_segmentManager.Page.Index - 1)).ToString();
                var pageSize = _segmentManager.Page.Size.ToString();
                segment.Result.Append(Instance.Extension.Page.Replace("{value}", pageIndex).Replace("{pageSize}", pageSize));
            }

            return segment.Result;
        }

        private String ReplacePlaceholder(String Fields, String AliasName)
        {
            return String.Format(Instance.SelectTemplate, Fields, typeof(TModel).GetTableName().TableName, AliasName);
        }

        /// <summary>
        ///判断表达式语句类型并转换为相应的sql
        /// </summary>
        /// <param name="statement">表达式分解后的对象</param>
        /// <returns></returns>
        protected override (String Fields, String AliasName) StatementParse(Statement statement)
        {
            var modelAliasName = new List<String>();
            if (statement == null) //如果表达式语句为空则表示需要翻译为SELECT a.xxx,a.xxx,a.xxx 类型的语句
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

            var fields = (LambdaExpression)statement.Expression;
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
