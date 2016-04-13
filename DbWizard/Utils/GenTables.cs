using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbWizard.CodeGenerate;
using System.Data;
using DbWizard.Models;
using DbWizard.Providers;

namespace DbWizard.Utils
{
    //取得表的方法
    public class GenTables
    {
        public static DataTable GetDBNames(string dbType, string conString)
        {
            var gen = GetGenProvider(dbType);
            var dbHelper = new DBHelper(conString);
            dbHelper.DataBaseType = dbType;
            return dbHelper.ExecuteQuery(gen.SqlGetDBNames()).Tables[0];
        }

        public static List<TableInfo> GetTables(string dbType, string conString)
        {
            var gen = GetGenProvider(dbType);
            var dbHelper = new DBHelper(conString);
            dbHelper.DataBaseType = dbType;
            var Tables = (from table in dbHelper.ExecuteQuery(gen.SqlGetTableNames()).Tables[0].AsEnumerable()
                          select new TableInfo { TableName = table.Field<string>("TableName") }).ToList();
            return Tables;
        }

        public static Table GetTables(string tableName, string dbType, string conString)
        {
            var gen = GetGenProvider(dbType);
            var dbHelper = new DBHelper(conString);
            dbHelper.DataBaseType = dbType;
            var table = new Table { TableName = tableName };
            table.TableKeys = dbHelper.ExecuteQuery(gen.SqlGetTableKeys(table.TableName)).Tables[0].AsEnumerable().Select(tableKey => tableKey.Field<string>("COLUMN_NAME")).ToList();
            table.TableSchemas = (from tableSchema in dbHelper.ExecuteQuery(gen.SqlGetTableSchemas(table.TableName)).Tables[0].AsEnumerable()
                                  select new TableSchema { ColumnName = Convert.ToString(tableSchema[0]), SqlTypeName = Convert.ToString(tableSchema[1]), MaxLength = Convert.ToInt16(tableSchema[2]), IsNullable = Convert.ToBoolean(tableSchema[3]), IsIdentity = Convert.ToBoolean(tableSchema[4]), IsPrimaryKey = Convert.ToBoolean(tableSchema[5]), Description = Convert.ToString(tableSchema[6]) }).ToList();

            return table;
        }

        private static ISqlGen GetGenProvider(string type)
        {
            switch (type)
            {
                case "Oracle":
                    return new OracleGen();

                case "SQLServer":
                default:
                    return new SqlServerGen();
            }
        }
    }
}
