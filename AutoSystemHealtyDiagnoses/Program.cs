using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Xml;

namespace AutoSystemHealtyDiagnoses
{
    class Program
    {
        static void Main(string[] args)
        {
            var helper = new PushInfoHelper();
            NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();
            _log.Info("开始运行");
            try
            {
                helper.startFunction(DataBaseConnectionString, FunctionClassIds);
                _log.Info("无异常");
              
            }
            catch (Exception ex)
            {
                _log.Info(ex.Message);
            }
            _log.Info("运行结束");
            //helper.GoogleSTT();
            //执行父父目录的自动更新程序,失败也会更新
            var curDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            if (curDir != null && curDir.Parent != null)
            {
                var hitWEBSiteUpdate = curDir.Parent.GetFiles().Where(c => c.Name == "WEBSiteUpdate.exe").FirstOrDefault();
                if (hitWEBSiteUpdate != null)
                {
                    var thread = new Thread(delegate()
                    {
                        ExecProcess(hitWEBSiteUpdate.FullName);
                    });
                    thread.Start();

                }
            }
            try
            {
                //更新配置文件主控地址
                UpdateConfig("DataBaseConnectionString", "");
                //更新mongodb://sa:dba@XXXXXXX/WorkPlanManage 为mongodb://MZsa:(MZdba)@XXXXXXX:37088/WorkPlanManage
                 
                if (!OuterConnectionStr.Contains("37088")&&!string.IsNullOrEmpty(OuterConnectionStr) && !string.IsNullOrEmpty(CustomerCode))
                {
                    var resultConStr = ChangeCustomerConnection(OuterConnectionStr, CustomerCode);
                    if (!string.IsNullOrEmpty(resultConStr))
                    {
                        UpdateConfig("OuterConnectionStr", resultConStr);
                    }
                }
            }
            catch (Exception ex)
            { 
            }
            return;
        }
        /// <summary>
        /// 根据用户代码修改客户对应的连接字符串
        /// </summary>
        /// <param name="OuterConnectionStr"></param>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        private static string ChangeCustomerConnection(string OuterConnectionStr, string customerCode)
        { 
       
            Dictionary<string, string> customerDic = new Dictionary<string, string>(){
                {"F8A3250F-A433-42be-9F68-803BBF01ZHHY", "ZHHY"},
                {"6F9619FF-8B86-D011-B42D-00C04FC964SN", "SN"},
                {"71E8DBA3-5DC6-4597-9DCD-F3CC1F04FCXH", "XH"},
                {"73345DB5-DFE5-41F8-B37E-7D83335AZHTZ", "ZHTZ"},
                {"84C7D7E3-26C2-479F-B67F-F240E506CEQX", "QX"},
                {"84C7D7E3-26C2-479F-B67F-F240E506QXSD", "QX"},
                {"4DD74057-DDF4-4533-AFE8-51AC263B05LF", "LF"},
                {"4BF8120C-DB2C-495D-8BC2-FD9189E8NJHY", "NJHY"},
                {"802812B4-B670-48C2-9E20-F9954CA65CXC", "XC"},
                {"958AEDDF-04F0-4702-B5F6-FC300262F96D", "SS"},
                {"6B47BD15-0400-1622-0250-39E3DB0411JH", "JH"},
                {"5D8A608E-85A6-45C3-A3FE-E3B24623DWPM", ""},
                {"DE548D75-FC95-40CB-B6AB-A0E9E8FF78ZY", "ZY"},
                {"638351E2-104A-4B27-945B-B8F2740BA10ANF", "NF"},
                {"90E53D26-A7EF-4DA0-A1A6-BDD82E29JZQH", "JZQH"},
                {"AB421821-B862-45B9-84CA-0DB02AD0FB1APUBMAT", "PUBMAT"},
                {"037EC46A-629E-420E-B04A-00446F3AZJGK", "ZJGK"},
                {"39FC0333-3CC3-459C-B67A-994E97E7ACPM",""},
                {"A647B10F-D0E5-4134-BFAE-6A47F0E5CPIM",""},
                {"2DD5DE1B-378F-4D0E-95A4-8CF08DE654LR","LR"},
                {"CAF6F384-A001-4717-B625-E6D1971BZBCG",""},
                {"AB65BF72-02EF-4338-9109-F1EC36D0JHGL",""},
                {"BE8E3DA5-637F-4409-8F3B-DE820FBXHNEW","XH"},
                {"6E890E12-AB2D-415E-9F22-8C79F20FE3LG","LG"},
                {"1CBA2D85-A12A-4752-BD3B-2A9B1376A3JD","JD"},
                {"F9DE05F7-A00B-4AAD-9FA1-2EC6A7HQCNEW","HQC"}
            };
            try
            {
                if (customerDic.ContainsKey(customerCode))
                {
                    //mongodb://sa:dba@XXXXXXX/WorkPlanManage 为mongodb://MZsa:(MZdbaQX)@XXXXXXX:37088/WorkPlanManage
                    var symbol = customerDic[customerCode];
                    var newPassWord = string.Format("MZdba");
                    if (!string.IsNullOrEmpty(symbol))
                    {
                        newPassWord = string.Format("(MZdba{0})", symbol);
                    }
                    
                    //var userName = GetStr(OuterConnectionStr, "mongodb://", ":");
                    //var passWord = GetStr(OuterConnectionStr, userName+":", "@");

                    var ip = GetStr(OuterConnectionStr, "@", "/").Replace(":37088", "");
                    var dataBaseIndex = OuterConnectionStr.LastIndexOf("/");
                    if (dataBaseIndex != -1)
                    {
                        var dataBaseName = OuterConnectionStr.Substring(dataBaseIndex+1, OuterConnectionStr.Length - dataBaseIndex-1);
                        var result = string.Format("mongodb://{0}:{1}@{2}:37088/{3}", "MZsa", newPassWord, ip, dataBaseName);
                        return result;
                    }
                }
            }
            catch (Exception ex)
            { }
            return string.Empty;
        }
        /// <summary>
        /// 获取间隔字符串
        /// </summary>
        /// <param name="curStr"></param>
        /// <param name="beginStr"></param>
        /// <param name="endStr"></param>
        /// <returns></returns>
        private static string GetStr(string curStr,string beginStr,string endStr)
        {
            var beginIndex = curStr.IndexOf(beginStr);
            var endIndex = curStr.IndexOf(endStr, beginIndex + beginStr.Length);
            if (beginIndex != -1 && endIndex!=-1)
            {
                var result = curStr.Substring(beginIndex + beginStr.Length, endIndex - beginIndex - beginStr.Length);
                return result;
            }
            return string.Empty;
        }
        /// <summary>
        /// 更新配置文件
        /// </summary>
        /// <param name="name"></param>
        /// <param name="Xvalue"></param>
        private static void UpdateConfig(string name, string Xvalue)
        {

            var defaultPath = AppDomain.CurrentDomain.BaseDirectory + "/SiteHealthDiagnoses.exe.config";
            if (System.IO.File.Exists(defaultPath))
            {
                UpdateConfig(defaultPath, name, Xvalue);
            }
            #region 同步更新对应控制台中的配置文件
            //   var dir = new DirectoryInfo(Application.StartupPath);

            var consoleDefaultPath = AppDomain.CurrentDomain.BaseDirectory + "/AutoSystemHealtyDiagnoses.exe.config";
            if (System.IO.File.Exists(consoleDefaultPath))
            {
                UpdateConfig(consoleDefaultPath, name, Xvalue);
            }
            #endregion
        }



        private static void UpdateConfig(string configPath, string name, string Xvalue)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(configPath);
            XmlNode node = doc.SelectSingleNode(@"//add[@key='" + name + "']");
            XmlElement ele = (XmlElement)node;
            ele.SetAttribute("value", Xvalue);
            doc.Save(configPath);
        }

        /// <summary>
        /// 执行可执行文件
        /// </summary>
        /// <param name="exeFilePath"></param>
        /// <param name="Arguments"></param>
        public static string ExecProcess(string exeFilePath, string Arguments = "")
        {

            // 执行exe文件
            Process process = new Process();
            process.StartInfo.FileName = exeFilePath;
            // 不显示闪烁窗口
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WorkingDirectory = Path.GetDirectoryName(exeFilePath);
            // 注意，参数需用引号括起来，因为路径中可能有空格
            if (!string.IsNullOrEmpty(Arguments))
            {
                process.StartInfo.Arguments = Arguments;
            }
            try
            {
                process.Start();


            }
            catch (OutOfMemoryException ex)
            {
                return ex.Message;

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                if (process != null)
                    process.Close();

            }
            return string.Empty;
        }

        /// <summary>
        /// 功能列表
        /// </summary>
        public static string DataBaseConnectionString
        {
            get
            {
                return "mongodb://MZsa:MZdba@59.61.72.34:37088/WorkPlanManage";
                //if (ConfigurationSettings.AppSettings["DataBaseConnectionString"] != null)
                //{
                //    return ConfigurationSettings.AppSettings["DataBaseConnectionString"];
                //}
                //else
                //{
                //    return "mongodb://MZsa:MZdba@59.61.72.34:37088/WorkPlanManage";
                //}
            }
        }

        /// <summary>
        /// 功能列表
        /// </summary>
        public static string FunctionClassIds
        {
            get
            {

                if (ConfigurationSettings.AppSettings["FunctionClassIds"] != null)
                {
                    return ConfigurationSettings.AppSettings["FunctionClassIds"];
                }
                else
                {
                    return "";
                }
            }
        }
        /// <summary>
        /// 数据库链接
        /// </summary>
        public static string OuterConnectionStr
        {
            get
            {

                if (ConfigurationSettings.AppSettings["OuterConnectionStr"] != null)
                {
                    return ConfigurationSettings.AppSettings["OuterConnectionStr"];
                }
                else
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// 客户
        /// </summary>
        public static string CustomerCode
        {
            get
            {

                if (ConfigurationSettings.AppSettings["CustomerCode"] != null)
                {
                    return ConfigurationSettings.AppSettings["CustomerCode"];
                }
                else
                {
                    return "";
                }
            }
        }
        
    }
}
