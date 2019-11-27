﻿using System;
using System.Linq.Expressions;
using NewLibCore.Data.SQL.Mapper;
using NewLibCore.Data.SQL.Mapper.Extension;
using NewLibCore.Validate;

namespace NewLibCore.Data.SQL.MergeExpression
{
    /// <summary>
    /// 合并扩展
    /// </summary>
    public static class MergeExtensions
    {

        /// <summary>
        /// 合并一个表示 与 的表达式对象
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <typeparam name="T"></typeparam>
        public static void And<T>(this Merge<T> left, Expression<Func<T, Boolean>> right) where T : EntityBase
        {
            Parameter.Validate(left);
            Parameter.Validate(left.MergeExpression);

            Parameter.Validate(right);

            if (left.MergeExpression == null)
            {
                left.MergeExpression = right;
                return;
            }

            var type = typeof(T);
            var internalParameter = Expression.Parameter(type, type.GetTableName().AliasName);
            var parameterVister = new ParameterVisitor(internalParameter);
            var leftBody = parameterVister.Replace(left.MergeExpression.Body);
            var rightBody = parameterVister.Replace(right.Body);
            var newExpression = Expression.AndAlso(leftBody, rightBody);
            left.MergeExpression = Expression.Lambda<Func<T, Boolean>>(newExpression, internalParameter);
        }

        /// <summary>
        /// 合并一个表示 或 的表达式对象
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <typeparam name="T"></typeparam>
        public static void Or<T>(this Merge<T> left, Expression<Func<T, Boolean>> right) where T : EntityBase
        {
            Parameter.Validate(left);
            Parameter.Validate(left.MergeExpression);

            Parameter.Validate(right);

            if (left.MergeExpression == null)
            {
                left.MergeExpression = right;
                return;
            }

            var type = typeof(T);
            var internalParameter = Expression.Parameter(type, type.GetTableName().AliasName);
            var parameterVister = new ParameterVisitor(internalParameter);
            var leftBody = parameterVister.Replace(left.MergeExpression.Body);
            var rightBody = parameterVister.Replace(right.Body);
            var orExpression = Expression.OrElse(leftBody, rightBody);
            left.MergeExpression = Expression.Lambda<Func<T, Boolean>>(orExpression, internalParameter);
        }

        /// <summary>
        /// 合并一个表示 非 的表达式对象
        /// </summary>
        /// <param name="left"></param>
        /// <typeparam name="T"></typeparam>
        public static void Not<T>(this Merge<T> left) where T : EntityBase
        {
            Parameter.Validate(left);
            Parameter.Validate(left.MergeExpression);

            var lambdaExpression = (LambdaExpression)left.MergeExpression;
            var internalParameter = lambdaExpression.Parameters[0];
            var newExpression = Expression.Not(lambdaExpression.Body);
            left.MergeExpression = Expression.Lambda<Func<T, Boolean>>(newExpression, internalParameter);
        }
    }
}
