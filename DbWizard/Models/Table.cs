using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbWizard.Utils;

namespace DbWizard.Models
{
    //表结构
    public class Table
    {
        public string TableName { get; set; }

        public List<string> TableKeys { get; set; }

        public List<TableSchema> TableSchemas { get; set; }

        public string Identity
        {
            get { return Util.GetIdentity(TableSchemas); }
        }

        public string InsertSql
        {
            get { return Util.GetInsertSql(TableName, TableSchemas); }
        }

        public string UpdateSql
        {
            get { return Util.GetUpdateSql(TableName, TableSchemas); }
        }

        public string DeleteSql
        {
            get { return Util.GetDeleteSql(TableName, TableSchemas); }
        }

        public string SelectSql
        {
            get { return Util.GetSelectSql(TableName, TableSchemas); }
        }

        public string SelectSqlByID
        {
            get { return Util.GetSelectSqlByID(TableName, TableSchemas); }
        }
    }

    public class TableInfo
    {
        public string TableName { get; set; }

        public bool Check { get; set; }
    }
}
