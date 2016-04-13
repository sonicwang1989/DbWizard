using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.SqlServer.Management.Trace;

namespace DbWizard.Models
{
    public class Trace
    {
        public string EventClass { get; set; }

        public string TextData { get; set; }

        public string ApplicationName { get; set; }

        public string NTUserName { get; set; }
        public string LoginName { get; set; }

        public string CPU { get; set; }

        public string Reads { get; set; }

        public string Writes { get; set; }

        public string Duration { get; set; }

        public string ClientProcessID { get; set; }

        public string SPID { get; set; }

        public string ObjectName { get; set; }

        public string ObjectType { get; set; }

        public string StartTime { get; set; }

        public string EndTime { get; set; }

        public Trace() { }

        public Trace(TraceServer ts)
        {
            this.EventClass = ts["EventClass"] as String;
            this.Duration = ts["Duration"] as String;
            this.LoginName = ts["SessionLoginName"] as String;
            this.TextData = (ts["TextData"] as String ?? "").Trim();
            this.ObjectName = ts["ObjectName"] as String;
            this.ObjectType = ts["ObjectType"] as String;
            this.ClientProcessID = ts["SPID"] as String;
            this.StartTime = DateTime.Now.ToString("yyyy-MM-dd HH:MM:ss");
        }
    }
}
