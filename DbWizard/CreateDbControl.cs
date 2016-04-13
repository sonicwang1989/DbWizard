using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Windows.Forms;
using Borui.Common;
using Borui.Common.Diagnostics;
using Borui.Common.Sql;

namespace DbWizard
{
    public partial class CreateDbControl : UserControl
    {
        public CreateDbControl()
        {
            InitializeComponent();
        }

        private void CreateDbControl_Load(object sender, EventArgs e)
        {
            BindServers();
        }

        void bgWorker_ListServer(object sender, DoWorkEventArgs e)
        {
            e.Result = GetServers();
        }

        void bgWorker_ListServer_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                string[] servers = e.Result as string[];
                if (servers != null && servers.Length > 0)
                {
                    cbServers.Items.Clear();
                    foreach (string item in servers)
                    {
                        cbServers.Items.Add(item.Trim());
                    }
                    cbServers.SelectedIndex = 0;
                }
            }

            bgWorker.DoWork -= bgWorker_ListServer;
            bgWorker.RunWorkerCompleted -= bgWorker_ListServer_Completed;
            tbNewDatabaseName.Enabled = true;
            cbServers.Cursor = Cursors.Default;
        }

        protected void BindServers()
        {
            if (bgWorker.IsBusy)
            {
                MessageBox.Show("操作正在进行中，请稍后", "系统", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                cbServers.Cursor = Cursors.WaitCursor;
                cbServers.Items.Add("正在查找网络中的服务器，请稍后");
                cbServers.SelectedIndex = 0;
                bgWorker.DoWork += bgWorker_ListServer;
                bgWorker.RunWorkerCompleted += bgWorker_ListServer_Completed;
                bgWorker.RunWorkerAsync();
            }
        }

        /// <summary>
        /// 获取所有的Sql Server 服务器
        /// </summary>
        /// <returns></returns>
        private string[] GetServers()
        {
            string[] servers = null;
            Result result = CommandLineHelper.Execute("osql.exe -L");
            if (result.Status == ResultStatus.Success)
            {
                if (result.Data != null)
                {
                    string serversStr = result.Data.ToString();
                    if (!String.IsNullOrEmpty(serversStr))
                    {
                        serversStr = serversStr.Replace("服务器:", "").Replace("Servers:", "");
                        servers = serversStr.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    }
                }
            }
            return servers;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            BindServers();
        }

        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            btnTestConnection.Enabled = false;
            btnTestConnection.Cursor = Cursors.WaitCursor;
            string connectionString = GetConnectionString();
            SqlDataReader dr = null;
            try
            {
                dr = SqlHelper.ExecuteReader(connectionString, CommandType.Text, "select name from  sys.databases");
                if (dr.Read())
                {
                    MessageBox.Show("连接成功", "系统", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(@"连接失败，请检查用户名\密码是否正确", "系统", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "系统", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (dr != null && !dr.IsClosed)
                {
                    dr.Close();
                }
            }
            btnTestConnection.Cursor = Cursors.Default;
            btnTestConnection.Enabled = true;
        }

        public string GetConnectionString()
        {
            string connectionString = string.Empty;
            if (rbWindowAuth.Checked)
            {
                connectionString = String.Format("Data Source={0};Initial Catalog=master;Integrated Security=True", cbServers.Text);
            }
            else if (rbSqlServerAuth.Checked)
            {
                connectionString = String.Format("Data Source={0};Initial Catalog=master;User ID={1};Password={2};",
                    cbServers.SelectedItem.ToString(), tbUsername.Text, tbPassword.Text);
            }
            return connectionString;
        }

        private void AuthMethod_CheckedChanged(object sender, EventArgs e)
        {
            if (rbWindowAuth.Checked)
            {
                tbUsername.Enabled = false;
                tbPassword.Enabled = false;
            }
            else if (rbSqlServerAuth.Checked)
            {
                tbUsername.Enabled = true;
                tbPassword.Enabled = true;
            }
        }

        private void tbNewDatabaseName_KeyUp(object sender, KeyEventArgs e)
        {
            var input = tbNewDatabaseName.Text;
            if (!String.IsNullOrEmpty(input) && input.Length > 0)
            {
                if (!btnOk.Enabled)
                {
                    btnOk.Enabled = true;
                }
            }
            else
            {
                btnOk.Enabled = false;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            btnOk.Enabled = false;
            string dbName = tbNewDatabaseName.Text;
            try
            {
                SqlHelper.ExecuteNonQuery(this.GetConnectionString(),
                    CommandType.Text,
                    String.Format("create database {0}", dbName));
                MessageBox.Show(String.Format("创建数据库{0}成功", dbName), "系统");
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("创建数据库{0}失败{1}{2}", dbName, Environment.NewLine, ex.Message), "系统");
            }
            btnOk.Enabled = true;
        }
    }
}
