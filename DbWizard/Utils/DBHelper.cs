using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Data.Common;
using System.Configuration;

namespace DbWizard.Utils
{
    public class DBHelper
    {

        /// <summary>
        /// 通用连接接口定义
        /// </summary>
        public IDbConnection connection = null;

        /// <summary>
        /// 事务处理接口
        /// </summary>
        private IDbTransaction transaction = null;

        /// <summary>
        /// 数据库类型
        /// </summary>
        private static string databaseType = "SQLServer";

        /// <summary>
        /// 数据库连接字符串 
        /// </summary>
        public string strConn = null;

        public int TimeOut { get; set; }

        public string DataBaseType
        {
            get
            {
                return databaseType;
            }
            set
            {
                databaseType = value;
            }
        }

        public DBHelper(string strConn)  //构造函数
        {
            connection = CreateConnection(strConn);
            this.strConn = strConn;
            this.TimeOut = 600;
        }


        /// <summary>
        /// 建立数据库连接对象
        /// </summary>
        /// <returns>数据库连接对象</returns>
        public static IDbConnection CreateConnection(string strConn)
        {
            IDbConnection conn;
            switch (databaseType)
            {
                case "SQLServer":	//SQL Server的连接
                    conn = new SqlConnection(strConn);
                    break;
                case "Oledb":	//Ole Db的连接
                    conn = new OleDbConnection(strConn);
                    break;
                case "Oracle":	//Oracle的连接
                    //conn = new OracleConnection(strConn);
                    throw new NotImplementedException();
                case "Odbc":	//ODBC的连接
                    conn = new OdbcConnection(strConn);
                    break;
                default:       //默认连接为SQL Server
                    conn = new SqlConnection(strConn);
                    break;
            }
            return conn;
        }

        /// <summary>
        /// 建立数据库命令执行对象 
        /// </summary>
        /// <param name="strCmd">要执行的SQL语句</param>
        /// <param name="conn">数据库连接对象</param>
        /// <returns>数据库命令对象</returns>
        public static IDbCommand CreateCommand(string strCmd, IDbConnection conn)
        {
            IDbCommand cmd = null;
            switch (databaseType)
            {
                case "SQLServer":
                    cmd = new SqlCommand(strCmd, (SqlConnection)conn);
                    break;
                case "Oledb":
                    cmd = new OleDbCommand(strCmd, (OleDbConnection)conn);
                    break;
                case "Oracle":
                    //cmd = new OracleCommand(strCmd, (OracleConnection)conn);
                    break;
                case "Odbc":
                    cmd = new OdbcCommand(strCmd, (OdbcConnection)conn);
                    break;
            }
            return cmd;
        }


        /// <summary>
        /// 建立数据适配器对象
        /// </summary>
        /// <param name="cmd">数据库命令对象</param>
        /// <returns>数据适配器对象</returns>
        public static DbDataAdapter CreateDataAdapter(IDbCommand cmd)
        {
            DbDataAdapter DataAdapter = null;
            switch (databaseType)
            {
                case "SQLServer":
                    DataAdapter = new SqlDataAdapter((SqlCommand)cmd);
                    break;
                case "Oledb":
                    DataAdapter = new OleDbDataAdapter((OleDbCommand)cmd);
                    break;
                case "Oracle":
                    //DataAdapter = new OracleDataAdapter((OracleCommand)cmd);
                    break;
                case "Odbc":
                    DataAdapter = new OdbcDataAdapter((OdbcCommand)cmd);
                    break;
            }
            return DataAdapter;
        }

        /// <summary>
        /// 查询操作,即查询记录
        /// </summary>
        /// <param name="strCmd">需要执行的SQL语句</param>
        /// <returns>查询结果(数据集)</returns>
        public DataSet ExecuteQuery(string strCmd)
        {
            DataSet dataSet = new DataSet();
            try
            {
                IDbCommand command = CreateCommand(strCmd, connection); //读取数据命令
                command.CommandTimeout = TimeOut;
                command.Transaction = transaction;

                DbDataAdapter dataAdapter = CreateDataAdapter(command); //读取数据接口

                dataAdapter.Fill(dataSet);	//填充数据到数据集
            }
            catch (Exception SelectException)
            {
                throw SelectException;	//传递错误信息
            }

            return dataSet;
        }

        /// <summary>
        /// 执行数据库查询操作
        /// </summary>
        /// <param name="strCmd">要执行的SQL语句</param>
        /// <param name="tableName">填充的表名</param>
        /// <returns>数据集</returns>
        public DataSet ExecuteQuery(string strCmd, string tableName)
        {
            DataSet dataSet = new DataSet();
            try
            {
                IDbCommand command = CreateCommand(strCmd, connection);		//读取数据命令
                command.CommandTimeout = TimeOut;
                command.Transaction = transaction;

                DbDataAdapter dataAdapter = CreateDataAdapter(command);		//读取数据接口

                dataAdapter.Fill(dataSet, tableName);						//填充数据到数据集
            }
            catch (Exception SelectException)
            {
                throw SelectException;	//传递错误信息
            }
            return dataSet;
        }


        /// <summary>
        /// 带返回值的存储过程
        /// </summary>
        /// <param name="strCmd"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public DataSet ExecuteQueryByParam(string strCmd, SqlParameter[] paras)
        {
            DataSet dataSet = new DataSet();
            try
            {
                SqlParameter temp = null;
                IDbCommand command = CreateCommand(strCmd, connection);
                command.CommandType = CommandType.StoredProcedure;
                foreach (SqlParameter param in paras)
                {
                    if (param.Direction == ParameterDirection.Output)
                    {
                        temp = param;
                    }
                    command.Parameters.Add(param);
                }
                DbDataAdapter dataAdapter = CreateDataAdapter(command);		//读取数据接口
                dataAdapter.Fill(dataSet);
            }
            catch (Exception ScalarException)
            {
                throw ScalarException;
            }
            return dataSet;
        }

        /// <summary>
        /// 执行数据库查询操作
        /// </summary>
        /// <param name="strCmd">要执行的SQL语句</param>
        /// <param name="dataTable">查询结果填充到此dataTable</param>
        public void ExecuteQuery(string strCmd, DataTable dataTable)
        {
            try
            {
                IDbCommand command = CreateCommand(strCmd, connection);		//读取数据命令
                command.Transaction = transaction;
                DbDataAdapter dataAdapter = CreateDataAdapter(command);		//读取数据接口
                dataAdapter.Fill(dataTable);					//填充数据到数据集
            }
            catch (Exception SelectException)
            {
                throw SelectException;	//传递错误信息
            }

        }

        /// <summary>
        /// 执行数据库查询操作
        /// </summary>
        /// <param name="strCmd">需要执行的SQL语句</param>
        /// <returns>操作执行是否执行成功，成功返回true，失败返回false</returns>
        public bool ExecuteNonQuery(string strCmd)
        {
            bool result = false;
            if (connection.State == ConnectionState.Closed)
                connection.Open();
            try
            {
                IDbCommand command = CreateCommand(strCmd, connection);
                command.Transaction = transaction;
                result = (command.ExecuteNonQuery() > 0);
            }
            catch (Exception NonException)
            {
                connection.Close();
                throw NonException;
            }
            connection.Close();
            return result;
        }


        /// <summary>
        /// 执行数据库查询操作
        /// </summary>
        /// <param name="strCmd">需要执行的SQL语句</param>
        /// <returns>操作执行是否执行成功，成功返回true，失败返回false</returns>
        public bool ExecuteNonQuery_Trans(string strCmd)
        {
            bool result = false;
            if (connection.State == ConnectionState.Closed)
                connection.Open();
            try
            {
                IDbCommand command = CreateCommand(strCmd, connection);
                command.Transaction = transaction;
                result = (command.ExecuteNonQuery() > 0);
            }
            catch (Exception ex)
            {
                connection.Close();
                throw ex;
            }
            return result;
        }


        /// <summary>
        /// 执行数据库查询操作
        /// </summary>
        /// <param name="strCmd">需要执行的SQL语句</param>
        /// <returns>操作执行是否执行成功，成功返回true，失败返回false</returns>
        public bool ExecuteNonQuery2(string strCmd)
        {
            bool result = false;
            if (connection.State == ConnectionState.Closed)
                connection.Open();
            try
            {
                IDbCommand command = CreateCommand(strCmd, connection);
                command.CommandTimeout = TimeOut;
                command.Transaction = transaction;
                result = (command.ExecuteNonQuery() > 0);
            }
            catch (Exception NonException)
            {
                connection.Close();
                throw NonException;
            }
            connection.Close();
            return result;
        }

        private object GetValueFromProc(string procName, SqlParameter[] prams)
        {
            object result = null;
            if (connection.State == ConnectionState.Closed)
                connection.Open();
            try
            {
                IDbCommand command = CreateCommand(procName, connection);
                command.CommandType = CommandType.StoredProcedure;
                if (prams != null)
                {
                    foreach (SqlParameter parameter in prams)
                    {
                        command.Parameters.Add(parameter);
                    }
                }
                //command.Parameters.Add(new SqlParameter(result, SqlDbType.Int, 4, ParameterDirection.ReturnValue,false, 0, 0, string.Empty, DataRowVersion.Default, null));
                command.ExecuteNonQuery();
                foreach (SqlParameter param in prams)
                {
                    if (param.Direction == ParameterDirection.Output)
                    {
                        result = param.Value;
                        break;
                    }
                }
            }
            catch (Exception ScalarException)
            {
                connection.Close();
                throw ScalarException;
            }
            return result;
        }



        /// <summary>
        /// 判断数据库中是否存在指定条件的数据
        /// </summary>
        /// <param name="Sql"></param>
        /// <returns></returns>
        public bool ExistData(string strCmd)
        {
            bool result = false;
            object obj = null;
            if (connection.State == ConnectionState.Closed)
                connection.Open();
            try
            {

                IDbCommand command = CreateCommand(strCmd, connection);
                obj = command.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            if (obj != null)
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// 查询操作,即查询记录   
        /// </summary>
        /// <param name="strCmd">需要执行的SQL语句</param>
        /// <returns>查询结果单值</returns>
        public object ExecuteScalar(string strCmd)
        {
            object result = null;
            if (connection.State == ConnectionState.Closed)
                connection.Open();
            try
            {
                IDbCommand command = CreateCommand(strCmd, connection);
                result = command.ExecuteScalar();
            }
            catch (Exception ScalarException)
            {
                connection.Close();
                throw ScalarException;
            }
            return result;
        }

        /// <summary>
        /// 带返回值的存储过程
        /// </summary>
        /// <param name="strCmd"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public object ExecuteScalarByParam(string strCmd, SqlParameter[] paras)
        {
            object result = null;
            if (connection.State == ConnectionState.Closed)
                connection.Open();

            try
            {
                SqlParameter temp = null;
                IDbCommand command = CreateCommand(strCmd, connection);
                command.CommandType = CommandType.StoredProcedure;
                foreach (SqlParameter param in paras)
                {
                    if (param.Direction == ParameterDirection.Output)
                    {
                        temp = param;
                    }
                    command.Parameters.Add(param);
                }
                command.ExecuteNonQuery();
                result = temp.Value.ToString();
            }
            catch (Exception ScalarException)
            {
                connection.Close();
                throw ScalarException;
            }
            connection.Close();
            return result;
        }

        /// <summary>
        /// 批量插入数据
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <param name="tableName">表明</param>
        /// <returns></returns>
        public bool SqlBulkCopy(DataTable sourceTable, string sTableName, IDictionary<string, string> dicColMapping)
        {
            bool result = false;
            try
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();
                BeginTransaction();
                using (SqlBulkCopy bulkCoye = new SqlBulkCopy((SqlConnection)connection, SqlBulkCopyOptions.Default, (SqlTransaction)transaction))
                {
                    //设置要批量写入的表 
                    bulkCoye.DestinationTableName = sTableName;
                    ////设定超时时间  
                    bulkCoye.BulkCopyTimeout = 60;
                    ////每批插入的行数(如果入库失败，只是本批次事物回滚，如果想全部回滚还需要加参数SqlTransaction)  
                    bulkCoye.BatchSize = 1000;
                    ////在上面定义的批次里，每准备插入1条数据时，呼叫相应的事件（这时只是准备，没有真正入库）  
                    bulkCoye.NotifyAfter = 1000;
                    foreach (KeyValuePair<string, string> kvp in dicColMapping)
                    {
                        bulkCoye.ColumnMappings.Add(kvp.Key, kvp.Value);
                    }
                    bulkCoye.WriteToServer(sourceTable);
                    Commit();
                    bulkCoye.Close();
                }
                result = true;
            }
            catch (Exception e)
            {
                Rollback();
                throw e;
            }
            return result;
        }

        /// <summary>
        /// 把DataSet更新到数据库
        /// </summary>
        /// <param name="strSQL"></param>
        /// <param name="tableName"></param>
        /// <param name="ds"></param>
        /// <returns></returns>
        public bool DataSetInsert(string strSQL, string tableName, DataSet source)
        {
            bool result = false;
            try
            {
                foreach (DataRow row in source.Tables[0].Rows)
                {
                    if (row.RowState == DataRowState.Unchanged)
                    {
                        row.SetAdded();
                    }
                }
                using (IDbCommand command = CreateCommand(strSQL, connection))
                {
                    DbDataAdapter adapter = CreateDataAdapter(command);
                    DbCommandBuilder builder = new SqlCommandBuilder(adapter as SqlDataAdapter);
                    int count = adapter.Update(source, tableName);
                    source.AcceptChanges();
                }
                result = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 把DataSet更新到数据库
        /// </summary>
        /// <param name="strSQL"></param>
        /// <param name="tableName"></param>
        /// <param name="ds"></param>
        /// <returns></returns>
        public bool DataSetInsert(string strSQL, DataTable dtSource)
        {
            bool result = false;
            try
            {
                foreach (DataRow row in dtSource.Rows)
                {
                    if (row.RowState == DataRowState.Unchanged)
                    {
                        row.SetAdded();
                    }
                }
                using (IDbCommand command = CreateCommand(strSQL, connection))
                {
                    DbDataAdapter adapter = CreateDataAdapter(command);
                    DbCommandBuilder builder = new SqlCommandBuilder(adapter as SqlDataAdapter);
                    int count = adapter.Update(dtSource);
                    dtSource.AcceptChanges();
                }
                result = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// 把DataSet更新到数据库
        /// </summary>
        /// <param name="strSQL"></param>
        /// <param name="tableName"></param>
        /// <param name="ds"></param>
        /// <returns></returns>
        public bool DataSetInsert2(string strSQL, DataTable dtSource)
        {
            bool result = false;
            try
            {
                using (IDbCommand command = CreateCommand(strSQL, connection))
                {
                    DbDataAdapter adapter = CreateDataAdapter(command);
                    DbCommandBuilder builder = new SqlCommandBuilder(adapter as SqlDataAdapter);
                    int count = adapter.Update(dtSource);
                    dtSource.AcceptChanges();
                }
                result = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }





        /// <summary>
        /// 把DataSet更新到数据库
        /// </summary>
        /// <param name="strSQL"></param>
        /// <param name="tableName"></param>
        /// <param name="ds"></param>
        /// <returns></returns>
        public bool DataSetInsert3(string strSQL, DataTable dtSource, SqlCommand cmd)
        {
            dtSource.AcceptChanges();
            bool result = false;
            try
            {
                foreach (DataRow row in dtSource.Rows)
                {

                    if (row.RowState == DataRowState.Unchanged)
                    {
                        row.SetModified();
                    }
                }
                using (IDbCommand command = CreateCommand(strSQL, connection))
                {
                    DbDataAdapter adapter = CreateDataAdapter(command);
                    DbCommandBuilder builder = new SqlCommandBuilder(adapter as SqlDataAdapter);
                    adapter.UpdateCommand = cmd;
                    int count = adapter.Update(dtSource);
                    dtSource.AcceptChanges();
                }
                result = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }


        /// <summary>
        /// 创建空的表结构
        /// </summary>
        /// <returns></returns>
        public DataTable CreateDataTable(string[] columns, string tableName)
        {
            DataTable dt = new DataTable(tableName);
            foreach (string column in columns)
            {
                DataColumn col = new DataColumn(column, Type.GetType("System.String"));
                dt.Columns.Add(col);
            }
            return dt;
        }

        /// <summary>
        /// 打开数据库连接
        /// </summary>
        public void OpenConnection()
        {
            if (connection.State == ConnectionState.Closed)
                connection.Open();
        }

        /// <summary>
        /// 关闭数据库连接
        /// </summary>
        public void CloseConnection()
        {
            connection.Close();
        }


        public void BeginTransaction()
        {
            if (connection.State == ConnectionState.Closed)
                connection.Open();
            transaction = connection.BeginTransaction();
        }

        public void Commit()
        {
            transaction.Commit();

        }

        public void Rollback()
        {
            transaction.Rollback();
        }

    }
}
