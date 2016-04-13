using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbWizard.Models;

namespace DbWizard.Utils
{
    // 工具类
    public class Util
    {
        //获取自增长字段
        public static string GetIdentity(List<TableSchema> TableSchemas)
        {
            foreach (var ts in TableSchemas.Where(ts => ts.IsIdentity))
            {
                return ts.ColumnName;
            }
            return "";
        }

        // 类型转换 从数据库类型转换为C#类型
        public static string GetTypeName(string SqlTypeName, bool IsNullable)
        {
            string str = "";
            switch (SqlTypeName.ToLower())
            {
                case "char":
                case "nchar":
                case "ntext":
                case "text":
                case "nvarchar":
                case "varchar":
                case "xml": str = "string"; break;//String

                case "smallint": str = "short"; break;//Int16
                case "int": str = "int"; break;//Int32
                case "bigint": str = "long"; break;//Int64
                case "binary":
                case "image":
                case "varbinary":
                case "timestamp": str = "byte[]"; break;//Byte[]
                case "tinyint": str = "SByte"; break;//SByte
                case "bit": str = "bool"; break;//Boolean
                case "float": str = "double"; break;//Double
                case "real": str = "Guid"; break;//Single
                case "uniqueidentifier": str = "Guid"; break;//Guid
                case "sql_variant": str = "object"; break;//Object
                case "decimal":
                case "numeric":
                case "money":
                case "smallmoney": str = "decimal"; break;//Decimal
                case "datetime":
                case "smalldatetime": str = "DateTime"; break;//DateTime
                default: str = SqlTypeName; break;
            }
            if (IsNullable)
            {
                switch (str)
                {
                    case "short":
                    case "int":
                    case "long":
                    case "SByte":
                    case "bool":
                    case "double":
                    case "Guid":
                    case "decimal":
                    case "DateTime":
                        str += "?";
                        break;
                    default:
                        break;
                }
            }
            return str;
        }

        //获取Insert插入语句
        public static string GetInsertSql(string TableName, List<TableSchema> TableSchemas)
        {
            var insertSql = new StringBuilder();
            insertSql.AppendFormat("Insert Into [{0}]", TableName);
            insertSql.Append("(");
            foreach (var ts in TableSchemas)
            {
                if (ts.IsIdentity) continue;
                insertSql.Append(ts.ColumnName);
                insertSql.Append(",");
            }
            insertSql.Remove(insertSql.Length - 1, 1);//删除最后一个逗号
            insertSql.Append(")");
            insertSql.Append(" Values");
            insertSql.Append("(");
            foreach (var ts in TableSchemas)
            {
                if (ts.IsIdentity) continue;
                insertSql.AppendFormat("@{0}", ts.ColumnName);
                insertSql.Append(",");
            }
            insertSql.Remove(insertSql.Length - 1, 1);//删除最后一个逗号
            insertSql.Append(")");
            return insertSql.ToString();
        }

        //获取Update更新语句
        public static string GetUpdateSql(string TableName, List<TableSchema> TableSchemas)
        {
            var updateSql = new StringBuilder();
            var whereSql = "";
            updateSql.AppendFormat("Update [{0}] set ", TableName);
            foreach (var ts in TableSchemas)
            {
                if (!ts.IsPrimaryKey && !ts.IsIdentity)
                {
                    updateSql.AppendFormat("{0}=@{0}", ts.ColumnName);
                    updateSql.Append(",");
                }
                else if (ts.IsPrimaryKey)
                {
                    if (whereSql != "") whereSql += ",";
                    whereSql += string.Format("{0}=@{0}", ts.ColumnName);
                }
            }
            updateSql.Remove(updateSql.Length - 1, 1);//删除最后一个逗号
            if (!string.IsNullOrEmpty(whereSql))
            {
                updateSql.AppendFormat(" WHERE {0}", whereSql);
            }
            return updateSql.ToString();
        }

        //获取Delete删除语句
        public static string GetDeleteSql(string TableName, List<TableSchema> TableSchemas)
        {
            var deleteSql = new StringBuilder();
            var whereSql = "";
            deleteSql.AppendFormat("DELETE FROM {0}", TableName);
            foreach (var ts in TableSchemas.Where(ts => ts.IsPrimaryKey))
            {
                if (whereSql != "") whereSql += ",";
                whereSql += string.Format("{0}=@{0}", ts.ColumnName);
            }
            if (!string.IsNullOrEmpty(whereSql))
            {
                deleteSql.AppendFormat(" WHERE {0}", whereSql);
            }
            return deleteSql.ToString();
        }

        public static string GetSelectSql(string TableName, List<TableSchema> TableSchemas)
        {
            var selectSql = new StringBuilder();
            selectSql.Append("SELECT ");
            foreach (var ts in TableSchemas)
            {
                selectSql.AppendFormat("{0}", ts.ColumnName);
                selectSql.Append(",");
            }
            selectSql.Remove(selectSql.Length - 1, 1);//删除最后一个逗号
            selectSql.AppendFormat(" FROM {0}", TableName);
            return selectSql.ToString();
        }

        public static string GetSelectSqlByID(string TableName, List<TableSchema> TableSchemas)
        {
            var selectSql = new StringBuilder();
            var whereSql = "";
            selectSql.Append("SELECT ");
            foreach (var ts in TableSchemas)
            {
                selectSql.AppendFormat("{0}", ts.ColumnName);
                selectSql.Append(",");
                if (ts.IsPrimaryKey)
                {
                    if (whereSql != "") whereSql += ",";
                    whereSql += string.Format("{0}=@{0}", ts.ColumnName);
                }
            }
            selectSql.Remove(selectSql.Length - 1, 1);//删除最后一个逗号
            selectSql.AppendFormat(" FROM {0}", TableName);
            if (!string.IsNullOrEmpty(whereSql))
            {
                selectSql.AppendFormat(" WHERE {0}", whereSql);
            }
            return selectSql.ToString();
        }
    }
}
