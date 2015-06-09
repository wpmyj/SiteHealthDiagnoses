using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace AutoSystemHealtyDiagnoses
{
    class Program
    {
        static void Main(string[] args)
        {
            var helper = new PushInfoHelper();
            //NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();
            //_log.Info("开始运行");
            //try
            //{
            //    helper.startFunction(DataBaseConnectionString, FunctionClassIds);
            //    _log.Info("无异常");
            //    //执行父父目录的自动更新程序
            //    var curDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            //    if (curDir != null && curDir.Parent != null)
            //    {
            //        var hitWEBSiteUpdate = curDir.Parent.GetFiles().Where(c => c.Name == "WEBSiteUpdate.exe").FirstOrDefault();
            //        if (hitWEBSiteUpdate != null)
            //        {
            //            var thread=new Thread(delegate(){
            //             ExecProcess(hitWEBSiteUpdate.FullName);
            //            });
            //            thread.Start();
                        
            //        }
            //    }
            //    return;
            //}
            //catch (Exception ex)
            //{
            //    _log.Info(ex.Message);
            //}
            //_log.Info("运行结束");
            helper.GoogleSTT();
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

                if (ConfigurationSettings.AppSettings["DataBaseConnectionString"] != null)
                {
                    return ConfigurationSettings.AppSettings["DataBaseConnectionString"];
                }
                else
                {
                    return "";
                }
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
    }
}
