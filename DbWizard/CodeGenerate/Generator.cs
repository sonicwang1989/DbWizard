using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DbWizard.Models;
using DbWizard.Utils;
using DbWizard.Properties;

namespace DbWizard.CodeGenerate
{
    public class Generator
    {
        public Table CurrentTable { get; set; }
        public string ModuleNameSpace { get; set; }
        public string IDALNameSpace { get; set; }
        public string DALNameSpace { get; set; }
        public string BillNameSpace { get; set; }

        string TemplatePath;
        string sPathModel;
        string sPathIDAL;
        string sPathDAL;
        string sPathIBLL;
        string sPathBLL;
        string OutputPath;

        private static string ToStr(string str, string sDefault = "")
        {
            return string.IsNullOrEmpty(str) ? sDefault : str;
        }

        public Generator()
        {
            TemplatePath = Environment.CurrentDirectory + "\\Template\\";
            OutputPath = Environment.CurrentDirectory + "\\AutoGenerate";
            sPathModel = TemplatePath + "Model.txt";
            sPathIDAL = TemplatePath + "IDAL.txt";
            sPathDAL = TemplatePath + "DAL.txt";
            sPathIBLL = TemplatePath + "IBLL.txt";
            sPathBLL = TemplatePath + "BLL.txt";

            try
            {
                if (Directory.Exists(OutputPath))
                {
                    Directory.Delete(OutputPath, true);
                }
                Directory.CreateDirectory(OutputPath);
            }
            catch { }
        }

        public string DbType { get; set; }

        public string ConnectionString { get; set; }

        public string GenModel()
        {
            var TModel = ReadTxtFile(sPathModel);
            var RegField = new Regex(@"(^\s*#FieldsStart#\s*$)([\s\S]*)(^\s*#FieldsEnd#)", RegexOptions.Multiline);
            var Temp = RegField.Match(TModel).Groups[2].Value.Trim();

            var sField = "";
            var sSpaces = Environment.NewLine + string.Empty.PadLeft(8, ' ') + "{0}";
            var padLeft = String.Empty.PadLeft(8, ' ');
            foreach (var TS in CurrentTable.TableSchemas)
            {
                if (TS.IsIdentity) sField += String.Format("{0}{1}{2}", padLeft, "[Identity]", Environment.NewLine);
                if (TS.IsPrimaryKey) sField += String.Format("{0}{1}{2}", padLeft, "[PrimaryKey]", Environment.NewLine);
                if (TS.IsTimestamp) sField += String.Format("{0}{1}{2}", padLeft, "[Ignore]", Environment.NewLine);
                sField += padLeft + Temp.Replace("{FieldType}", TS.TypeName).Replace("{FieldName}", TS.ColumnName);
                sField += Environment.NewLine + Environment.NewLine;
            }
            TModel = TModel.Replace("{TableName}", CurrentTable.TableName);
            TModel = RegField.Replace(TModel, sField.TrimEnd());
            return TModel;
        }

        public string GenIDAL()
        {
            var TIDAL = ReadTxtFile(sPathIDAL);
            TIDAL = TIDAL.Replace("{TableName}", CurrentTable.TableName);
            return TIDAL;
        }

        public string GenDAL()
        {
            var TDAL = ReadTxtFile(sPathDAL);
            TDAL = TDAL.Replace("{TableName}", CurrentTable.TableName);
            return TDAL;
        }

        public string GenIBLL()
        {
            var TIBLL = ReadTxtFile(sPathIBLL);
            TIBLL = TIBLL.Replace("{TableName}", CurrentTable.TableName);
            return TIBLL;
        }

        public string GenBLL()
        {
            var TBLL = ReadTxtFile(sPathBLL);
            TBLL = TBLL.Replace("{TableName}", CurrentTable.TableName);
            return TBLL;
        }

        public string ReadTxtFile(string FilePath)
        {
            var content = string.Empty;//返回的字符串
            using (var fs = new FileStream(FilePath, FileMode.Open))
            {
                using (var reader = new StreamReader(fs, Encoding.UTF8))
                {
                    string text = string.Empty;
                    while (!reader.EndOfStream)
                    {
                        text += reader.ReadLine() + "\r\n";
                        content = text;
                    }
                }
            }
            return content;
        }

        public void Generate(string tableName)
        {
            this.CurrentTable = GenTables.GetTables(tableName, this.DbType, this.ConnectionString);

            var sModel = GenModel();
            var sIDAL = GenIDAL();
            var sDAL = GenDAL();
            var sIBLL = GenIBLL();
            var sBLL = GenBLL();

            string sPathModel = OutputPath + "\\Model";
            string sPathIDAL = OutputPath + "\\IDAL";
            string sPathDAL = OutputPath + "\\DAL";
            string sPathIBLL = OutputPath + "\\IBLL";
            string sPathBLL = OutputPath + "\\BLL";

            if (!Directory.Exists(sPathModel)) Directory.CreateDirectory(sPathModel);
            if (!Directory.Exists(sPathIDAL)) Directory.CreateDirectory(sPathIDAL);
            if (!Directory.Exists(sPathDAL)) Directory.CreateDirectory(sPathDAL);
            if (!Directory.Exists(sPathIBLL)) Directory.CreateDirectory(sPathIBLL);
            if (!Directory.Exists(sPathBLL)) Directory.CreateDirectory(sPathBLL);

            //输出到文件中 model
            CreateFile(String.Format("{0}\\{1}.cs", sPathModel, this.CurrentTable.TableName), sModel);
            //输出到文件中 idal
            CreateFile(String.Format("{0}\\IDAL_{1}.cs", sPathIDAL, this.CurrentTable.TableName), sIDAL);
            //输出到文件中 dal
            CreateFile(String.Format("{0}\\DAL_{1}.cs", sPathDAL, this.CurrentTable.TableName), sDAL);
            //输出到文件中 ibll
            CreateFile(String.Format("{0}\\IBLL_{1}.cs", sPathIBLL, this.CurrentTable.TableName), sIBLL);
            //输出到文件中 bll
            CreateFile(String.Format("{0}\\BLL_{1}.cs", sPathBLL, this.CurrentTable.TableName), sBLL);
        }

        public void Generate(string[] tables)
        {
            if (tables != null && tables.Length > 0)
            {
                foreach (var table in tables)
                {
                    Generate(table);
                }
            }
            System.Diagnostics.Process.Start(OutputPath);
        }

        private void CreateFile(string filename, string content)
        {
            var tmp = String.Format("{0}{1}{1}{2}",
                Resources.AutoGenerateInfomation,
                Environment.NewLine, content)
                .Replace("#Runtime#", Environment.Version.ToString())
                .Replace("#AppName#", AppDomain.CurrentDomain.FriendlyName)
                .Replace("#Date#", DateTime.Now.ToString("yyyy-MM-dd"));

            File.WriteAllText(filename, tmp, Encoding.UTF8);
        }
    }
}
