﻿using System;

namespace NewLibCore.Data.SQL.Mapper
{
    /// <summary>
    /// mssql数据库sql模板配置
    /// </summary>
    internal class MsSqlTemplate : TemplateBase
    {


        internal override String UpdateTemplate
        {
            get
            {
                return "UPDATE {0} SET {1} FROM {2} AS {0} ";
            }
        }

        internal override String Identity
        {
            get
            {
                return " SELECT @@IDENTITY ;";
            }
        }

        internal override String RowCount
        {
            get
            {
                return " SELECT @@ROWCOUNT ;";
            }
        }

        protected override void AppendPredicateType()
        {
            PredicateMapper.Add(PredicateType.FULL_LIKE, "{0} LIKE '%{1}%'");
            PredicateMapper.Add(PredicateType.START_LIKE, "{0} LIKE '{1}%'");
            PredicateMapper.Add(PredicateType.END_LIKE, "{0} LIKE '%{1}' ");
            PredicateMapper.Add(PredicateType.IN, "{0} IN ({1})");
        }

        internal override String CreatePredicate(PredicateType predicateType, String left, String right)
        {
            return String.Format(PredicateMapper[predicateType], left, right);
        }

        internal override String Page(Int32 pageIndex, Int32 pageSize, ParserResult parserResult)
        {
            var sql = "";
            if (MapperConfig.MsSqlPaginationVersion == MsSqlPaginationVersion.GreaterThan2012)
            {
                sql = $@" {parserResult} OFFSET ({pageIndex * pageSize}) ROWS FETCH NEXT {pageSize} ROWS ONLY ;";
            }
            else
            {
                sql = parserResult.ToString();
                sql = $@" SELECT * FROM ( {sql.Insert(sql.IndexOf(" "), $@" TOP({pageSize}) ROW_NUMBER() OVER(ORDER BY ) AS rownumber,")} ) AS temprow WHERE temprow.rownumber>({pageSize}*({pageIndex}-1))";
            }
            return sql;
        }
    }
}
