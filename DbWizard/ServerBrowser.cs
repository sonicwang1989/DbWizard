using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DbWizard.Utils;
using DbWizard.Models;

namespace DbWizard
{
    public partial class ServerBrowser : Form
    {
        private string _server;

        /// <summary>
        /// 表示选中的服务器
        /// </summary>
        public string Server
        {
            get
            {
                return _server;
            }
        }

        public ServerBrowser()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
        }

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //获取所有的Sql Server 服务器
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
            e.Result = servers;
        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.dgv.Rows.Clear();

            var servers = e.Result as string[];
            if (servers == null || servers.Length == 0) return;

            foreach (var server in servers)
            {
                this.dgv.Rows.Add(new object[] { server });
            }
            ClearSelectedRows();
        }

        private void ServerBrowser_Load(object sender, EventArgs e)
        {
            btnRefresh.PerformClick();
        }

        private void dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var value = this.dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value as String;
            if (String.IsNullOrEmpty(value)) return;
            value = value.Trim();

            if (value == "-- 无 --" || value == "正在查找网络中的服务器...") return;
            this._server = value;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (this.bgWorker.IsBusy) return;
            this.dgv.Rows.Clear();
            this.dgv.Rows.Add(new object[] { "正在查找网络中的服务器..." });
            ClearSelectedRows();
            this.bgWorker.RunWorkerAsync();
        }

        private void ClearSelectedRows() {
            if (this.dgv.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in this.dgv.SelectedRows)
                {
                    row.Selected = false;
                }
            }
        }

        private void ServerBrowser_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == DialogResult.OK)
            {
                if (String.IsNullOrEmpty(this.Server))
                {
                    MessageBox.Show("请选择服务器！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Cancel = true;
                }
            }
        }
    }
}
