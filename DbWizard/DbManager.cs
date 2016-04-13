using DbWizard.CodeGenerate;
using DbWizard.Models;
using DbWizard.Utils;
using FastColoredTextBoxNS;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Trace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace DbWizard
{
    public partial class DbManager : Form
    {
        private string _connectionString;
        private DbConnection _dbConnection;
        private string[] sysDatabases = new string[] { "master", "model", "msdb", "tempdb" };
        private List<string> autocompletedKeys = new List<string>();
        private AutocompleteMenu popupMenu;
        private Generator _generator;
        private string _dbType = "SQLServer";
        private string _selectedDb = "master";
        private List<Trace> _traces = new List<Trace>();
        private Thread _traceThread = null;
        private volatile bool _stopTrace = false;

        private delegate void ShowRecordDelegate(Trace trace);

        public DbManager()
        {
            InitializeComponent();

            InitEditor();

            this._generator = new Generator();
            this._generator.DbType = _dbType;
            this.tsStatusProgressBar.Visible = true;
            this.dgvRecord.AutoGenerateColumns = false;

            this.dgvRecord.Columns.Add("EventClass", "EventClass");
            this.dgvRecord.Columns.Add("ObjectName", "ObjectName");
            this.dgvRecord.Columns.Add("TextData", "TextData");
            this.dgvRecord.Columns.Add("LoginName", "LoginName");
            this.dgvRecord.Columns.Add("Time", "Time");
        }

        /// <summary>
        /// 初始化编辑器
        /// </summary>
        private void InitEditor()
        {
            fctb.Language = FastColoredTextBoxNS.Language.SQL;
            fctb.OnSyntaxHighlight(new TextChangedEventArgs(fctb.Range));

            autocompletedKeys.AddRange(SqlKeys.AllKeys);
            popupMenu = new AutocompleteMenu(fctb);
            popupMenu.AllowTabKey = true;
            popupMenu.AppearInterval = 200;
            popupMenu.MinFragmentLength = 1;
            popupMenu.Items.SetAutocompleteItems(autocompletedKeys);
            popupMenu.Items.MaximumSize = new System.Drawing.Size(200, 300);
            popupMenu.Items.Width = 200;
        }

        #region 加载数据库
        void bgWorker_LoadDatabases(object sender, DoWorkEventArgs e)
        {
            e.Result = LoadDatabases();
        }

        void bgWorker_LoadDatabasesCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var databases = e.Result as List<DbObject>;

            if (databases == null || databases.Count == 0) return;

            var rootNode = new TreeNode(_dbConnection.Server);
            rootNode.ImageIndex = 0;
            rootNode.Nodes.Clear();
            tsDatabases.Items.Clear();
            foreach (var item in databases)
            {
                var node = new TreeNode(item.Name);
                node.Tag = item;
                node.ImageIndex = 1;
                rootNode.Nodes.Add(node);
                tsDatabases.Items.Add(item.Name);
            }
            this.dbTree.Nodes.Add(rootNode);
            this.dbTree.ExpandAll();

            tsStatusProgressBar.Value = 100;
            tsStatusProgressBar.Visible = false;
            bgWorker.DoWork -= bgWorker_LoadDatabases;
            bgWorker.RunWorkerCompleted -= bgWorker_LoadDatabasesCompleted;

            RefreshAutoCompletedKeys(databases.Select(t => t.Name));
        }
        #endregion

        #region 加载数据表
        void bgWorker_LoadTables(object sender, DoWorkEventArgs e)
        {
            var dict = e.Argument as Dictionary<string, string>;
            e.Result = LoadTables(dict["DbName"]);
        }

        void bgWorker_LoadTablesCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var tables = e.Result as List<DbObject>;
            if (tables == null || tables.Count == 0) return;

            var selectedNode = dbTree.SelectedNode;
            selectedNode.Nodes.Clear();
            foreach (var item in tables)
            {
                var node = new TreeNode(item.Name);
                node.Tag = item;
                node.ImageIndex = 2;
                selectedNode.Nodes.Add(node);
            }
            bgWorker.DoWork -= bgWorker_LoadTables;
            bgWorker.RunWorkerCompleted -= bgWorker_LoadTablesCompleted;
            dbTree.SelectedNode.Expand();

            RefreshAutoCompletedKeys(tables.Select(t => t.Name));
        }
        #endregion

        #region 加载数据列
        void bgWorker_LoadColumns(object sender, DoWorkEventArgs e)
        {
            var dict = e.Argument as Dictionary<string, string>;
            e.Result = LoadColumns(dict["DbName"], dict["TableName"]);
        }

        void bgWorker_LoadColumnsCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var columns = e.Result as List<DbObject>;
            if (columns == null || columns.Count == 0) return;

            var selectedNode = dbTree.SelectedNode;
            selectedNode.Nodes.Clear();
            foreach (var item in columns)
            {
                var node = new TreeNode(item.Name);
                node.Tag = item;
                node.ImageIndex = 3;
                selectedNode.Nodes.Add(node);
            }
            bgWorker.DoWork -= bgWorker_LoadColumns;
            bgWorker.RunWorkerCompleted -= bgWorker_LoadColumnsCompleted;
            dbTree.SelectedNode.Expand();

            RefreshAutoCompletedKeys(columns.Select(t => t.Name));
        }
        #endregion

        protected List<DbObject> LoadDatabases()
        {
            var databases = new List<DbObject>();
            var sql = @"SELECT  *
                               FROM    Master..SysDatabases
                               ORDER BY Name";
            using (var dr = SqlHelper.ExecuteReader(this._connectionString, CommandType.Text, sql))
            {
                if (dr != null)
                {
                    while (dr.Read())
                    {
                        var name = dr["name"] as String;
                        if (this.sysDatabases.Contains(name)) continue;
                        var dbObj = new DbObject
                        {
                            Name = name,
                            Type = DbObjectType.Database
                        };
                        databases.Add(dbObj);
                    }
                }
            }
            return databases;
        }

        protected List<DbObject> LoadTables(string dbName)
        {
            var tables = new List<DbObject>();
            var sql = String.Format(@"
                                                    SELECT  Name
                                                    FROM    [{0}]..SysObjects
                                                    WHERE   XType = 'U'
                                                    ORDER BY Name ", dbName);
            using (var dr = SqlHelper.ExecuteReader(this._connectionString, CommandType.Text, sql))
            {
                if (dr != null)
                {
                    while (dr.Read())
                    {
                        var name = dr["name"] as String;
                        var dbObj = new DbObject
                        {
                            Name = name,
                            Type = DbObjectType.Table
                        };
                        tables.Add(dbObj);
                    }
                }
            }
            return tables;
        }

        protected List<DbObject> LoadColumns(string dbName, string tableName)
        {
            var columns = new List<DbObject>();
            var sql = String.Format(@"
                                                    USE [{0}]
                                                    SELECT  (name+' ('+TYPE_NAME(xtype)+ (CASE TYPE_NAME(xtype) WHEN 'varchar' THEN '('+CAST([length] AS VARCHAR)+')' ELSE '' END) +', '+ CASE(isnullable) WHEN 0 THEN 'not null' WHEN 1 THEN 'null' END +')') AS name
                                                    FROM    SysColumns
                                                    WHERE   id = OBJECT_ID('{1}')
                                                    ORDER BY colorder ASC", dbName, tableName);
            using (var dr = SqlHelper.ExecuteReader(this._connectionString, CommandType.Text, sql))
            {
                if (dr != null)
                {
                    while (dr.Read())
                    {
                        var name = dr["name"] as String;
                        var dbObj = new DbObject
                        {
                            Name = name,
                            Type = DbObjectType.Column
                        };
                        columns.Add(dbObj);
                    }
                }
            }
            return columns;
        }

        private void BuildGrid(Panel panel, DataTable table)
        {
            var grid = new GridView(table.TableName, table);
            grid.Dock = DockStyle.Top;
            panel.Controls.Add(grid);
            panel.Controls.SetChildIndex(grid, 0);
        }

        private void ExcuteSql()
        {
            SetToolStripButtonStates(tsRun, false);

            var sql = fctb.SelectedText.Trim();
            if (String.IsNullOrEmpty(sql))
            {
                sql = fctb.Text.Trim();
            }
            sql = String.Format(" USE [{0}] {1} {2} ", tsDatabases.SelectedItem ?? "master", Environment.NewLine, sql);
            bgWorker.DoWork += bgWorker_ExecuteSql;
            bgWorker.RunWorkerCompleted += bgWorker_RunWorkerCompleted;
            bgWorker.RunWorkerAsync(sql);
        }

        private void AppendSqlToEditor(string sql)
        {
            var index = 0;
            var len = sql.Length;
            var text = this.fctb.Text;
            if (String.IsNullOrEmpty(text))
            {
                text = sql;
            }
            else
            {
                text = String.Format("{0}{1}", text, Environment.NewLine);
                index = text.Length;
                text = String.Format("{0}{1}", text, sql);
            }
            this.fctb.Text = text;
            this.fctb.SelectionStart = index;
            this.fctb.SelectionLength = len;

            ExcuteSql();
        }

        /// <summary>
        /// 刷新自动提示的单词
        /// </summary>
        private void RefreshAutoCompletedKeys(IEnumerable<string> newKeys)
        {
            foreach (string item in newKeys)
            {
                if (autocompletedKeys.Exists(t => t.Equals(item, StringComparison.CurrentCultureIgnoreCase)) == false)
                {
                    autocompletedKeys.Add(item.ToUpper());
                }
            }
            popupMenu.Items.SetAutocompleteItems(autocompletedKeys);
        }

        public void SetToolStripButtonStates(ToolStripButton btn, bool enable)
        {
            if (enable)
            {
                btn.Enabled = true;
                btn.Image = btn.Tag as Image;
                btn.Tag = null;
                btn.ToolTipText = "";
            }
            else
            {
                btn.Enabled = false;
                btn.Tag = btn.Image;
                btn.Image = global::DbWizard.Properties.Resources.Loading;
                btn.ToolTipText = "正在处理，请稍后";
            }
        }

        #region 执行SQL
        void bgWorker_ExecuteSql(object sender, DoWorkEventArgs e)
        {
            var sql = e.Argument as String;
            var result = new Result();
            try
            {
                var dt = SqlHelper.ExecuteDataset(this._connectionString, CommandType.Text, sql, delegate (string msg)
                {
                    result.Message += msg + Environment.NewLine;
                });
                result.Status = ResultStatus.Success;
                result.Data = dt;
            }
            catch (Exception ex)
            {
                result.Status = ResultStatus.Failure;
                result.Message = ex.Message;
            }
            finally
            {
                e.Result = result;
            }
        }
        void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            pResult.Controls.Clear();
            var result = e.Result as Result;
            if (result != null)
            {
                if (result.Status == ResultStatus.Success)
                {
                    var dt = result.Data as DataSet;
                    if (dt != null && dt.Tables != null && dt.Tables.Count > 0)
                    {
                        foreach (DataTable table in dt.Tables)
                        {
                            BuildGrid(pResult, table);
                        }
                        if (dt.Tables.Count == 1)
                        {
                            pResult.Controls[0].Dock = DockStyle.Fill;
                        }
                    }
                    tbMsg.ForeColor = Color.Black;
                    tabResult.SelectedIndex = 0;
                }
                else if (result.Status == ResultStatus.Failure)
                {
                    tbMsg.ForeColor = Color.Red;
                    tabResult.SelectedIndex = 1;
                }
                tbMsg.Text = result.Message;
            }
            bgWorker.DoWork -= bgWorker_ExecuteSql;
            bgWorker.RunWorkerCompleted -= bgWorker_RunWorkerCompleted;

            SetToolStripButtonStates(tsRun, true);
        }
        #endregion

        private void dbTree_DoubleClick(object sender, EventArgs e)
        {
            var node = dbTree.SelectedNode;
            var tag = node.Tag as DbObject;
            if (tag != null)
            {
                if (tag.LoadCompleted == true) return;
                var dict = new Dictionary<string, string>();
                if (tag.Type == DbObjectType.Database)
                {
                    dict.Add("DbName", tag.Name);
                    bgWorker.DoWork += bgWorker_LoadTables;
                    bgWorker.RunWorkerCompleted += bgWorker_LoadTablesCompleted;
                }
                else if (tag.Type == DbObjectType.Table)
                {
                    var parentNode = node.Parent;
                    var parentTag = parentNode.Tag as DbObject;
                    if (parentTag != null)
                    {
                        dict.Add("DbName", parentTag.Name);
                    }
                    dict.Add("TableName", tag.Name);
                    bgWorker.DoWork += bgWorker_LoadColumns;
                    bgWorker.RunWorkerCompleted += bgWorker_LoadColumnsCompleted;
                }
                else if (tag.Type == DbObjectType.Column) { }
                bgWorker.RunWorkerAsync(dict);
            }
        }

        private void tsRun_Click(object sender, EventArgs e)
        {
            ExcuteSql();
        }

        private void DbManager_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                ExcuteSql();
            }
        }

        //单击树节点选中对应的库
        private void dbTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var tag = e.Node.Tag as DbObject;
            if (tag != null)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (tag.Type == DbObjectType.Database)
                    {
                        //选中当前库
                        tsDatabases.SelectedItem = _selectedDb = tag.Name;
                    }
                }
                else
                {
                    if (tag.Type == DbObjectType.Table)
                    {
                        var contextMenu = new ContextMenu();
                        var menuItem = new MenuItem("选择前100行");
                        menuItem.Click += delegate (object _sender, EventArgs _e)
                        {
                            var sql = String.Format(@"SELECT TOP 100 * FROM {0}", tag.Name);
                            AppendSqlToEditor(sql);
                        };
                        contextMenu.MenuItems.Add(menuItem);

                        var menuItem2 = new MenuItem("生成代码");
                        menuItem2.Click += delegate (object _sender, EventArgs _e)
                        {
                            _generator.ConnectionString = _dbConnection.GetString(_selectedDb);
                            _generator.Generate(new string[] { tag.Name });
                        };
                        contextMenu.MenuItems.Add(menuItem2);

                        contextMenu.Show(dbTree, new Point(e.X, e.Y));
                    }
                    else if (tag.Type == DbObjectType.Database)
                    {
                        var contextMenu = new ContextMenu();
                        var menuItem = new MenuItem("生成代码");
                        menuItem.Click += delegate (object _sender, EventArgs _e)
                        {
                            var tables = (from t in LoadTables(tag.Name)
                                          select t.Name
                                             ).ToArray();
                            _generator.ConnectionString = _dbConnection.GetString(tag.Name);
                            _generator.Generate(tables);
                        };
                        contextMenu.MenuItems.Add(menuItem);
                        contextMenu.Show(dbTree, new Point(e.X, e.Y));
                    }
                    else if (tag.Type == DbObjectType.Column)
                    {
                        var contextMenu = new ContextMenu();
                        var menuItem = new MenuItem("复制");
                        menuItem.Click += delegate (object _sender, EventArgs _e)
                        {
                            var name = tag.Name.Substring(0, tag.Name.IndexOf("(")).Trim();
                            Clipboard.SetDataObject(name);
                        };
                        contextMenu.MenuItems.Add(menuItem);
                        contextMenu.Show(dbTree, new Point(e.X, e.Y));
                    }
                }
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            var frm = new FrmConnection();
            if (frm.ShowDialog() == DialogResult.OK)
            {
                this._dbConnection = frm.Connection;
                this._connectionString = frm.Connection.ToString();
                this.bgWorker.DoWork += bgWorker_LoadDatabases;
                this.bgWorker.RunWorkerCompleted += bgWorker_LoadDatabasesCompleted;
                this.bgWorker.RunWorkerAsync();


            }
            frm.Dispose();
        }

        private void Trace()
        {
            try
            {
                SqlConnectionInfo connInfo = new SqlConnectionInfo();
                if (_dbConnection.IntegratedSecurity)
                {
                    connInfo.UseIntegratedSecurity = true;
                }
                else
                {
                    connInfo.ServerName = _dbConnection.Server;
                    connInfo.UserName = _dbConnection.UserID;
                    connInfo.Password = _dbConnection.Password;
                }

                TraceServer trace = new TraceServer();
                trace.InitializeAsReader(connInfo, @"trace.tdf");


                while (trace.Read())
                {
                    if (!_stopTrace)
                    {
                        var record = new Trace(trace);
                        this.dgvRecord.Invoke(new ShowRecordDelegate(AddTrace), record);
                    }
                }
            }
            catch (ThreadAbortException e) { }
        }

        private void AddTrace(Trace trace)
        {
            _traces.Add(trace);
            this.dgvRecord.Rows.Add(new object[] {
                trace.EventClass,
                trace.ObjectName,
                trace.TextData,
                trace.LoginName,
                trace.StartTime
            });
        }

        private void dgvRecord_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var contextMenu = new ContextMenu();
                var menuItem = new MenuItem("清空历史");
                menuItem.Click += delegate (object _sender, EventArgs _e)
                {
                    dgvRecord.Rows.Clear();
                };
                contextMenu.MenuItems.Add(menuItem);

                var hit = dgvRecord.HitTest(e.X, e.Y);
                if (hit.RowIndex != -1 && hit.ColumnIndex != -1)
                {

                    menuItem = new MenuItem("复制");
                    menuItem.Click += delegate (object _sender, EventArgs _e)
                    {
                        var text = dgvRecord.Rows[hit.RowIndex].Cells[hit.ColumnIndex].Value as String;
                        Clipboard.SetDataObject(text);
                    };
                    contextMenu.MenuItems.Add(menuItem);
                }
                contextMenu.Show(dgvRecord, new Point(e.X, e.Y));
            }
        }

        private void tsbRefresh_Click(object sender, EventArgs e)
        {
            dbTree.Nodes.Clear();

            this.bgWorker.DoWork += bgWorker_LoadDatabases;
            this.bgWorker.RunWorkerCompleted += bgWorker_LoadDatabasesCompleted;
            this.bgWorker.RunWorkerAsync();
        }

        //关于对话框
        private void tsAbout_Click(object sender, EventArgs e)
        {
            var dialog = new About();
            dialog.ShowDialog();
        }

        //开始跟踪
        private void tsbStartRecord_Click(object sender, EventArgs e)
        {
            if (_traceThread == null)
            {
                _traceThread = new Thread(new ThreadStart(Trace));
                _traceThread.IsBackground = true;
            }
            _stopTrace = false;
            if (!_traceThread.IsAlive)
            {
                _traceThread.Start();
            }

            tsbStartRecord.Enabled = false;
            tsbStopRecord.Enabled = true;
        }

        //停止跟踪
        private void tsbStopRecord_Click(object sender, EventArgs e)
        {
            try
            {
                _stopTrace = true;
            }
            catch
            {

            }
            finally
            {
                tsbStartRecord.Enabled = true;
                tsbStopRecord.Enabled = false;
            }
        }

        //清除跟踪历史
        private void tsbClearRecord_Click(object sender, EventArgs e)
        {
            dgvRecord.Rows.Clear();
        }

        private void fctb_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var contextMenu = new ContextMenu();
                var item = new MenuItem("复制");
                item.Tag = "copy";
                item.Click += Item_Click;
                contextMenu.MenuItems.Add(item);
                contextMenu.Show(fctb, e.Location);
            }
        }

        private void Item_Click(object sender, EventArgs e)
        {
            var item = sender as MenuItem;
            var cmd = item.Tag as String;

            switch (cmd)
            {
                case "copy":
                    var selectText = fctb.SelectedText;
                    if (String.IsNullOrEmpty(selectText)) return;
                    Clipboard.SetText(selectText);
                    break;
            }
        }

        private void fctb_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Tab)
            {
            }
        }
    }
}
