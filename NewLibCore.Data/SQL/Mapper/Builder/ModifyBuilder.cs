﻿using System;
using System.Linq;
using NewLibCore.Data.SQL.Mapper.Config;
using NewLibCore.Data.SQL.Mapper.EntityExtension;
using NewLibCore.Data.SQL.Mapper.ExpressionStatment;
using NewLibCore.Data.SQL.Mapper.Translation;
using NewLibCore.Validate;

namespace NewLibCore.Data.SQL.Mapper.Builder
{
    /// <summary>
    /// 更新操作builder类
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    internal class ModifyBuilder<TModel> : Builder<TModel> where TModel : PropertyMonitor, new()
    {
        private readonly TModel _instance;

        private readonly Boolean _isVerifyModel;

        private readonly ExpressionSegment _expressionSegment;

        public ModifyBuilder(TModel model, ExpressionSegment expressionSegment, Boolean isVerifyModel = false)
        {
            Parameter.Validate(model);
            Parameter.Validate(expressionSegment);

            _instance = model;
            _isVerifyModel = isVerifyModel;
            _expressionSegment = expressionSegment;
        }

        /// <summary>
        /// 构建一个修改操作的翻译结果
        /// </summary>
        /// <returns></returns>
        protected override TranslationResult ExecuteSegmentTranslate()
        {
            _instance.SetUpdateTime();

            if (_isVerifyModel)
            {
                _instance.Validate();
            }

            var (TableName, AliasName) = typeof(TModel).GetTableName();
            var propertys = _instance.GetChangedProperty();
            var template = String.Format(MapperConfig.DatabaseConfig.UpdateTemplate, TableName, AliasName, String.Join(",", propertys.Select(p => $@"{AliasName}.{p.Key}=@{p.Key}")));

            var translationSegment = new TranslationSegment(_expressionSegment);
            translationSegment.TranslationResult.Append(template, propertys.Select(c => new EntityParameter(c.Key, c.Value)));
            if (_expressionSegment.Where != null)
            {
                translationSegment.Translate();
            }
            _instance.Reset();

            translationSegment.TranslationResult.Append($@"{MapperConfig.DatabaseConfig.Extension.RowCount}");
            return translationSegment.TranslationResult;
        }
    }
}
