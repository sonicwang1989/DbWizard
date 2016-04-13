using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

using DbWizard.Models;
using DbWizard.Utils;

namespace DbWizard
{
    public partial class FrmConnection : Form
    {
        [Description("单击确定事件")]
        public event EventHandler OnOpen;
        [Description("单击取消事件")]
        public event EventHandler OnCancel;

        private List<DbConnection> _history = new List<DbConnection>();

        public DbConnection Connection { get; set; }

        public FrmConnection()
        {
            InitializeComponent();

            this.Connection = new DbConnection();
        }

        private void FrmConnection_Load(object sender, EventArgs e)
        {
            LoadHistory();
        }

        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(cbServers.Text))
            {
                MessageBox.Show("请输入或选择服务器！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnTestConnection.Enabled = false;
            btnTestConnection.Cursor = Cursors.WaitCursor;
            string connectionString = GetConnectionString();
            SqlDataReader dr = null;
            try
            {
                using (dr = SqlHelper.ExecuteReader(connectionString, CommandType.Text, "select top 1 1 from  sys.databases"))
                {
                    if (dr != null && dr.Read())
                    {
                        MessageBox.Show("连接成功", "系统", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        btnCreate.Enabled = true;
                        tbNewDatabaseName.Enabled = true;
                    }
                    else
                    {
                        MessageBox.Show(@"连接失败，请检查用户名\密码是否正确", "系统", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
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
                    cbServers.Text,
                    tbUsername.Text,
                    tbPassword.Text);
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
                if (!btnCreate.Enabled)
                {
                    btnCreate.Enabled = true;
                }
            }
            else
            {
                btnCreate.Enabled = false;
            }
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            btnCreate.Enabled = false;
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
            btnCreate.Enabled = true;
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            this.Connection.Server = this.cbServers.SelectedItem as String ?? this.cbServers.Text;
            this.Connection.InitialCatalog = "master";
            if (rbWindowAuth.Checked)
            {
                this.Connection.IntegratedSecurity = true;
            }
            else if (rbSqlServerAuth.Checked)
            {
                this.Connection.UserID = tbUsername.Text;
                this.Connection.Password = tbPassword.Text;
            }

            var _connection = this._history.Find(t => t.Server.Equals(this.Connection.Server, StringComparison.CurrentCultureIgnoreCase));
            if (_connection == null)
            {
                this._history.Add(this.Connection);
            }
            else {
                if (rbSqlServerAuth.Checked) {
                    _connection.IntegratedSecurity = false;
                    _connection.UserID = tbUsername.Text;
                    _connection.Password = tbPassword.Text;
                }
                else if (rbWindowAuth.Checked) {
                    _connection.IntegratedSecurity = true;
                    _connection.UserID = "";
                    _connection.Password = "";
                }
            }
            SaveHistory();

            if (this.OnOpen != null)
            {
                this.OnOpen(sender, e);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (this.OnCancel != null)
            {
                this.OnCancel(sender, e);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                btnOpen.PerformClick();
                return true;
            }
            else if (keyData == Keys.Escape)
            {
                btnCancel.PerformClick();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// 保存历史记录
        /// </summary>
        protected void SaveHistory()
        {
            var formatter = new BinaryFormatter();
            using (var file = File.Create("history.dat"))
            {
                formatter.Serialize(file, _history);
            }
        }

        protected void LoadHistory()
        {
            var formatter = new BinaryFormatter();
            try
            {
                using (var fs = File.Open("history.dat", FileMode.Open, FileAccess.Read))
                {
                    _history = formatter.Deserialize(fs) as List<DbConnection>;
                }
            }
            catch
            {
                _history = new List<DbConnection>();
            }

            if (_history.Count > 0)
            {
                cbServers.DisplayMember = "Server";
                foreach (var conn in _history)
                {
                    cbServers.Items.Add(conn);
                }
            }
        }


        private void btnBrowser_Click(object sender, EventArgs e)
        {
            var sb = new ServerBrowser();
            var result = sb.ShowDialog();
            if (result == DialogResult.OK)
            {
                cbServers.Text = sb.Server;
            }
            sb.Dispose();
        }

        private void cbServers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_history == null || _history.Count == 0) return;
            var server = cbServers.Text;
            if (String.IsNullOrEmpty(server)) return;

            var historyItem = _history.Find(t => t.Server.Equals(server, StringComparison.CurrentCultureIgnoreCase));
            if (historyItem == null) return;

            if (historyItem.IntegratedSecurity == false)
            {
                rbSqlServerAuth.Checked = true;
                tbUsername.Text = historyItem.UserID;
                tbPassword.Text = historyItem.Password;
            }
            else
            {
                rbWindowAuth.Checked = true;
                tbUsername.Text = "";
                tbPassword.Text = "";
            }
        }
    }
}
