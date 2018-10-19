﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NewLibCore.Data.Mapper.MapperExtension;
using NewLibCore.Data.Mapper.PropertyExtension;

namespace NewLibCore.Data.Mapper
{
    internal abstract class SqlBuilder<TModel> where TModel : PropertyMonitor, new()
    {
        protected Type ModelType { get; }

        protected TModel ModelInstance { get; }

        protected SqlBuilder(TModel model)
        {
            if (model == null)
            {
                ModelInstance = Activator.CreateInstance<TModel>();
                ModelType = typeof(TModel);
            }
            else
            {
                ModelInstance = model;
                ModelType = model.GetType();
            }
        }

        protected internal abstract BuildEntry<TModel> Build();

        protected void ValidateModel(IList<PropertyInfo> propertyInfos)
        {
            var propertys = propertyInfos;
            foreach (var propertyItem in propertys)
            {
                if (!propertyItem.CustomAttributes.Any())
                {
                    continue;
                }

                var validateBases = GetValidateAttributes(propertyItem);
                for (int i = 0; i < validateBases.Count; i++)
                {
                    if (!validateBases[i].IsValidate(propertyItem.GetValue(ModelInstance)))
                    {
                        if (SetPropertyDefaultValue(validateBases[i + 1], propertyItem))
                        {
                            i = i + 1;
                            continue;
                        }
                        throw new Exception(validateBases[i].FailReason($@"{propertyItem.DeclaringType.FullName}.{propertyItem.Name}"));
                    }
                    else
                    {
                        SetPropertyDefaultValue(validateBases[i], propertyItem);
                    }
                }
            }
        }

        private Boolean SetPropertyDefaultValue(ValidateBase validateItem, PropertyInfo propertyItem)
        {
            if (validateItem is PropertyDefaultValueAttribute defaultValueAttribute)
            {
                var propertyInstanceValue = propertyItem.GetValue(ModelInstance);
                if (String.IsNullOrEmpty(propertyInstanceValue + "") || (propertyInstanceValue.GetType() == typeof(DateTime) && (DateTime)propertyInstanceValue == default(DateTime)))
                {
                    propertyItem.SetValue(ModelInstance, defaultValueAttribute.Value);
                }
                return true;
            }
            return false;
        }

        private IList<ValidateBase> GetValidateAttributes(PropertyInfo propertyInfo)
        {
            var validateAttributes = propertyInfo.GetCustomAttributes<ValidateBase>(true);

            if (validateAttributes.GroupBy(g => g.Order).Where(w => w.Count() > 1).Any())
            {
                throw new Exception($@"{propertyInfo.Name} 中使用了多个优先级相同的特性");
            }

            return validateAttributes.OrderByDescending(o => o.Order).ToList();
        }
    }
}
