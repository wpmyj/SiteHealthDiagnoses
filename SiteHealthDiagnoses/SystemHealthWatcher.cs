using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MongoDB.Bson;
using Yinhe.ProcessingCenter;
using Yinhe.ProcessingCenter.SystemHealth;
using System.Configuration;
using System.Xml;
using MongoDB.Driver.Builders;
using System.IO;
namespace SiteHealthDiagnoses
{
    public partial class SystemHealthWatcher : Form
    {
        public SystemHealthWatcher()
        {
            InitializeComponent();
        }

      
        List<BsonDocument> allFunctionList = new List<BsonDocument>();
      
        private List<string> classIds = new List<string>();//需要运行的功能列表

        private void SystemWatcher_Load(object sender, EventArgs e)
        {
            

            InitialAppParam();
            RefleshFunction(false);
        }
        

        //刷新功能列表
        public void RefleshFunction(bool createDefaule)
        {
           
            var constr = this.conStr.Text;
            var curCustomerCode = string.Empty;
            if (this.CustomerCodeCmb.SelectedItem!= null)
            {
                curCustomerCode = this.CustomerCodeCmb.SelectedItem.ToString();
            }
           
            if (!string.IsNullOrEmpty(constr))
            {
                var dataOp = new DataOperation(new MongoOperation(constr));
                try
                {
                    if (!string.IsNullOrEmpty(curCustomerCode))
                    {
                        allFunctionList = dataOp.FindAll("SystemHealthMessageHandleStore").Where(c => string.IsNullOrEmpty(c.Text("customerCode")) || c.Text("customerCode") == curCustomerCode).ToList();
                    }
                    else
                    {
                        allFunctionList = dataOp.FindAll("SystemHealthMessageHandleStore").ToList();
                    }
                }

                catch (MongoDB.Driver.MongoConnectionException ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
                catch (MongoDB.Driver.MongoException ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }

                if (createDefaule&&allFunctionList.Count() <= 0)
                {
                    if (this.CustomerCodeCmb.SelectedItem == null)
                    {
                        MessageBox.Show("请输入客户代码并进行初始化");
                        return;
                    }
                    var addBsonList = new List<BsonDocument>();
                    var firstBson = new BsonDocument().Add("name", "系统登录0次报警").Add("isActive", "1").Add("tranClass", "Yinhe.ProcessingCenter.SystemHealth.SystemLoginStatic").Add("type","0");
                    if (this.CustomerCodeCmb.SelectedItem != null)
                    {
                        firstBson.Add("customerCode", this.CustomerCodeCmb.SelectedItem.ToString());
                    }
                    addBsonList.Add(firstBson);
                    var secdBson = new BsonDocument().Add("name", "文件上传失败与切图失败推送").Add("isActive", "1").Add("tranClass", "Yinhe.ProcessingCenter.SystemHealth.FileUploadErrorStatic").Add("type", "0");
                    if (this.CustomerCodeCmb.SelectedItem != null)
                    {
                        secdBson.Add("customerCode", this.CustomerCodeCmb.SelectedItem.ToString());
                    }
                    addBsonList.Add(secdBson);
                    var result = dataOp.QuickInsert("SystemHealthMessageHandleStore", addBsonList);
                    if (result.Status == Status.Successful)
                    {
                        allFunctionList = dataOp.FindAll("SystemHealthMessageHandleStore").ToList();
                    }
                }
                this.functionListCtrl.Items.Clear();
                foreach (var function in allFunctionList)
                {
                    if (classIds.Contains(function.Text("classId")))
                    {
                        var index=this.functionListCtrl.Items.Add(function.Text("name"),true);
                        this.functionListCtrl.SetItemChecked(index, true);
                    }
                    else
                    {
                        this.functionListCtrl.Items.Add(function.Text("name"));
                    }
                   
                }
                
            }

        }
        /// <summary>
        /// 初始化系统参数
        /// </summary>
        private void InitialAppParam()
        {
            if (!string.IsNullOrEmpty(DataBaseConnectionString))
            {
                this.conStr.Text = DataBaseConnectionString;
            }
            if (!string.IsNullOrEmpty(OuterConnectionStr))
            {
                this.outerConnection.Text = OuterConnectionStr;
            }
            if (!string.IsNullOrEmpty(SysAppConfig.BugPushUrl))
            {
                this.pushUrlText.Text = SysAppConfig.BugPushUrl;
            }

            if(!string.IsNullOrEmpty(SysAppConfig.CustomerCode))
            {
                foreach(var item in CustomerCodeCmb.Items)
                {
                   if(item.ToString()==SysAppConfig.CustomerCode)
                   {
                       CustomerCodeCmb.SelectedItem = item;
                       break;
                   }
                
                }
                 
            }

             if (!string.IsNullOrEmpty(FunctionClassIds) != null)
             {
                 classIds = FunctionClassIds.Split(new string[]{","}, StringSplitOptions.RemoveEmptyEntries).ToList();
             }
            
        
        }

        private void start_Click(object sender, EventArgs e)
        {
            try
            {
                startFunction();
              //  SaveConfigBtn_Click(sender, e);
            }
            catch (IOException ex)
            {
                this.label2.Text = ex.Message;
            }
            catch (InvalidCastException ex)
            {
                this.label2.Text = ex.Message;
            }
            catch (SystemException ex)
            {
                this.label2.Text = ex.Message; 
            }
            
            catch (Exception ex)
            {
                this.label2.Text = ex.Message; 
            }
        }

        /// <summary>
        /// 开始运行
        /// </summary>
        private void startFunction()
        {

            List<BsonDocument> hitFunctionList = new List<BsonDocument>();
            //开始执行事务
            if (this.functionListCtrl.CheckedItems.Count > 0)
            {
                foreach (var item in this.functionListCtrl.CheckedItems)
                {
                    var hitFunction = allFunctionList.Where(c => c.Text("name") == item.ToString()).FirstOrDefault();
                    if (hitFunction != null)
                    {
                        hitFunctionList.Add(hitFunction);
                    }

                }

            }

            if (hitFunctionList.Count() > 0)
            {
                MessageInfoFactory.Instance.PushInfo(hitFunctionList, this.conStr.Text,this.outerConnection.Text);
            }

            this.label2.Text = "异步执行任务";

            
        }


        


        /// <summary>
        /// 更新配置文件
        /// </summary>
        /// <param name="name"></param>
        /// <param name="Xvalue"></param>
        private void UpdateConfig(string name, string Xvalue)
        {
            var defaultPath = Application.ExecutablePath + ".config";
            if (System.IO.File.Exists(defaultPath))
            {
                UpdateConfig(defaultPath, name, Xvalue);
            }
            #region 同步更新对应控制台中的配置文件
         //   var dir = new DirectoryInfo(Application.StartupPath);

            var consoleDefaultPath = Application.StartupPath + "/AutoSystemHealtyDiagnoses.exe.config";
            if (System.IO.File.Exists(consoleDefaultPath))
            {
                UpdateConfig(consoleDefaultPath, name, Xvalue);
            }
            #endregion
        }
     


        private void UpdateConfig(string configPath,string name, string Xvalue)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(configPath);
            XmlNode node = doc.SelectSingleNode(@"//add[@key='" + name + "']");
            XmlElement ele = (XmlElement)node;
            ele.SetAttribute("value", Xvalue);
            doc.Save(configPath);
        }

        /// <summary>
        /// 数据库连接串
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
        /// 数据库连接串
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
        /// 是否拥有自动更新功能
        /// </summary>
        public static string HasAutoUpdate
        {
            get
            {

                if (ConfigurationSettings.AppSettings["HasAutoUpdate"] != null)
                {
                    return ConfigurationSettings.AppSettings["HasAutoUpdate"];
                }
                else
                {
                    return "";
                }
            }
        }


        private void SaveConfigBtn_Click(object sender, EventArgs e)
        {

            try
            {
                if (this.functionListCtrl.CheckedItems.Count > 0)
                {
                    classIds = new List<string>();
                    foreach (var item in this.functionListCtrl.CheckedItems)
                    {
                        var hitFunction = allFunctionList.Where(c => c.Text("name") == item.ToString()).FirstOrDefault();
                        if (hitFunction != null)
                        {
                            var classId = hitFunction.Text("classId");
                            classIds.Add(classId);
                        }

                    }
                    var classIdsStr = string.Join(",", classIds);
                    UpdateConfig("FunctionClassIds", classIdsStr);

                }
                else
                {
                    UpdateConfig("FunctionClassIds", "");
                }
                UpdateConfig("DataBaseConnectionString", conStr.Text);
                UpdateConfig("BugPushUrl", this.pushUrlText.Text);
                UpdateConfig("OuterConnectionStr", this.outerConnection.Text);
                if (this.CustomerCodeCmb.SelectedIndex > -1)
                {
                    UpdateConfig("CustomerCode", this.CustomerCodeCmb.SelectedItem.ToString());
                }
                else
                {
                    MessageBox.Show("请先选择客户代码");
                    return;
                }
                this.label2.Text = "配置保存成功";
            }
            catch (Exception ex)
            {
                this.label2.Text = ex.Message;
            }

        }

        private void refesh_Click(object sender, EventArgs e)
        {
            RefleshFunction(true);
        }

    }
}
