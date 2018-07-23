﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NewLibCore.Data.Mapper.InternalDataStore;
using NewLibCore.Data.Mapper.MapperExtension;

namespace NewLibCore.Data.Mapper
{
	internal class AddBuilder<TModel> : SqlBuilder<TModel> where TModel : class, new()
	{
		private Boolean _isVerifyModel;

		public AddBuilder(TModel model, Boolean isVerifyModel = false) : base(model)
		{
			_isVerifyModel = isVerifyModel;
		}

		protected override IList<PropertyInfo> GetProperty()
		{
			return ModelType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
		}

		protected internal override BuildEntry<TModel> Build()
		{
			var buildEntry = new BuildEntry<TModel>();

			if (_isVerifyModel)
			{
				ValidateModel();
			}

			var columns = GetColumns();
			buildEntry.Append($@" INSERT {ModelType.Name} ({String.Join(",", columns.Select(c => c.Name))} ) VALUES ({String.Join(",", columns.Select(key => $@"@{key.Name}"))})");

			foreach (var item in columns.Select(c => new ParameterMapper($@"@{c.Name}", c.GetValue(ModelInstance))))
			{
				buildEntry.Parameters.Add(item);
			}
			return buildEntry;
		}

		private IEnumerable<PropertyInfo> GetColumns()
		{
			foreach (var item in ModelType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(w => w.PropertyType.Name != "IList`1" && w.CustomAttributes.Count() != 0))
			{
				yield return item;
			}
		}

	}
}
