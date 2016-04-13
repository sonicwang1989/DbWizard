using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbWizard.Utils
{
    public class DateHelper
    {
        public static string GetDate(string format = "yyyy-MM-dd")
        {
            return GetDate(DateTime.Now, format);
        }

        public static string GetDate(DateTime input, string format = "yyyy-MM-dd")
        {
            return input.ToString(format);
        }

        public static string GetTime(string format = "hh24:mm:ss")
        {
            return DateTime.Now.ToString(format);
        }

        public static string GetDateTime(string format = "yyyy-MM-dd HH:mm:ss")
        {
            return DateTime.Now.ToString(format);
        }
    }
}
