using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbWizard.Providers
{
   public interface ISqlGen
    {
        string SqlGetDBNames();                       //取得数据库名称
        string SqlGetTableNames();                          //取得表名
        string SqlGetTableSchemas(string tableName);        //取得表结构
        string SqlGetTableKeys(string tableName);           //sql server 调用sp_pkeys取得主key
    }
}
