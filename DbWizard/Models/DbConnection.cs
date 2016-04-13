using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbWizard.Models
{
    [Serializable]
    public class DbConnection
    {
        public DbConnection()
        {
        }

        public DbConnection(string server, int port, string initialCatalog, bool integratedSecurity)
        {
            this.Server = server;
            this.Port = port;
            this.InitialCatalog = initialCatalog;
            this.IntegratedSecurity = integratedSecurity;
        }

        public DbConnection(string server, string initialCatalog, string userid, string password)
            : this(server, 1433, initialCatalog, userid, password)
        { }

        public DbConnection(string server, int port, string initialCatalog, string userid, string password)
        {
            this.Server = server;
            this.Port = port;
            this.InitialCatalog = initialCatalog;
            this.UserID = userid;
            this.Password = password;
        }

        /// <summary>
        /// 服务器
        /// </summary>
        public string Server { get; set; }
        /// <summary>
        /// 初始库
        /// </summary>
        public string InitialCatalog { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserID { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 集成认证
        /// </summary>
        public bool IntegratedSecurity { get; set; }

        public override string ToString()
        {
            return GetString(this.InitialCatalog);
        }

        public string GetString(string catalog)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Data Source={0}", this.Server);
            if (this.Port != 0)
            {
                sb.AppendFormat(",{0}", this.Port);
            }
            sb.Append(";");
            if (String.IsNullOrEmpty(this.InitialCatalog) == false)
            {
                sb.AppendFormat("Initial Catalog={0};", catalog);
            }
            if (this.IntegratedSecurity)
            {
                sb.Append("Integrated Security=true;");
            }
            else
            {
                sb.AppendFormat("User ID={0};", this.UserID);
                sb.AppendFormat("Password={0};", this.Password);
            }
            return sb.ToString();
        }
    }
}
