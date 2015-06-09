using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using System.Threading;
using MongoDB.Driver.Builders;
using System.Configuration;
using Yinhe.ProcessingCenter;
using Yinhe.ProcessingCenter.SystemHealth;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Json;
namespace AutoSystemHealtyDiagnoses
{
     
 
    /// <summary>
    /// 消息推送助手
    /// </summary>
    public class PushInfoHelper
    {

        /// <summary>
        /// 开始运行
        /// </summary>
        public void startFunction(string constr, string FunctionClassIds)
        {
            NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();
          
            
                if (!string.IsNullOrEmpty(constr) && !string.IsNullOrEmpty(FunctionClassIds) != null)
                {

                    var classIds = FunctionClassIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(c => (BsonValue)c);

                    List<BsonDocument> hitFunctionList = new List<BsonDocument>();

                    var dataOp = new DataOperation(new MongoOperation(constr));
                  try
                     {
                    hitFunctionList = dataOp.FindAllByQuery("SystemHealthMessageHandleStore", Query.In("classId", classIds)).ToList();

                      }
                  catch (MongoDB.Driver.MongoConnectionException ex)
                  {
                      _log.Info(ex.Message);
                  }
                  catch (MongoDB.Driver.MongoException ex)
                  {
                      _log.Info(ex.Message);
                  }
                      if (hitFunctionList.Count() > 0)
                    {
                        MessageInfoFactory.Instance.PushInfo(hitFunctionList, constr);
                    }
                }
         
          
         }


        /// <summary>
        /// 富文本类型
        /// 
        /// </summary>
        public class RichTextMessage
        {
            /// <summary>
            /// 地址
            /// </summary>
            public string detailurl { get; set; }
            /// <summary>
            /// 图片
            /// </summary>
            public string icon { get; set; }

            /// <summary>
            /// 开始时间
            /// </summary>
            public string starttime { get; set; }
            /// <summary>
            /// 结束时间
            /// </summary>
            public string endtime { get; set; }

            ///-----------------新闻类
            /// <summary>
            /// 主题
            /// </summary>
            public string article { get; set; }
            /// <summary>
            /// 来源
            /// </summary>
            public string source { get; set; }


            ///-----------------列车类
            /// <summary>
            /// 车次
            /// </summary>
            public string trainnum { get; set; }
            /// <summary>
            /// 起点
            /// </summary>
            public string start { get; set; }
            /// <summary>
            /// 终点
            /// </summary>
            public string terminal { get; set; }


            ///-----------------列车类
            /// <summary>
            /// 航班
            /// </summary>
            public string flight { get; set; }
            /// <summary>
            /// 路由
            /// </summary>
            public string route { get; set; }

            ///-----------------菜谱类code	 状态码
            /// <summary>
            /// 菜谱名
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// 信息
            /// </summary>
            public string info { get; set; }
        }

        public class TulingMessage
        {
            /// <summary>
            /// 代码100000	 文本类数据
            //305000	 列车
            //306000	 航班
            //200000	 网址类数据
            //302000	 新闻
            //308000	 菜谱、视频、小说
            //40001	 key的长度错误（32位）
            //40002	 请求内容为空
            //40003	 key错误或帐号未激活
            //40004	 当天请求次数已用完
            //40005	 暂不支持该功能
            //40006	 服务器升级中
            //40007	 服务器数据格式异常
            /// </summary>
            public string code { get; set; }
            /// <summary>
            /// 文本
            /// </summary>
            public string text { get; set; }
            /// <summary>
            /// url
            /// </summary>
            public string url { get; set; }

            /// <summary>
            /// 列表,新闻类，航班，菜谱
            /// </summary>
            public List<RichTextMessage> list { get; set; }

        }
        public string GetTulingMessage(string info)
        {


            var Url = string.Format("http://www.tuling123.com/openapi/api?key={0}&info={1}&userid=96266", "7d9bcbeb4a3fe11ccc35c755480fb50a", info);
            var retString = string.Empty;
            try
            {
                var returnInfo = getPageInfo(Url);
                if (returnInfo != null)
                {
                    var ser = new DataContractJsonSerializer(typeof(TulingMessage));
                    var ms = new MemoryStream(Encoding.UTF8.GetBytes(returnInfo));
                    TulingMessage sn = (TulingMessage)ser.ReadObject(ms);
                    if (sn != null)
                    {
                        switch (sn.code)
                        {
                            //普通文本
                            case "100000": retString = sn.text; break;
                            //链接类
                            case "200000":
                            //航班类
                            case "306000":
                                retString = string.Format("{0}查看地址为{1}", sn.text, sn.url); break;
                            //新闻
                            case "302000":

                                retString = sn.text;
                              
                                break;
                            //菜谱视频小说
                            case "308000":

                                retString = sn.text;
                               
                                break;
                            // 列车
                            case "305000": retString = sn.text;


                                retString = sn.text;
                              
                                break;
                            default: retString = sn.text; break;
                        }


                    }
                }
            }
            catch (ThreadInterruptedException ex)
            {
                // ReportErrors(ex.Message, "PushQueue");
                return retString;
            }
            catch (TimeoutException ex)
            {
                // ReportErrors(ex.Message, "PushQueue");
                return retString;
            }
            catch (HttpListenerException ex)
            {
                // ReportErrors(ex.Message, "PushQueue");
                return retString;
            }
            catch (Exception ex)
            {
                //  Yinhoo.Framework.Log.LogWarpper._().PushApplicationException(ex);
                //ReportErrors(ex.Message, "PushQueue");
                return retString;
            }
            return retString;
        }
        /// <summary>
        /// 获取url数据
        /// </summary>
        /// <param name="strUrl"></param>
        /// <returns></returns>
        public static string getPageInfo(string strUrl)
        {
            // 构建一个请求
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strUrl);
            // 请求的方式
            request.Method = "GET";

            // 请求的响应
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            // 响应的流
            Stream responseStream = response.GetResponseStream();

            // 字符编码
            Encoding enc = Encoding.GetEncoding("utf-8");

            // 读取流
            StreamReader readResponseStream = new StreamReader(responseStream, enc);

            // 请求的结果
            string result = readResponseStream.ReadToEnd();

            // 关闭流,响应,释放资源
            readResponseStream.Close();
            response.Close();

            return result;

        }


        #region google语音识别

        /// <summary>
        /// 调用GOOLE语音识别引擎
        /// </summary>
        /// <returns></returns>
        public  string GoogleSTT()
        {
            string result = string.Empty;
            try
            {
                var key = "AIzaSyAuDjHINVhTjH21yz42yVTSXNSpjxhg4zk";
                string inFile = "4.flac";
                FileStream fs = new FileStream(inFile, FileMode.Open);
                byte[] voice = new byte[fs.Length];
                fs.Read(voice, 0, voice.Length);
                fs.Close();
                HttpWebRequest request = null;
                //  string url = string.Format("http://www.google.com/speech-api/v1/recognize?xjerr=1&client=chromium&lang=zh-cn&maxresults=1&key={0}", key);
                string url = string.Format("https://www.google.com/speech-api/v2/recognize?xjerr=1&client=chromium&output=json&lang=zh-tw&key={0}&maxresults=1", key);
                //string url = string.Format("https://www.google.com/speech-api/v2/recognize?output=json&xjerr=1&client=chromium&lang=zh-cn&maxresults=1&key={0}", key);
   
                Uri uri = new Uri(url);
                request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = "POST";
                 request.ContentType = "audio/x-flac; rate=16000";
               // request.ContentType = "audio/L16; rate=16000";
                request.ContentLength = voice.Length;
                using (Stream writeStream = request.GetRequestStream())
                {
                    writeStream.Write(voice, 0, voice.Length);
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (StreamReader readStream = new StreamReader(responseStream, Encoding.UTF8))
                        {
                            result = readStream.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            return result;
        }
        #endregion

    }

}
