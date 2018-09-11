﻿using System;
using System.Linq.Expressions;
using NewLibCore.Data.Mapper.DomainSpecification.Factory;
using NewLibCore.Data.Mapper.PropertyExtension;

namespace NewLibCore.Data.Mapper.DomainSpecification.ConcreteSpecification
{
	/// <summary>
	/// 默认规约工厂
	/// </summary>
	public sealed class FilterFactory : SpecificationFactory
	{
		private static FilterFactory _filterFactory;

		static FilterFactory()
		{
			_filterFactory = new FilterFactory();
		}


		internal override Specification<T> Create<T>(Expression<Func<T, Boolean>> expression = null)
		{
			return expression == null ? new DefaultSpecification<T>() : new DefaultSpecification<T>(expression);
		}

		public static Specification<T> CreateFilter<T>(Expression<Func<T, Boolean>> expression = null) where T : PropertyMonitor, new()
		{
			return _filterFactory.Create(expression);
		}
	}
}