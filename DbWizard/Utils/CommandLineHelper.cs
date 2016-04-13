using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

using DbWizard.Models;

namespace DbWizard.Utils
{
    /// <summary>
    /// 调用命令行辅助类
    /// </summary>
    public class CommandLineHelper
    {
        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="command">命令名</param>
        /// <param name="waitForExit">是否等待命令执行完</param>
        /// <returns></returns>
        public static Result Execute(string command, bool waitForExit = false)
        {
            Result result = new Result();
            if (String.IsNullOrEmpty(command))
            {
                result.Status = ResultStatus.Failure;
                result.AppendMessage("Comand can't be empty!");
            }
            else
            {
                try
                {
                    Process p = new Process();
                    p.StartInfo = new ProcessStartInfo("cmd.exe");
                    p.StartInfo.Arguments = String.Format(" /C {0} ", command);
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.RedirectStandardError = true;
                    p.Start();

                    result.Status = ResultStatus.Success;
                    result.Data = p.StandardOutput.ReadToEnd();
                    result.AppendMessage(p.StandardError.ReadToEnd());

                    if (waitForExit)
                    {
                        p.WaitForExit();
                    }
                }
                catch (Exception ex)
                {
                    result.Status = ResultStatus.Failure;
                    result.AppendMessage("Message:{0}{1}StackTrace:{2}", ex.Message, Environment.NewLine, ex.StackTrace);
                }
                string[] array = new string[] { "The command completed successfully.", "These Windows services are started:" };
            }
            return result;
        }
    }
}
