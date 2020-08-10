﻿using System;

namespace NewLibCore.Validate
{
    public static class Parameter
    {
        /// <summary>
        /// 验证引用类型是否合法
        /// </summary>
        public static void IfNullOrZero(Object argument)
        {
            IfNullOrZero(argument, false);
        }

        /// <summary>
        /// 验证引用类型是否合法
        /// </summary>
        public static void IfNullOrZero(Object argument, Boolean canNull)
        {
            if (!canNull && (argument == null || String.IsNullOrEmpty(argument.ToString())))
            {
                throw new ArgumentNullException("参数不能为0或null");
            }
        }


        public static void IfNullOrZero(ValueType valueType)
        {
            IfNullOrZero(valueType, false);
        }

        /// <summary>
        /// 验证值类型是否合法
        /// </summary>
        public static void IfNullOrZero(ValueType valueType, Boolean canZero)
        {
            var type = valueType.GetType();

            if (type.IsValueType && type.IsNumeric())
            {
                var flag = !canZero ? valueType.CastTo(0.0) <= 0.0 : valueType.CastTo(0.0) < 0.0;

                if (flag)
                {
                    throw new ArgumentNullException("参数不能为0或null");
                }
            }
        }
    }
}
