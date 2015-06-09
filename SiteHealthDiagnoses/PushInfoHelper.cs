using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using System.Threading;
using MongoDB.Driver.Builders;
using System.Configuration;
namespace Yinhe.ProcessingCenter.SystemHealth
{
     
 
    /// <summary>
    /// 消息推送助手
    /// </summary>
    public class PushInfoHelper
    {

        /// <summary>
        /// 开始运行
        /// </summary>
        private void startFunction(string constr, string FunctionClassIds)
        {
              
            if (!string.IsNullOrEmpty(constr)&&!string.IsNullOrEmpty(FunctionClassIds) != null)
            {
                
                    var classIds = FunctionClassIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(c=>(BsonValue)c);
               
                    List<BsonDocument> hitFunctionList = new List<BsonDocument>();
               
                    var dataOp = new DataOperation(new MongoOperation(constr));
                    hitFunctionList = dataOp.FindAllByQuery("SystemHealthMessageHandleStore", Query.In("classId", classIds)).ToList();
                    if (hitFunctionList.Count() > 0)
                    {
                        MessageInfoFactory.Instance.PushInfo(hitFunctionList, constr);
                    }
            }
         }

        
    }

}
