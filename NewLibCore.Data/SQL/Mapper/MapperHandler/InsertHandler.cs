﻿using System;
using System.Collections.Generic;
using System.Linq;
using NewLibCore.Data.SQL.Mapper.EntityExtension;
using NewLibCore.Validate;

namespace NewLibCore.Data.SQL.Mapper
{

    /// <summary>
    /// 新增操作处理
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    internal class InsertHandler<TModel> : Handler where TModel : EntityBase, new()
    {
        private readonly TModel _instance;

        /// <summary>
        /// 初始化一个InsertHandler类的实例
        /// </summary>
        /// <param name="model">要插入的模型</param>
        internal InsertHandler(TModel model)
        {
            Parameter.Validate(model);
            _instance = model;
        }

        internal override RawExecuteResult Execute()
        {
            _instance.OnChanged();
            if (MapperConfig.EnableModelValidate)
            {
                _instance.Validate();
            }

            var propertyInfos = _instance.GetChangedProperty();

            var tableName = typeof(TModel).GetTableName().TableName;
            var template = ReplacePlaceholder(propertyInfos, tableName);
            return TranslationResult.CreateTranslationResult().Append(template, CreateParameter(propertyInfos)).Execute();
        }

        private static IEnumerable<EntityParameter> CreateParameter(IReadOnlyList<KeyValuePair<String, Object>> propertyInfos)
        {
            return propertyInfos.Select(c => new EntityParameter(c.Key, c.Value));
        }

        private String ReplacePlaceholder(IReadOnlyList<KeyValuePair<String, Object>> propertyInfos, String tableName)
        {
            return String.Format(Instance.AddTemplate, tableName, String.Join(",", propertyInfos.Select(c => c.Key)), String.Join(",", propertyInfos.Select(key => $@"@{key.Key}")), Instance.Extension.Identity);
        }
    }
}