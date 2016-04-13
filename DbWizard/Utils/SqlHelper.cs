using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;

namespace DbWizard.Utils
{
    public class SqlHelper
    {
        /// <summary>
        /// 批量导入数据
        /// </summary>
        /// <param name="connectionString">将要导入数据库的连接字符串</param>
        /// <param name="table">数据源</param>
        public static void BulkInsertToDatabase(string connectionString, DataTable table)
        {
            BulkInsertToDatabase(connectionString, table.TableName, table);
        }

        /// <summary>
        /// 批量导入数据
        /// </summary>
        /// <param name="connectionString">将要导入数据库的连接字符串</param>
        /// <param name="tableName">数据库中对应表名</param>
        /// <param name="table">数据源</param>
        public static void BulkInsertToDatabase(string connectionString, string tableName, DataTable table)
        {
            Hashtable mapping = new Hashtable();
            foreach (DataColumn column in table.Columns)
            {
                mapping.Add(column.ColumnName, column.ColumnName);
            }
            BulkInsertToDatabase(connectionString, tableName, mapping, table);
        }

        /// <summary>
        /// 批量导入数据
        /// </summary>
        /// <param name="connectionString">将要导入数据库的连接字符串</param>
        /// <param name="tableName">数据库中对应表名</param>
        /// <param name="columnsMapping">对应字段映射</param>
        /// <param name="table">数据源</param>
        public static void BulkInsertToDatabase(string connectionString, string tableName, Hashtable mapping, DataTable table)
        {
            try
            {
                //声明数据库连接
                SqlConnection con = new SqlConnection(connectionString);
                con.Open();
                //声明SqlBulkCopy ,using释放非托管资源
                using (SqlBulkCopy sqlBC = new SqlBulkCopy(con))
                {
                    //一次批量的插入的数据量
                    sqlBC.BatchSize = 100000;
                    //超时之前操作完成所允许的秒数，如果超时则事务不会提交 ，数据将回滚，所有已复制的行都会从目标表中移除
                    sqlBC.BulkCopyTimeout = 60;

                    //设置要批量写入的表
                    sqlBC.DestinationTableName = tableName;
                    //自定义的datatable和数据库的字段进行对应
                    foreach (var item in mapping.Keys)
                    {
                        sqlBC.ColumnMappings.Add(item.ToString(), mapping[item].ToString());
                    }
                    //批量写入
                    sqlBC.WriteToServer(table);
                }
                con.Dispose();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void AttachParameters(SqlCommand command, SqlParameter[] commandParameters)
        {
            foreach (SqlParameter p in commandParameters)
            {
                //check for derived output value with no value assigned
                if (p.Direction == ParameterDirection.InputOutput && p.Value == null)
                {
                    p.Value = DBNull.Value;
                }

                command.Parameters.Add(p);
            }
        }

        private static void AssignParameterValues(SqlParameter[] commandParameters, Object[] parameterValues)
        {
            if (commandParameters == null || parameterValues == null)
            {
                //do nothing If we get no data
                return;
            }

            // we must have the same number of values as we pave parameters to put them in
            if (commandParameters.Length != parameterValues.Length)
            {
                throw new ArgumentException("Parameter count does not match Parameter Value count.");
            }

            //iterate through the SqlParameters, assigning the values from the corresponding position in the 
            //value array
            for (int i = 0; i < commandParameters.Length; i++)
            {
                commandParameters[i].Value = parameterValues[i];
            }
        }

        private static void PrepareCommand(SqlCommand command, SqlConnection connection, SqlTransaction transaction, CommandType commandType, String commandText, SqlParameter[] commandParameters)
        {
            //if the provided connection is not open, we will open it
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            //associate the connection with the command
            command.Connection = connection;

            //set the command text (stored procedure name or SQL statement)
            command.CommandText = commandText;

            //if we were provided a transaction, assign it.
            if (transaction != null)
            {
                command.Transaction = transaction;
            }

            //set the command type
            command.CommandType = commandType;

            //attach the command parameters if they are provided
            if (commandParameters != null)
            {
                AttachParameters(command, commandParameters);
            }
            return;
        }

        public static int ExecuteNonQuery(String connectionString, CommandType commandType, String commandText, params  SqlParameter[] commandParameters)
        {
            //create & open a SqlConnection, and dispose of it after we are done.
            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                cn.Open();

                //call the overload that takes a connection in place of the connection string
                return ExecuteNonQuery(cn, commandType, commandText, commandParameters);
            }
        }

        public static int ExecuteNonQuery(SqlConnection connection, CommandType commandType, String commandText, params SqlParameter[] commandParameters)
        {
            //create a command and prepare it for execution
            SqlCommand cmd = new SqlCommand();
            cmd.CommandTimeout = 600;
            PrepareCommand(cmd, connection, null, commandType, commandText, commandParameters);

            //finally, execute the command.
            int retval = 0;
            try
            {
                retval = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                // detach the SqlParameters from the command object, so they can be used again.
                cmd.Parameters.Clear();
            }

            return retval;
        }

        public static DataSet ExecuteDataset(String connectionString, CommandType commandType, String commandText, Action<string> msgAction = null, params SqlParameter[] commandParameters)
        {
            //create & open a SqlConnection, and dispose of it after we are done.
            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                cn.Open();

                //call the overload that takes a connection in place of the connection string
                return ExecuteDataset(cn, commandType, commandText, msgAction, commandParameters);
            }
        }

        public static DataSet ExecuteDataset(SqlConnection connection, CommandType commandType, String commandText, Action<string> msgAction = null, params SqlParameter[] commandParameters)
        {
            //create a command and prepare it for execution
            SqlCommand cmd = new SqlCommand();
            PrepareCommand(cmd, connection, null, commandType, commandText, commandParameters);
            connection.InfoMessage += delegate(object sender, SqlInfoMessageEventArgs e)
            {
                if (msgAction != null)
                {
                    msgAction.Invoke(e.Message);
                }
            };
            //create the DataAdapter & DataSet
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();

            try
            {
                //fill the DataSet using default values for DataTable names, etc.
                da.Fill(ds);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                // detach the SqlParameters from the command object, so they can be used again.			
                cmd.Parameters.Clear();
            }

            //return the dataset
            return ds;
        }

        private enum SqlConnectionOwnership
        {
            /// <summary>Connection is owned and managed by SqlHelper</summary>
            Internal,
            /// <summary>Connection is owned and managed by the caller</summary>
            External
        }

        private static SqlDataReader ExecuteReader(SqlConnection connection, SqlTransaction transaction, CommandType commandType, String commandText, SqlParameter[] commandParameters, SqlConnectionOwnership connectionOwnership)
        {
            //create a command and prepare it for execution
            SqlCommand cmd = new SqlCommand();
            PrepareCommand(cmd, connection, transaction, commandType, commandText, commandParameters);

            //create a reader
            SqlDataReader dr;

            try
            {
                // call ExecuteReader with the appropriate CommandBehavior
                if (connectionOwnership == SqlConnectionOwnership.External)
                {
                    dr = cmd.ExecuteReader();
                }
                else
                {
                    dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                // detach the SqlParameters from the command object, so they can be used again.
                cmd.Parameters.Clear();
            }

            return dr;
        }

        public static SqlDataReader ExecuteReader(String connectionString, CommandType commandType, String commandText, params SqlParameter[] commandParameters)
        {
            var retry = 0;//重试次数
            var retryOut = 5;//最大重试次数
            var connected = false;
            SqlConnection cn = null;
            do
            {
                try
                {
                    //create & open a SqlConnection
                    cn = new SqlConnection(connectionString);
                    cn.Open();
                    connected = true;
                }
                catch
                {
                    retry++;
                    System.Threading.Thread.Sleep(1000);

                    if (retry > retryOut)
                    {
                        connected = false;
                        break;
                    }
                }
            } while (connected == false);
            if (connected == false)
            {
                return null;
            }

            try
            {
                //call the private overload that takes an internally owned connection in place of the connection string
                return ExecuteReader(cn, null, commandType, commandText, commandParameters, SqlConnectionOwnership.Internal);
            }
            catch (Exception ex)
            {
                //if we fail to return the SqlDatReader, we need to close the connection ourselves
                cn.Close();
                throw ex;
            }
        }

        public static object ExecuteScalar(String connectionString, CommandType commandType, String commandText, params SqlParameter[] commandParameters)
        {
            //create & open a SqlConnection, and dispose of it after we are done.
            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                cn.Open();

                //call the overload that takes a connection in place of the connection string
                return ExecuteScalar(cn, commandType, commandText, commandParameters);
            }
        }

        public static object ExecuteScalar(SqlConnection connection, CommandType commandType, String commandText, params SqlParameter[] commandParameters)
        {
            //create a command and prepare it for execution
            SqlCommand cmd = new SqlCommand();
            PrepareCommand(cmd, connection, null, commandType, commandText, commandParameters);

            //execute the command & return the results
            object retval = null;

            try
            {
                retval = cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                // detach the SqlParameters from the command object, so they can be used again.
                cmd.Parameters.Clear();
            }
            return retval;
        }

        /// <summary>
        /// 清空表 run sql truncate table
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tableName"></param>
        public static void TruncateTable(string connectionString, string tableName)
        {
            if (String.IsNullOrEmpty(tableName))
            {
                throw new Exception("Connection can't be null.");
            }
            else if (String.IsNullOrEmpty(tableName))
            {
                throw new Exception("TableName can't be null.");
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    TruncateTable(conn, tableName);
                }
            }
        }

        /// <summary>
        /// 清空表 run sql truncate table
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tableName"></param>
        public static void TruncateTable(SqlConnection conn, string tableName)
        {
            if (conn == null)
            {
                throw new Exception("SqlConnection can't be null.");
            }
            else if (String.IsNullOrEmpty(tableName))
            {
                throw new Exception("TableName can't be null.");
            }
            else
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();

                }
                using (SqlCommand cmd = new SqlCommand(String.Format("TRUNCATE TABLE {0}", tableName), conn))
                {
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }

        }

        public static void DorpTable(string[] tableNames, string connectionString)
        {
            if (tableNames != null && tableNames.Length > 0 && !String.IsNullOrEmpty(connectionString))
            {
                string sql = String.Format("drop table {0}", String.Join(",", tableNames));
                ExecuteNonQuery(connectionString, CommandType.Text, sql);
            }
        }
    }
}



