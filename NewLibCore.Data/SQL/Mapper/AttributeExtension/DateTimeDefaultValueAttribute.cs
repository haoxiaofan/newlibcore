﻿using System;

namespace NewLibCore.Data.SQL.Mapper.AttributeExtension
{
    /// <summary>
    /// 提供默认时间值
    /// </summary>
    public class DateTimeDefaultValueAttribute : DefaultValueAttribute
    {
        public DateTimeDefaultValueAttribute() : base(typeof(DateTime), DateTime.Now)
        {

        }
    }
}