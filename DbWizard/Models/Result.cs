using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbWizard.Models
{
    public class Result
    {
        public ResultStatus Status { get; set; }

        public string Message { get; set; }

        public object Data { get; set; }

        public Result() { }

        public Result(ResultStatus status, string msg, object data)
        {
            this.Status = status;
            this.Message = msg;
            this.Data = data;
        }

        public void AppendMessage(string msg, params object[] args)
        {
            this.Message = String.Format("{0}{1}{2}",
                this.Message,
                Environment.NewLine,
                String.Format(msg, args));
        }
    }

    public enum ResultStatus
    {
        Success = 0,
        Failure = 1
    }
}
