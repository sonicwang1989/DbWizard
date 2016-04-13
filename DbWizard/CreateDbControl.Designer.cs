namespace DbWizard
{
    partial class CreateDbControl
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tbPassword = new System.Windows.Forms.TextBox();
            this.tbUsername = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.rbSqlServerAuth = new System.Windows.Forms.RadioButton();
            this.rbWindowAuth = new System.Windows.Forms.RadioButton();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.cbServers = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.tbNewDatabaseName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.bgWorker = new System.ComponentModel.BackgroundWorker();
            this.btnTestConnection = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tbPassword);
            this.groupBox1.Controls.Add(this.tbUsername);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.rbSqlServerAuth);
            this.groupBox1.Controls.Add(this.rbWindowAuth);
            this.groupBox1.Location = new System.Drawing.Point(26, 119);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(354, 197);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "登陆到服务器";
            // 
            // tbPassword
            // 
            this.tbPassword.Enabled = false;
            this.tbPassword.Location = new System.Drawing.Point(103, 130);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.PasswordChar = '*';
            this.tbPassword.Size = new System.Drawing.Size(219, 21);
            this.tbPassword.TabIndex = 5;
            // 
            // tbUsername
            // 
            this.tbUsername.Enabled = false;
            this.tbUsername.Location = new System.Drawing.Point(103, 93);
            this.tbUsername.Name = "tbUsername";
            this.tbUsername.Size = new System.Drawing.Size(219, 21);
            this.tbUsername.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(47, 134);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 12);
            this.label4.TabIndex = 3;
            this.label4.Text = "密 码：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(45, 97);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "用户名：";
            // 
            // rbSqlServerAuth
            // 
            this.rbSqlServerAuth.AutoSize = true;
            this.rbSqlServerAuth.Location = new System.Drawing.Point(19, 59);
            this.rbSqlServerAuth.Name = "rbSqlServerAuth";
            this.rbSqlServerAuth.Size = new System.Drawing.Size(167, 16);
            this.rbSqlServerAuth.TabIndex = 1;
            this.rbSqlServerAuth.Text = "使用 SQL Server 身份验证";
            this.rbSqlServerAuth.UseVisualStyleBackColor = true;
            this.rbSqlServerAuth.CheckedChanged += new System.EventHandler(this.AuthMethod_CheckedChanged);
            // 
            // rbWindowAuth
            // 
            this.rbWindowAuth.AutoSize = true;
            this.rbWindowAuth.Checked = true;
            this.rbWindowAuth.Location = new System.Drawing.Point(19, 36);
            this.rbWindowAuth.Name = "rbWindowAuth";
            this.rbWindowAuth.Size = new System.Drawing.Size(149, 16);
            this.rbWindowAuth.TabIndex = 0;
            this.rbWindowAuth.TabStop = true;
            this.rbWindowAuth.Text = "使用 Windows 身份验证";
            this.rbWindowAuth.UseVisualStyleBackColor = true;
            this.rbWindowAuth.CheckedChanged += new System.EventHandler(this.AuthMethod_CheckedChanged);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(305, 82);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 23);
            this.btnRefresh.TabIndex = 12;
            this.btnRefresh.Text = "刷新";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // cbServers
            // 
            this.cbServers.FormattingEnabled = true;
            this.cbServers.Location = new System.Drawing.Point(24, 84);
            this.cbServers.Name = "cbServers";
            this.cbServers.Size = new System.Drawing.Size(271, 20);
            this.cbServers.TabIndex = 11;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 12);
            this.label2.TabIndex = 10;
            this.label2.Text = "请选择服务器名：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(359, 12);
            this.label1.TabIndex = 9;
            this.label1.Text = "输入信息以连接到 SQL Server，然后指定要创建的数据库的名称。";
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(306, 398);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 17;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            this.btnOk.Enabled = false;
            this.btnOk.Location = new System.Drawing.Point(209, 398);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 16;
            this.btnOk.Text = "确定";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // tbNewDatabaseName
            // 
            this.tbNewDatabaseName.Enabled = false;
            this.tbNewDatabaseName.Location = new System.Drawing.Point(26, 354);
            this.tbNewDatabaseName.Name = "tbNewDatabaseName";
            this.tbNewDatabaseName.Size = new System.Drawing.Size(354, 21);
            this.tbNewDatabaseName.TabIndex = 15;
            this.tbNewDatabaseName.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbNewDatabaseName_KeyUp);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(26, 333);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(89, 12);
            this.label5.TabIndex = 14;
            this.label5.Text = "新数据库名称：";
            // 
            // btnTestConnection
            // 
            this.btnTestConnection.Location = new System.Drawing.Point(26, 398);
            this.btnTestConnection.Name = "btnTestConnection";
            this.btnTestConnection.Size = new System.Drawing.Size(75, 23);
            this.btnTestConnection.TabIndex = 18;
            this.btnTestConnection.Text = "测试连接";
            this.btnTestConnection.UseVisualStyleBackColor = true;
            this.btnTestConnection.Click += new System.EventHandler(this.btnTestConnection_Click);
            // 
            // CreateDbControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnTestConnection);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.cbServers);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.tbNewDatabaseName);
            this.Controls.Add(this.label5);
            this.MaximumSize = new System.Drawing.Size(410, 447);
            this.MinimumSize = new System.Drawing.Size(410, 447);
            this.Name = "CreateDbControl";
            this.Size = new System.Drawing.Size(410, 447);
            this.Load += new System.EventHandler(this.CreateDbControl_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox tbPassword;
        private System.Windows.Forms.TextBox tbUsername;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RadioButton rbSqlServerAuth;
        private System.Windows.Forms.RadioButton rbWindowAuth;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.ComboBox cbServers;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.TextBox tbNewDatabaseName;
        private System.Windows.Forms.Label label5;
        private System.ComponentModel.BackgroundWorker bgWorker;
        private System.Windows.Forms.Button btnTestConnection;
    }
}
