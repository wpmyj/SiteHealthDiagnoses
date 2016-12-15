using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using System.IO;
using System.Net;
using MySql.Data.MySqlClient;
using Yinhe.ProcessingCenter;
using Yinhe.ProcessingCenter.DataRule;
using Yinhe.ProcessingCenter.SynAD;
using Yinhe.ProcessingCenter.Helper;
using System.IO.Compression;
using System.Collections;
using Helpers;
using System.Reflection;
using DotNet.Utilities;
using System.Threading.Tasks;
using System.Threading;
using SimpleCrawler;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Web;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using MongoDB.Driver;
using SimpleCrawler.Demo;
using org.in2bits.MyXls;
using System.Security.Cryptography;
//using Yinhe.ProcessingCenter.QuestionAnswer;


namespace DayLifeDataInitialFromDNFTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string connStr = "mongodb://sa:dba@192.168.1.230/WorkPlanManage";
            DataOperation operation = new DataOperation(new MongoOperation(connStr));
            string[] source = "job,revert,flavor_text,set_name,fullset_basic_explain,fullset_detail_explain,rarity".Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string rarity = "4";
            DataTable table = this.getmysqlAd("  select * from dnf_item_info where rarity=" + rarity);
            int num = 1;
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("it_name", "name");
            dictionary.Add("it_explain", "remark");
            string str4 = rarity;
            string str8 = rarity;
            if ((str8 != null) && (str8 == "4"))
            {
                str4 = "5";
            }
            List<StorageData> list = new List<StorageData>();
            foreach (DataRow row in table.Rows)
            {
                BsonDocument bsonDoc = new BsonDocument();
                bsonDoc.Add("rarity", str4);
                foreach (object column in table.Columns)
                {
                    if (!source.Contains<string>(column.ToString()))
                    {
                        string name = column.ToString();
                        string dbStr = row[column.ToString()].ToString();
                        string value = Microsoft.VisualBasic.Strings.StrConv(this.DBStringToNormal(dbStr), Microsoft.VisualBasic.VbStrConv.SimplifiedChinese, 0).Replace("%%", "%");
                        if (dictionary.ContainsKey(column.ToString()))
                        {
                            name = dictionary[column.ToString()];
                        }
                        if (!string.IsNullOrEmpty(dbStr))
                        {
                            bsonDoc.Add(name, value);
                        }
                    }
                }
                if (!((string.IsNullOrEmpty(bsonDoc.Text("name")) || bsonDoc.Text("name").Contains("古老")) || bsonDoc.Text("name").Contains("网咖")))
                {
                    StorageData item = new StorageData
                    {
                        Document = bsonDoc,
                        Name = "Item",
                        Type = StorageType.Insert
                    };
                    list.Add(item);
                    this.richTextBox1.Text = this.richTextBox1.Text + bsonDoc.Text("name");
                    this.richTextBox1.Text = this.richTextBox1.Text + "\n\r";
                    num++;
                }
            }
            if (list.Count<StorageData>() > 0)
            {
                //operation.BatchSaveStorageData(list); 
            }

        }
        /// <summary>
        /// 从文本中调取
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            Func<BsonDocument, bool> predicate = null;
            string connStr = "mongodb://sa:dba@192.168.1.230/WorkPlanManage";
            DataOperation operation = new DataOperation(new MongoOperation(connStr));
            string[] source = new string[] { "set_name", "name2", "name", "basic_explain", "detail_explain", "fullset_basic_explain", "fullset_detail_explain", "flavor_text", "explain", "speech", "emancipate_explain" };
            Dictionary<string, BsonDocument> keyFieldDic = new Dictionary<string, BsonDocument>();
            string path = "equipment2.txt";
            StreamReader reader = null;
            try
            {
                reader = new StreamReader(path);
                for (string equipmentInfo = reader.ReadLine(); equipmentInfo != null; equipmentInfo = reader.ReadLine())
                {
                    if (equipmentInfo.Contains("//") || string.IsNullOrEmpty(equipmentInfo))
                    {
                        continue;
                    }
                    string[] strArray2 = equipmentInfo.Split(new char[] { '>' }, StringSplitOptions.RemoveEmptyEntries);
                    if (strArray2.Length >= 2)
                    {
                        string itemName = strArray2[0];
                        string itemDesc = strArray2[1];
                        itemDesc = Microsoft.VisualBasic.Strings.StrConv(itemDesc, Microsoft.VisualBasic.VbStrConv.SimplifiedChinese, 0).Replace("%%", "%");
                        int length = itemName.LastIndexOf('_');
                        if (length != -1)
                        {
                            string keyField = itemName.Substring(0, length);
                            string key = itemName.Substring(length + 1, (itemName.Length - length) - 1);
                            if (source.Contains<string>(keyField))
                            {
                                if (!keyFieldDic.ContainsKey(key))
                                {
                                    keyFieldDic.Add(key, new BsonDocument().Add(keyField, itemDesc));
                                }
                                else
                                {
                                    BsonDocument document = keyFieldDic[key];
                                    document.Set(keyField, itemDesc);
                                }
                                continue;
                            }
                            bool flag = false;
                            foreach (string sourceField in source)
                            {
                                if (itemName.Contains(sourceField))
                                {
                                    key = itemName.Replace(sourceField, "").Trim(new char[] { '_' });
                                    keyField = itemName.Replace(key, "").Trim(new char[] { '_' });
                                    if (!keyFieldDic.ContainsKey(key))
                                    {
                                        keyFieldDic.Add(key, new BsonDocument().Add(keyField, itemDesc));
                                    }
                                    else
                                    {
                                        keyFieldDic[key].Set(keyField, itemDesc);
                                    }
                                    flag = true;
                                    break;
                                }
                            }
                            if (!flag)//未查找到字段
                            {
                                this.richTextBox1.Text = this.richTextBox1.Text + string.Format("未查找字段:{0}:{1}\n\r", keyField, equipmentInfo);
                            }
                            continue;
                        }
                        this.richTextBox1.Text = this.richTextBox1.Text + string.Format("未查找字段:{0}:{1}\n\r", itemName, equipmentInfo);
                        continue;
                    }
                    if (strArray2.Length < 1)
                    {
                        this.richTextBox1.Text = this.richTextBox1.Text + string.Format("未识别字段:{0}\n\r", equipmentInfo);
                    }
                }
                reader.Close();
                List<StorageData> list = new List<StorageData>();
                if (predicate == null)
                {
                    predicate = c => keyFieldDic.ContainsKey(c.Text("it_no"));
                }
                List<BsonDocument> ItemList = operation.FindAll("Item").Where<BsonDocument>(predicate).ToList<BsonDocument>();
                foreach (BsonDocument item in ItemList)
                {
                    BsonDocument bsonDoc = keyFieldDic[item.Text("it_no")];
                    bsonDoc.Set("remark", bsonDoc.Text("detail_explain"));
                    StorageData itemUpdate = new StorageData
                    {
                        Document = bsonDoc,
                        Name = "Item",
                        Query = Query.EQ("it_no", item.Text("it_no")),
                        Type = StorageType.Update
                    };
                    list.Add(itemUpdate);
                }
                if (list.Count<StorageData>() > 0)
                {
                }
            }
            catch (IOException exception)
            {
                Console.WriteLine(exception.Message);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            List<ADDepartment> entity = new List<ADDepartment>();
            entity = new SerializerXml<List<ADDepartment>>(entity).BuildObject("dep.xml");
            List<ADUser> list2 = new List<ADUser>();
            list2 = new SerializerXml<List<ADUser>>(list2).BuildObject("user.xml");
            ADDepartment curDep = new ADDepartment
            {
                Name = "华侨城组织架构",
                Path = "LDAP://12/ou=Oct,cn=Apps,DC=CHINAOCT,DC=COM",
                Code = "LDAP://12/cn=华侨城策划组织架构,cn=Apps,DC=CHINAOCT,DC=COM",
                Level = 0,
                Guid = "0000000000000000000",
                NewGuid = "0000000000000000000",
                ParentName = ""
            };
            string connStr = "mongodb://sa:dba@192.168.1.114/HQC2";
            DataOperation dataOp = new DataOperation(new MongoOperation(connStr));
            //ADToDB odb = new ADToDB();
            //ADDepartment department2 = odb.GetADTreeHQC_Modify(entity, list2, curDep);
            //new SerializerXml<ADDepartment>(department2).BuildXml("tree.xml");
            //OrganizationAD nad = new OrganizationAD(dataOp);
            //nad.OrganizationSave(entity, department2);
            ////nad.UserInsertByDepType(odb.UserListFilter(department2), 1, 0, 0, 1);
            //MessageBox.Show("oK");

        }

        private string DBStringLatinToNormal(string s_unicode)
        {
            Encoding encoding = Encoding.GetEncoding("iso8859-1");
            Encoding encoding2 = Encoding.GetEncoding("gbk");
            byte[] bytes = encoding.GetBytes(s_unicode);
            return encoding2.GetString(bytes);
        }

        private string DBStringToNormal(string dbStr)
        {
            byte[] bytes = new byte[dbStr.Length];
            for (int i = 0; i < dbStr.Length; i++)
            {
                bytes[i] = (byte)dbStr[i];
            }
            return Encoding.UTF8.GetString(bytes, 0, dbStr.Length);
        }



        public DataTable getmysqlAd(string M_str_sqlstr)
        {
            MySqlConnection connection = this.getmysqlcon();
            MySqlCommand selectCommand = new MySqlCommand(M_str_sqlstr, connection);
            connection.Open();
            new MySqlCommand(" set names latin1 ", connection).ExecuteNonQuery();
            DataTable dataTable = new DataTable();
            new MySqlDataAdapter(selectCommand).Fill(dataTable);
            connection.Close();
            return dataTable;
        }

        public void getmysqlcom(string M_str_sqlstr)
        {
            MySqlConnection connection = this.getmysqlcon();
            connection.Open();
            MySqlCommand command = new MySqlCommand(M_str_sqlstr, connection);
            command.ExecuteNonQuery();
            command.Dispose();
            connection.Close();
            connection.Dispose();
        }


        public MySqlConnection getmysqlcon()
        {
            return new MySqlConnection("server=localhost;user id=game;password=uu5!^%jg;database=taiwan_cain_web;Charset=latin1");
        }




        public MySqlDataReader getmysqlread(string M_str_sqlstr)
        {
            MySqlConnection connection = this.getmysqlcon();
            MySqlCommand command = new MySqlCommand(M_str_sqlstr, connection);
            connection.Open();
            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }





        CookieContainer cookie = new CookieContainer();
        public void PostData(object obj)
        {
            string s = string.Format("u={0}&p={1}&from_ui=1&dumy=", Guid.NewGuid().GetHashCode(), Guid.NewGuid().GetHashCode());
            string requestUriString = "http://asdhhrrdf.xmqesz.win:86/mb_edit_index_a1.asp";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUriString);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = Encoding.UTF8.GetByteCount(s);
            request.CookieContainer = this.cookie;
            request.Headers["Accept-Encoding"] = "gzip, deflate";
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)";
            StreamWriter writer = new StreamWriter(request.GetRequestStream(), Encoding.GetEncoding("gb2312"));
            writer.Write(s);
            writer.Close();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            response.Cookies = this.cookie.GetCookies(response.ResponseUri);
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
            string str3 = reader.ReadToEnd();
            reader.Close();
            responseStream.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitDeviceInfo();
            var dir = new FileInfo(Application.ExecutablePath);
            this.openFileDialog1.InitialDirectory = dir.DirectoryName;
            LibCurlNet.HttpManager.Instance.InitWebClient(hi, true, 30, 30);
        }

        public class AnagramEqualityComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y)
            {
                return getCanonicalString(x) == getCanonicalString(y);
            }

            public int GetHashCode(string obj)
            {
                return getCanonicalString(obj).GetHashCode();
            }

            private string getCanonicalString(string word)
            {
                char[] wordChars = word.ToCharArray();
                Array.Sort<char>(wordChars);
                return new string(wordChars);
            }
        }

        public class MapPoint
        {
            public double x;
            public double y;
            public MapPoint(double _x, double _y)
            {
                x = _x; y = _y;
            }
            
        }
        /// <summary>
        /// 多边形
        /// </summary>
        public class MapPolygon
        {
            public List<MapPoint> PointList { get; set; }
            public void Add(MapPoint point)
            {
                if (PointList == null) PointList = new List<MapPoint>();
                PointList.Add(point);
            }
            public double[,] To2DArray()
            {
                double[,] polygon = new double[PointList.Count(), 2];  //语句（1）  
                var index = 0;
                foreach (var point in PointList)
                {
                    polygon[index, 0] = point.x;
                    polygon[index, 1] = point.y;
                    index++;
                }
                return polygon;
            }
        }
        /// <summary>
        /// 多边形
        /// </summary>
        public class MapCircle
        {


            public MapPoint CenterPoint { get; set; }
            public double Radius { get; set; }
            public bool Spherical { get; set; }
            public MapCircle(double x, double y, double radius)
            {
                CenterPoint = new MapPoint(x, y);
                Radius = GetConvertRadius(radius);
            }
            public MapCircle(double x, double y, double radius, bool spherical)
            {
                CenterPoint = new MapPoint(x, y);
                Radius = GetConvertRadius(radius, spherical);
                Spherical = spherical;
            }
            public MapCircle(MapPoint centerpoint, double radius)
            {
                CenterPoint = centerpoint;
                Radius = GetConvertRadius(radius); ;
            }
            public MapCircle(MapPoint centerpoint, double radius, bool spherical)
            {
                CenterPoint = centerpoint;
                Radius = GetConvertRadius(radius, spherical); Spherical = spherical;
            }
            /// <summary>
            /// 转换为查询语句可兼容的radius
            /// 球面算（不能超过5000公里），不按球面算
            /// //radius = maxDistance / (3959 * 1.61) 3959.192为地球半径，单位英里。这里将英里转换成千米
            /// </summary>
            /// <param name="maxDistance">公里数</param>
            /// <param name="spherical">是否求面算</param>
            /// <returns></returns>
            public double GetConvertRadius(double maxDistance, bool spherical = false)
            {
                if (spherical)//按球面算
                {
                    var radius = maxDistance / (3959 * 1.61);//radius/6378137.0=3959.192 * 1.61?
                    return radius;
                }
                else
                {
                    var radius = maxDistance / 100;//radius/6378137.0=3959.192 * 1.61?
                    return radius;
                }
            }
        }
        /// <summary>
        /// 多边形
        /// </summary>
        public class MapRectangle
        {


            public double LowerLeftX { get; set; }
            public double LowerLeftY { get; set; }
            public double UpperRightX { get; set; }
            public double UpperRightY { get; set; }
            public MapRectangle(double lowerLeftX, double lowerLeftY, double upperRightX, double upperRightY)
            {
                LowerLeftX = lowerLeftX;
                LowerLeftY = lowerLeftY;
                UpperRightX = upperRightX;
                UpperRightY = upperRightY;
            }
            public MapRectangle(MapPoint lowerLeft, MapPoint upperRight)
            {
                LowerLeftX = lowerLeft.x;
                LowerLeftY = lowerLeft.y;
                UpperRightX = upperRight.x;
                UpperRightY = upperRight.y;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var pointList = new MapPolygon();
            pointList.Add(new MapPoint(116.387112, 39.92097));
            pointList.Add(new MapPoint(116.385243, 39.913063));
            pointList.Add(new MapPoint(116.394226, 39.917988));
            pointList.Add(new MapPoint(116.401772, 39.921364));
            pointList.Add(new MapPoint(116.41248, 39.927893));




            var connStr = "mongodb://sa:dba@59.61.72.36/MZEnterpriseGeo";
            DataOperation dataOp = new DataOperation(new MongoOperation(connStr));
            MongoOperation _mongoDBOp = new MongoOperation(connStr);
            //var hitResult = dataOp.FindAllByQuery("MapInfo", Query.WithinPolygon("loc", pointList.To2DArray())).ToList();

            //var rect = new MapRectangle(new MapPoint(116.387112, 39.92097), new MapPoint(116.394226, 39.917988));
            //var hitResult = dataOp.FindAllByQuery("MapInfo", Query.WithinRectangle("loc", rect.LowerLeftX,rect.LowerLeftY,rect.UpperRightX,rect.UpperRightY)).ToList();
            //5公里/   
            var radius = 5;//5公里 ,球面算（不能超过5000公里），不按球面算
            //3959.192为地球半径，单位英里。这里将英里转换成千米
            //var radius = 0.05;
            var circle = new MapCircle(new MapPoint(118.783799, 31.979234), radius,false);
            var hitResult = dataOp.FindAllByQuery("EnterpriseGeo", Query.And(Query.WithinCircle("loc", circle.CenterPoint.x, circle.CenterPoint.y, circle.Radius, circle.Spherical), Query.EQ("isDetail", "1"))).ToList();
            foreach(var result in hitResult)
            {
            
            }
            

            //var allAccountList = dataOp.FindAllByQuery("QiXinEnterpriseKey",Query.And( Query.Exists("province", false), Query.Exists("provinceName", true))).SetFields("guid", "province", "name", "provinceName");
            //var cityName = "青岛";
            //var provinceName = "山东省";
            //var updateBson = new BsonDocument().Add("provinceName", provinceName);
            // updateBson.Add("cityName", cityName);
            //var guidList = new List<string>();
            //foreach (var enterprise in allAccountList.Where(c => c.Text("name").Contains(cityName)))
            //{

            //    guidList.Add(enterprise.Text("guid"));
            //}

            // DBChangeQueue.Instance.EnQueue(new StorageData() { Name = "QiXinEnterpriseKey", Document = updateBson, Query = Query.In("guid", guidList.Select(c=>(BsonValue)c)), Type = StorageType.Update });
            // StartDBChangeProcessQuick(_mongoDBOp);
            ////var hitResult = from c in allAccountList
            ////                group c by c.Text("province") into g
            ////                select new { g.Key, count=g.Count() };
            //foreach (var result in allAccountList.GroupBy(x => x.Text("provinceName")).Select(g => new { g.Key, count = g.Count() }))
            //{
            //    this.richTextBox2.Text += string.Format("{0}:{1}\r", result.Key, result.count);
            //}
            //var allAccountList = dataOp.FindAllByQuery("QiXinEnterpriseKey", Query.And(Query.Exists("detailInfo", false),Query.EQ("cityName","成都"),Query.EQ("key","1"))).SetFields("guid","name").ToList();
            //var _addBson = new BsonDocument().Add("ip", this.richTextBox1.Text).Add("cookie", this.richTextBox2.Text);
            //DBChangeQueue.Instance.EnQueue(new StorageData() { Name = "QCCAccountCookie", Document = _addBson, Type = StorageType.Insert });
            //StartDBChangeProcessQuick(_mongoDBOp);
            //return;
            ////启信账号添加
            //var tableName = "QCCAccount";

            //var allAccountNameList = dataOp.FindAll(tableName).Select(c => c.Text("name")).ToList();
            //var textStrArr = this.richTextBox1.Text.Replace("\t", " ").Split(new string[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            //foreach (var text in textStrArr)
            //{
            //    var accountArray = text.Trim().Split(new string[] { "  ", " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
            //    if (accountArray.Length >= 2)
            //    {
            //        var name = accountArray[0].Trim();
            //        var password = accountArray[1].Trim();
            //        if (!allAccountNameList.Contains(name))
            //        {
            //            var addBson = new BsonDocument().Add("name", name).Add("password", password);
            //            DBChangeQueue.Instance.EnQueue(new StorageData() { Name = tableName, Document = addBson, Type = StorageType.Insert });
            //        }
            //        else {
            //            var addBson = new BsonDocument().Add("password", password);
            //            DBChangeQueue.Instance.EnQueue(new StorageData() { Name = tableName, Document = addBson,Query=Query.EQ("name",name), Type = StorageType.Update });
            //        }
            //    }
            //}

            //启信关键字添加
            //var allAccountNameList = dataOp.FindAll("QCCEnterpriseKeyScopeKeyWord").Select(c => c.Text("keyWord")).ToList();
            //var keyWordStr = this.richTextBox1.Text;
            //var textStrArr = keyWordStr.Split(new string[] { "\n", "\r"}, StringSplitOptions.RemoveEmptyEntries);
            //foreach (var text in textStrArr)
            //{


            //        var keyWord = text.Trim();

            //        if (allAccountNameList.Contains(keyWord)) continue;
            //        allAccountNameList.Add(keyWord);
            //        var addBson = new BsonDocument().Add("keyWord", keyWord);
            //        DBChangeQueue.Instance.EnQueue(new StorageData() { Name = "QCCEnterpriseKeyScopeKeyWord", Document = addBson, Type = StorageType.Insert });

            //}

            //DBChangeQueue.Instance.EnQueue(new StorageData() { Name = "QiXinEnterpriseKey", Document = new BsonDocument().Add("key", "2"), Query = Query.And(Query.Exists("detailInfo", false), Query.EQ("cityName", "成都"), Query.EQ("key", "1")), Type = StorageType.Update });
            StartDBChangeProcessQuick(_mongoDBOp);
            MessageBox.Show("succeed");
        }

        /// <summary>
        /// Check for Images
        /// read text from these images.
        /// save text from each image in text file automaticly.
        /// handle problems with images
        /// </summary>
        /// <param name="directoryPath">Set Directory Path to check for Images in it</param>
        public void CheckFileType(string directoryPath)
        {
            IEnumerator files = Directory.GetFiles(directoryPath).GetEnumerator();
            while (files.MoveNext())
            {
                //get file extension 
                string fileExtension = Path.GetExtension(Convert.ToString(files.Current));

                //get file name without extenstion 
                string fileName =
                  Convert.ToString(files.Current).Replace(fileExtension, string.Empty);

                //Check for JPG File Format 
                if (fileExtension == ".jpg" || fileExtension == ".JPG")
                // or // ImageFormat.Jpeg.ToString()
                {
                    try
                    {
                        ////OCR Operations ... 
                        //MODI.Document md = new MODI.Document();
                        //md.Create(Convert.ToString(files.Current));
                        ////md.OCR(MODI.MiLANGUAGES.miLANG_ENGLISH, true, true);
                        //md.OCR(MODI.MiLANGUAGES.miLANG_CHINESE_SIMPLIFIED, true, true);
                        //MODI.Image image = (MODI.Image)md.Images[0];

                        ////create text file with the same Image file name 
                        //FileStream createFile =
                        //  new FileStream(fileName + ".txt", FileMode.CreateNew);
                        ////save the image text in the text file 
                        //StreamWriter writeFile = new StreamWriter(createFile);
                        //writeFile.Write(image.Layout.Text);
                        //writeFile.Close();
                    }
                    catch (Exception exc)
                    {
                        //uncomment the below code to see the expected errors
                        //MessageBox.Show(exc.Message,
                        //"OCR Exception",
                        //MessageBoxButtons.OK, MessageBoxIcon.Information); 
                    }
                }
            }
        }

        public class province
        {
            public string name { get; set; }
            public List<city> cityList { get; set; }

        }
        public class city
        {
            public string name { get; set; }

        }



        private void button6_Click(object sender, EventArgs e)
        {
            var provinceList = new List<province>();
            string connStr = "mongodb://sa:dba@192.168.1.114/CityData";
            DataOperation operation = new DataOperation(new MongoOperation(connStr));
            var allCityList = operation.FindAll("SysSDCityArea").ToList();//城市列表
            var cityHouseConStr = "Data Source=192.168.1.114;Initial Catalog=MZCityLibrary;User ID=sa;Password=dba";

            var cityHouseHelper = new SqlServerHelper(cityHouseConStr);


            //声明添加对象

            var projListQuery = string.Format(" select cityName+' '+name as fullName from dbo.MZ_Project where x is null");
            DataTable projList = SqlServerHelper.ExecuteDataTable(cityHouseHelper.GetConnection(), CommandType.Text, projListQuery);

            string path = "重庆.txt";//测试用 全数据为1 经纬度
            StreamReader reader = null;
            var updateSB = new StringBuilder();
            try
            {
                using (reader = new StreamReader(path))
                {
                    for (string strInfo = reader.ReadLine(); strInfo != null; strInfo = reader.ReadLine())
                    {
                        //GPS纬度,GPS经度,谷歌地图纬度,谷歌地图经度,百度纬度,百度经度,地址,邮编,关键词
                        //25.0994005446,102.7637022828,25.0964663683,102.7651989554,25.102294,102.771758,云南省昆明市盘龙区沣源路,-,昆明 清水佳湖雅居
                        var strInfoArray = strInfo.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        if (strInfoArray.Length >= 9)
                        {
                            var baiduX = strInfoArray[4];//百度经度
                            var baiduY = strInfoArray[5];//百度维度
                            if (baiduX == "0" || baiduY == "0") continue;
                            var address = strInfoArray[6];
                            // var code = strInfoArray[7];//邮编
                            var fullCityName = strInfoArray[8];
                            var cityArray = fullCityName.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            if (cityArray.Length < 2) continue;
                            var cityName = cityArray[0];
                            var projectName = cityArray[1];
                            //var cityName = "重庆";
                            //var projectName = fullCityName.Replace("重庆","");

                            if (!string.IsNullOrEmpty(baiduX) && !string.IsNullOrEmpty(baiduY) && !string.IsNullOrEmpty(fullCityName))
                            {
                                //var hitDataRow = projList.Select("where fullName={0}" + fullCityName).FirstOrDefault();
                                // if (hitDataRow != null)
                                {
                                    var curSB = new StringBuilder();

                                    curSB.AppendFormat(" update MZ_Project set y='{0}',x='{1}',tel='手动' ", baiduX, baiduY);
                                    if (!string.IsNullOrEmpty(address) && address != "-")
                                    {
                                        curSB.AppendFormat(",address='{0}'", address);
                                    }
                                    curSB.AppendFormat(" where cityName='{0}' and name='{1}'\n", cityName, projectName);
                                    updateSB.AppendFormat(curSB.ToString());
                                }
                            }
                        }

                    }
                    this.richTextBox1.Text = updateSB.ToString();


                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                reader.Close();
            }

        }


        /// <summary>
        /// 判断是否是省
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private bool IsProvince(string equipmentInfo)
        {
            if (string.IsNullOrEmpty(equipmentInfo)) return false;
            return equipmentInfo.Contains("省") || equipmentInfo.Contains("：") || equipmentInfo.Contains(":");
        }

        /// <summary>
        /// 消除重复
        /// </summary>
        private void DistinctData()
        {
            string connStr = "mongodb://sa:dba@192.168.1.230/SimpleCrawler";
            DataOperation operation = new DataOperation(new MongoOperation(connStr));
            List<StorageData> list = new List<StorageData>();
            var curAllIpList2 = operation.FindAll("IPProxy").SetFields("key", "ip", "status").ToList();
            var groupByIP = from c in curAllIpList2
                            group c by c.Text("ip") into g
                            where g.Count() >= 2
                            select new { ip = g.Key, count = g.Count() };
            var gourByIPList = groupByIP.ToList();
            gourByIPList.ForEach(c =>
            {
                var hitObj = curAllIpList2.Where(d => d.Text("ip") == c.ip).FirstOrDefault();
                if (hitObj != null)
                {
                    list.Add(new StorageData() { Query = Query.EQ("key", hitObj.Text("key")), Name = "IPProxy", Type = StorageType.Delete });
                    this.richTextBox2.Text += string.Format("ip:{0}:count:{1},status:{2}\n\r", c.ip, c.count, hitObj.Text("status"));
                }
            }
                );
            if (list.Count() > 0)
            {
                var result = operation.BatchSaveStorageData(list);
            }
            return;
        }
        private void button7_Click(object sender, EventArgs e)
        {
            string connStr = "mongodb://sa:dba@192.168.1.230/SimpleCrawler";
            DataOperation operation = new DataOperation(new MongoOperation(connStr));
            List<StorageData> list = new List<StorageData>();
            var curAllIpList = operation.FindAll("IPProxy").SetFields("key", "ip", "status").Select(c => c.Text("ip")).ToList();

            //1.209.188.180:8080@HTTP#韩国
            var array = this.richTextBox1.Text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var str in array)
            {

                var ipArray = str.Split(new string[] { "@", " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                if (ipArray.Count() > 1)
                {
                    var firIndex = 0;
                    //Cn	49.72.126.252	8888	江苏苏州	高匿	HTTP	8小时	不到1分钟
                    var ipStr = ipArray[firIndex];
                    while (!IPCheck(ipStr))
                    {
                        firIndex += 1;
                        if (firIndex < ipArray.Count())
                        {
                            ipStr = ipArray[firIndex];
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (firIndex >= ipArray.Count())
                    {
                        continue;
                    }
                    if (curAllIpList.Any(c => c.Contains(ipStr))) continue;//防止重复添加
                    var remark = string.Empty;
                    if (ipStr.Contains(":"))
                    {
                        if (ipArray.Length >= 2)
                        {
                            remark = ipArray[firIndex + 1];
                        }
                    }
                    else
                    {
                        if (ipArray.Length >= 2)
                        {
                            var port = ipArray[firIndex + 1];
                            ipStr += ":" + port;
                        }
                        for (var i = firIndex + 2; i <= ipArray.Length - 1; i++)
                        {
                            remark += ipArray[i];
                        }
                    }
                    if (IPCheck(ipStr))
                    {
                        list.Add(new StorageData() { Document = new BsonDocument().Add("ip", ipStr).Add("remark", remark), Name = "IPProxy", Type = StorageType.Insert });
                    }
                    else
                    {
                        this.richTextBox2.Text += string.Format("{0}无法添加ip:{1}\n", str, ipStr);
                    }
                }
            }
            if (list.Count() > 0)
            {
                operation.BatchSaveStorageData(list);
                this.richTextBox2.Text += string.Format("成功添加ip个数{0}:", list.Count());
            }
        }
        public bool IPCheck(string ip)
        {
            string pattrn = @"(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])[:]*[\d]*";
            if (System.Text.RegularExpressions.Regex.IsMatch(ip, pattrn))
            {
                return true;
            }
            else
            {
                return false;

            }
        }

        static Queue<StorageData> Queue = new Queue<StorageData>();
        public delegate void AddIPEventHandler(EventArgs args, BsonDocument IpProxy);
        public event AddIPEventHandler AddIPEvent;
        public static int ListCount = 0;
        public static int AllCount = 0;
        private void button8_Click(object sender, EventArgs e)
        {
            AddIPEvent += IPResultReviceEvent;
            string connStr = "mongodb://sa:dba@192.168.1.230/SimpleCrawler";
            DataOperation operation = new DataOperation(new MongoOperation(connStr));

            var ipProxyList = operation.FindAllByQuery("IPProxy", Query.NE("status", "1")).ToList();


            List<Task> AllTask = new List<Task>();

            //var task = new Task<int>(StartCode, i, TaskCreationOptions.AttachedToParent);
            AllCount = ipProxyList.Count();
            var index = 1;
            foreach (var ipProxy in ipProxyList)
            {

                ThreadPool.QueueUserWorkItem((_ipProxy) =>
                {
                    DotNet.Utilities.HttpHelper http = new DotNet.Utilities.HttpHelper();
                    var curIpProxy = _ipProxy as BsonDocument;
                    //创建Httphelper参数对象
                    DotNet.Utilities.HttpItem item = new DotNet.Utilities.HttpItem()
                    {
                        //URL = "http://luckymn.cn",//URL     必需项 
                        URL = "http://www.qichacha.com/",
                        Method = "get",//URL     可选项 默认为Get   
                        ContentType = "text/html",//返回类型    可选项有默认值 
                        ProxyIp = ipProxy.Text("ip"),
                        Timeout = 2000
                    };

                    //请求的返回值对象
                    DotNet.Utilities.HttpResult result = http.GetHtml(item);
                    if (result.StatusCode != HttpStatusCode.OK)
                    {
                        var addUrlEventArgs = new EventArgs();
                        if (curIpProxy.Text("status") != "1")
                        {

                            Queue.Enqueue(new StorageData()
                            {
                                Document = new BsonDocument().Add("status", "1"),
                                Name = "IPProxy",
                                Type = StorageType.Update,
                                Query = Query.EQ("ip", curIpProxy.Text("ip"))
                            });

                        }
                        AddIPEvent(addUrlEventArgs, curIpProxy);
                    }
                    else
                    {
                        var addUrlEventArgs = new EventArgs();
                        if (curIpProxy.Text("status") != "0")
                        {
                            try
                            {
                                Queue.Enqueue(new StorageData()
                                {
                                    Document = new BsonDocument().Add("status", "0"),
                                    Name = "IPProxy",
                                    Type = StorageType.Update,
                                    Query = Query.EQ("ip", curIpProxy.Text("ip"))
                                });
                                curIpProxy.Set("avaiable", "1");
                            }
                            catch (Exception ex)
                            { }
                        }
                        AddIPEvent(addUrlEventArgs, curIpProxy);
                    }

                }, ipProxy);
                if (index++ % 100 == 0)
                {
                    //Thread.Sleep(500);
                }

                // Thread.Sleep(50);
                // Task cwt = task.ContinueWith(_task => Console.WriteLine("thread index 真正完成 is:{0}", _task.Result), TaskContinuationOptions.AttachedToParent);

            }
            //Task.WaitAll(AllTask.ToArray());

            //获取请请求的Html
            //string html = result.Html;
            //this.richTextBox2.Text = html;

        }
        public static int iCount;
        private void IPResultReviceEvent(EventArgs args, BsonDocument IpProxy)
        {
            Interlocked.Increment(ref iCount);
            this.richTextBox2.Invoke(new Action(delegate
            {
                if (IpProxy.Text("avaiable") != "1")
                {
                    this.richTextBox2.AppendText(string.Format("{0} 失败，更新\n", IpProxy.Text("ip")));
                }
                else
                {
                    this.richTextBox2.AppendText(string.Format("{0} 可用，更新\n", IpProxy.Text("ip")));
                }
                if (iCount >= AllCount)
                {
                    this.richTextBox2.AppendText(string.Format("操作完毕", IpProxy.Text("ip")));
                }
            }));
            if (Queue.Count() >= 20 || AllCount - ListCount <= 20)
            {
                SaveIpStatus();
            }

        }

        public bool SaveIpStatus()
        {
            var result = new InvokeResult();
            string connStr = "mongodb://sa:dba@192.168.1.230/SimpleCrawler";
            DataOperation operation = new DataOperation(new MongoOperation(connStr));
            List<StorageData> updateList = new List<StorageData>();
            while (Queue.Count() > 0 && updateList.Count() <= 20)
            {
                try
                {
                    var curStorage = Queue.Dequeue();

                    if (curStorage != null)
                    {
                        updateList.Add(curStorage);
                        ListCount++;
                    }
                }
                catch (Exception ex)
                { }
            }
            if (updateList.Count() > 0)
            {
                result = operation.BatchSaveStorageData(updateList);
                if (result.Status != Status.Successful)
                {
                    return false;
                }
            }
            if (Queue.Count() > 0)
            {
                SaveIpStatus();
            }
            return true;
        }

        public void TestLogin()
        {
            DotNet.Utilities.HttpHelper http = new DotNet.Utilities.HttpHelper();
            DotNet.Utilities.HttpItem item = new DotNet.Utilities.HttpItem()
            {
                URL = "http://bbs.xmfish.com/login.php",//URL     必需项
                Encoding = null,//编码格式（utf-8,gb2312,gbk）     可选项 默认类会自动识别
                //Encoding = Encoding.Default,
                Method = "post",//URL     可选项 默认为Get
                //Timeout = 100000,//连接超时时间     可选项默认为100000
                //ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000
                //IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写
                //Cookie = "",//字符串Cookie     可选项
                UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)",//用户的浏览器类型，版本，操作系统     可选项有默认值
                Accept = "text/html, application/xhtml+xml, */*",//    可选项有默认值
                ContentType = "application/x-www-form-urlencoded",//返回类型    可选项有默认值
                //Referer = "http://www.sufeinet.com",//来源URL     可选项
                Postdata = "answer=&cktime=0&customquest=&forward=&hideid=0&jumpurl=&lgt=0&pwpwd=&pwuser=&question=0&step=2&submit=",
                Allowautoredirect = true
            };
            DotNet.Utilities.HttpResult result = http.GetHtml(item);
            string cookie = string.Empty;
            foreach (CookieItem s in HttpCookieHelper.GetCookieList(result.Cookie))
            {
                if (s.Key.Contains("24a79_"))
                {
                    cookie += HttpCookieHelper.CookieFormat(s.Key, s.Value);
                }
            }
            if (result.Html.IndexOf("您已经顺利登录") > 0)
            {
                item = new DotNet.Utilities.HttpItem()
                {
                    URL = "http://bbs.xmfish.com/u.php",
                    Cookie = cookie
                };
                result = http.GetHtml(item);//目前这个里面是未登入的状态
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            //            x-requested-with: XMLHttpRequest
            //Accept-Language: zh-CN
            //Referer: https://passport.fang.com/
            //Accept: */*
            //Content-Type: application/x-www-form-urlencoded
            //Accept-Encoding: gzip, deflate
            //User-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko
            //Host: passport.fang.com
            //Content-Length: 324
            //Connection: Keep-Alive
            //Cache-Control: no-cache
            DotNet.Utilities.HttpHelper http = new DotNet.Utilities.HttpHelper();
            DotNet.Utilities.HttpItem item = new DotNet.Utilities.HttpItem()
            {
                URL = "https://passport.fang.com/login.api",//URL     必需项
                Encoding = null,//编码格式（utf-8,gb2312,gbk）     可选项 默认类会自动识别
                //Encoding = Encoding.Default,
                Method = "post",//URL     可选项 默认为Get
                //Timeout = 100000,//连接超时时间     可选项默认为100000
                //ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000
                //IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写
                //Cookie = "",//字符串Cookie     可选项
                UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko",//用户的浏览器类型，版本，操作系统     可选项有默认值
                Accept = "text/html, application/xhtml+xml, */*",//    可选项有默认值
                ContentType = "application/x-www-form-urlencoded",//返回类型    可选项有默认值
                Referer = "https://passport.fang.com/",//来源URL     可选项
                Postdata = "Uid=zluckymn&Pwd=0031f915e6fe279122d028654fad2da21026a111d01302b1ebefccc79619403286027a209868e65ff764d9426fe318c565bb08df4e8a74538a8e658e55d9f6bb352a914da07b3ae36d82300570099f10472127bbb6fd56753b609aa8370fe17e26b6c5b98f7dbe5427615a8c17b8383a813fe770f972305636cecc886b0f64e5&Service=soufun-passport-web&IP=&VCode=&AutoLogin=1",
                Allowautoredirect = true,
            };
            DotNet.Utilities.HttpResult result = http.GetHtml(item);
            string cookie = string.Empty;
            foreach (CookieItem s in HttpCookieHelper.GetCookieList(result.Cookie))
            {
                //if (s.Key.Contains("24a79_"))
                {
                    cookie += HttpCookieHelper.CookieFormat(s.Key, s.Value);
                }
            }
            if (result.Html.IndexOf("zluckymn") > 0)
            {
                var item1 = new DotNet.Utilities.HttpItem()
                {
                    URL = "http://land.fang.com/market/166dd2f8-e5c8-44fd-bd66-c6b34bca2aa4.html",
                    Cookie = cookie
                };
                result = http.GetHtml(item1);//目前这个里面是未登入的状态
                this.richTextBox1.Text = result.Html;
            }
            if (result.Html.Contains("419万元"))
            {
                MessageBox.Show("登陆成功");
            }
        }

        string connStr = "mongodb://sa:dba@192.168.1.230/WorkPlanManage";
        DataOperation dataop = new DataOperation(new MongoOperation("mongodb://sa:dba@192.168.1.230/WorkPlanManage"));
        private void button10_Click(object sender, EventArgs e)
        {


            string DataTableName = "LandFang";//存储的数据库表明
            string DataTableNameURL = "LandFangURL";//存储的数据库表明
            var hitUrl = dataop.FindAllByQuery(DataTableNameURL, Query.Matches("url", "/-/")).Select(c => c["url"]).ToList();//执行过的
            //var AllUrl = dataop.FindAll(DataTableNameURL).Select(c => c.Text("url")).ToList();//执行过的
            var savedUrl = dataop.FindAll(DataTableName).Select(c => c["url"]).ToList();//保存过的
            //   var unSaveUrl = hitUrl.Where(c =>savedUrl.IndexOf(c)==-1).ToList();//需要再次执行的
            //尝试分半
            var splitIndex = 1000;
            var skipCount = 0;
            var allCount = savedUrl.Count();
            List<Task> AllTask = new List<Task>();

            while (skipCount < hitUrl.Count())
            {
                var curSaveUrl = hitUrl.Skip(skipCount).Take(splitIndex).ToList();
                var task = new Task((_urlList) =>
                {

                    // var unSaveUrl = dataop.FindAllByQuery(DataTableNameURL, Query.NotIn("url", urlList as List<BsonValue>)).ToList();//需要再次执行的
                    var urlList = _urlList as List<BsonValue>;
                    var unSaveUrl = urlList.Where(c => !savedUrl.Contains(c)).ToList();

                    var index = 1;
                    foreach (var url in unSaveUrl)
                    {
                        try
                        {

                            DBChangeQueue.Instance.EnQueue(new StorageData() { Name = "LandFangNewUrl", Document = new BsonDocument().Add("url", url), Type = StorageType.Insert });
                            if (index++ % 20 == 0)//开启新进程
                            {
                                StartDBChangeProcess();
                            }

                        }
                        catch (Exception ex)
                        {
                            this.richTextBox1.Text += string.Format("{0}出错{1}\n\r", url, ex.Message);
                        }

                    }
                }, curSaveUrl, TaskCreationOptions.AttachedToParent);
                task.Start();
                AllTask.Add(task);
                skipCount += splitIndex;
                Thread.Sleep(1000);
            }
            Task.WaitAll(AllTask.ToArray());//等待完成
            StartDBChangeProcess();
        }

        private void StartDBChangeProcess()
        {

            List<StorageData> updateList = new List<StorageData>();
            while (DBChangeQueue.Instance.Count > 0)
            {
                var curStorage = DBChangeQueue.Instance.DeQueue();
                if (curStorage != null)
                {
                    updateList.Add(curStorage);
                }
            }
            if (updateList.Count() > 0)
            {
                var result = dataop.BatchSaveStorageData(updateList);
                if (result.Status != Status.Successful)//出错进行重新添加处理
                {
                    foreach (var storageData in updateList)
                    {
                        DBChangeQueue.Instance.EnQueue(storageData);
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_dataop"></param>
        /// <param name="imd"></param>
        private void StartDBChangeProcess(DataOperation _dataop, bool imd = false)
        {
            //if (DBChangeQueue.Instance.Count < 0) return;
            List<StorageData> updateList = new List<StorageData>();
            while (DBChangeQueue.Instance.Count > 0)
            {
                var curStorage = DBChangeQueue.Instance.DeQueue();
                if (curStorage != null)
                {
                    updateList.Add(curStorage);
                }
            }
            if (updateList.Count() > 0)
            {
                var result = _dataop.BatchSaveStorageData(updateList);
                if (result.Status != Status.Successful)//出错进行重新添加处理
                {
                    foreach (var storageData in updateList)
                    {
                        DBChangeQueue.Instance.EnQueue(storageData);
                    }
                }
            }
        }


        /// <summary>
        /// 对需要更新的队列数据更新操作进行批量处理,可考虑异步执行
        /// </summary>
        private static void StartDBChangeProcessQuick(MongoOperation _mongoDBOp)
        {
            if (_mongoDBOp == null)
            {
                var connStr = "mongodb://sa:dba@59.61.72.34/SimpleCrawler";

                _mongoDBOp = new MongoOperation(connStr);
            }
            var result = new InvokeResult();
            List<StorageData> updateList = new List<StorageData>();
            while (DBChangeQueue.Instance.Count > 0)
            {

                var temp = DBChangeQueue.Instance.DeQueue();
                if (temp != null)
                {
                    var insertDoc = temp.Document;

                    switch (temp.Type)
                    {
                        case StorageType.Insert:
                            //if (insertDoc.Contains("createDate") == false) insertDoc.Add("createDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));      //添加时,默认增加创建时间
                            //if (insertDoc.Contains("createUserId") == false) insertDoc.Add("createUserId", "1");
                            ////更新用户
                            //if (insertDoc.Contains("underTable") == false) insertDoc.Add("underTable", temp.Name);
                            //insertDoc.Set("updateDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));      //更新时间
                            //insertDoc.Set("updateUserId", "1");
                            result = _mongoDBOp.Save(temp.Name, insertDoc); ;
                            break;
                        case StorageType.Update:

                            result = _mongoDBOp.Save(temp.Name, temp.Query, insertDoc);
                            break;
                        case StorageType.Delete:
                            result = _mongoDBOp.Delete(temp.Name, temp.Query);
                            break;
                    }
                    //logInfo1.Info("");
                    if (result.Status == Status.Failed) throw new Exception(result.Message);

                }

            }

            if (DBChangeQueue.Instance.Count > 0)
            {
                StartDBChangeProcessQuick(_mongoDBOp);
            }
        }



        private void button11_Click(object sender, EventArgs e)
        {
            //var html = "http://fdc.fang.com/data/land/440100_440105________1_1.html";
            // this.richTextBox2.Text = IsGUID(html).ToString();
            var html = "http://fdc.fang.com/data/land/440100_440103________1_1.html";
            this.richTextBox2.Text = IsHitUrl(html).ToString();
        }
        public static bool IsGUID(string expression)
        {
            if (expression != null)
            {
                Regex guidRegEx = new Regex(@".*?market/(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1}).html");
                return guidRegEx.IsMatch(expression);
            }
            return false;
        }
        public static bool IsHitUrl(string expression)
        {

            Regex guidRegEx = new Regex(@".*?data/land/.*?_.*?________.*?_1.html");
            return guidRegEx.IsMatch(expression);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            var provinceList = new List<province>();
            //string connStr = "mongodb://sa:dba@192.168.1.114/CityData";
            //DataOperation operation = new DataOperation(new MongoOperation(connStr));
            //var allCityList = operation.FindAll("SysSDCityArea").ToList();//城市列表
            //var cityHouseConStr = "Data Source=192.168.1.114;Initial Catalog=MZCityLibrary;User ID=sa;Password=dba";

            //var cityHouseHelper = new SqlServerHelper(cityHouseConStr);


            //声明添加对象

            //var projListQuery = string.Format(" select cityName+' '+name as fullName from dbo.MZ_Project where x is null");
            //DataTable projList = SqlServerHelper.ExecuteDataTable(cityHouseHelper.GetConnection(), CommandType.Text, projListQuery);
            var cityname = this.textBox1.Text;
            //string path = cityname + "土地.txt";//测试用 全数据为1 经纬度
            // this.openFileDialog1.InitialDirectory = Application.StartupPath + path;
            ;

            var updateSB = new StringBuilder();
            try
            {
                var textStrArr = this.richTextBox1.Text.Split(new string[] { "\n", "\r", "\t" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string strInfo in textStrArr)
                {
                    // updateSB.Append(AddressXY(strInfo, cityname));
                    updateSB.Append(AddressXY_Fix(strInfo, cityname));
                }
                this.richTextBox2.Text = updateSB.ToString();

            }
            catch (Exception ex)
            {

            }
        }
        /// <summary>
        /// http://www.gpsspg.com/latitude-and-longitude.htm
        /// </summary>
        /// <param name="strInfo"></param>
        /// <param name="cityname"></param>
        /// <returns></returns>
        public string AddressXY(string strInfo, string cityname)
        {

            //GPS纬度,GPS经度,谷歌地图纬度,谷歌地图经度,百度纬度,百度经度,地址,邮编,关键词
            //25.0994005446,102.7637022828,25.0964663683,102.7651989554,25.102294,102.771758,云南省昆明市盘龙区沣源路,-,昆明 清水佳湖雅居
            var strInfoArray = strInfo.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if (strInfoArray.Length >= 9)
            {
                var baiduX = strInfoArray[4];//百度经度
                var baiduY = strInfoArray[5];//百度维度
                if (baiduX == "0" || baiduY == "0") return string.Empty;
                var address = strInfoArray[6];
                // var code = strInfoArray[7];//邮编
                var fullCityName = string.Empty;
                if (strInfoArray.Length > 9)
                {

                    for (var index = 8; index <= strInfoArray.Length - 1; index++)
                    {
                        if (!string.IsNullOrEmpty(fullCityName))
                        {
                            fullCityName += string.Format(" and position like '%{0}%'", strInfoArray[index]);
                        }
                        else
                        {
                            fullCityName = string.Format(" position like '%{0}%' ", strInfoArray[index]);
                        }
                    }
                }
                else
                {
                    fullCityName = string.Format(" position like '{0}%' ", strInfoArray[8].Trim()); ;//地址可能以逗号隔开
                }

                var cityArray = strInfoArray[8].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (cityArray.Length < 2) return string.Empty;
                //var cityName = cityArray[0];
                //var projectName = cityArray[1];//地址可能以逗号隔开

                //var cityName = "重庆";
                //var projectName = fullCityName.Replace("重庆","");
                if (!string.IsNullOrEmpty(address) && address != "-")
                {
                    //curSB.AppendFormat(",address='{0}'", address);
                    if (!address.Contains(cityname)) return string.Empty;
                }

                if (!string.IsNullOrEmpty(baiduX) && !string.IsNullOrEmpty(baiduY) && !string.IsNullOrEmpty(fullCityName))
                {
                    //var hitDataRow = projList.Select("where fullName={0}" + fullCityName).FirstOrDefault();
                    // if (hitDataRow != null)
                    {
                        var curSB = new StringBuilder();

                        curSB.AppendFormat(" update MZ_Land set y='{0}',x='{1}' ", baiduX, baiduY);
                        curSB.AppendFormat(" where cityName='{0}' and {1}\n", cityname, fullCityName.Replace(cityname + "市 ", "").Replace("% ", "%").Trim());
                        return curSB.ToString();
                    }
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// http://map.yanue.net/
        /// </summary>
        /// <param name="strInfo"></param>
        /// <param name="cityname"></param>
        /// <returns></returns>
        public string AddressXY_Fix(string strInfo, string cityname)
        {
            var filter = this.textBox2.Text;
            if (string.IsNullOrEmpty(filter))
            {
                filter = "cityName='{0}' and position like '{1}%'";
            }
            // /厦门 集美11-07片区厦门（新）站片区圣果路与圣岩路交叉口东南侧:118.071035,24.647118
            var strInfoArray = strInfo.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
            if (strInfoArray.Length >= 2)
            {

                var positionStr = strInfoArray[0];
                var regex = cityname + "市 ";
                var index = positionStr.IndexOf(regex);
                if (index != -1)
                {
                    positionStr = positionStr.Substring(index + regex.Length, positionStr.Length - regex.Length);
                }
                else
                {
                    regex = cityname + " ";
                    index = positionStr.IndexOf(regex);
                    if (index != -1)
                        positionStr = positionStr.Substring(index + regex.Length, positionStr.Length - regex.Length);
                }
                //118.071035,24.647118
                var XYString = strInfoArray[1];
                var xyArray = XYString.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                if (xyArray.Length < 2) return string.Empty;
                var baiduX = xyArray[0];//百度经度
                var baiduY = xyArray[1];//百度维度
                if (baiduX == "0" || baiduY == "0") return string.Empty;


                if (!string.IsNullOrEmpty(baiduX) && !string.IsNullOrEmpty(baiduY))
                {

                    var curSB = new StringBuilder();
                    curSB.AppendFormat(" update MZ_Land set x='{0}',y='{1}' ", baiduX, baiduY);
                    curSB.AppendFormat(" where x is null and " + filter + "\n", cityname.Trim(), positionStr.Trim());
                    return curSB.ToString();

                }
            }
            return string.Empty;
        }

        private void button13_Click(object sender, EventArgs e)
        {

            var cityHouseConStr = "Data Source=192.168.1.114;Initial Catalog=MZCityLibrary;User ID=sa;Password=dba";

            var cityHouseHelper = new SqlServerHelper(cityHouseConStr);
            //'QZ-B2D32841-1911-4BC0-9A8E-062EA525A097'
            var cityName = this.textBox1.Text;
            if (string.IsNullOrEmpty(cityName))
            {
                cityName = "厦门";
            }
            var projListQuery = string.Format(" select distinct a.name,  a.regionGuid from BaseCityRegion as a join BaseCity as b on a.cityGuid=b.cityGuid where b.name='{0}'", cityName);
            //获取区域列表
            DataTable regionList = SqlServerHelper.ExecuteDataTable(cityHouseHelper.GetConnection(), CommandType.Text, projListQuery);
            var regionDic = new Dictionary<string, string>();
            foreach (DataRow region in regionList.Rows)
            {

                regionDic.Add(region["name"].ToString(), region["regionGuid"].ToString());

            }
            var curSB = new StringBuilder();
            var hasEdit = new List<string>();
            var strInfoList = this.richTextBox1.Text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var strInfo in strInfoList)
            {
                var strInfoArray = strInfo.Split(new string[] { "：", ":" }, StringSplitOptions.RemoveEmptyEntries);
                if (strInfoArray.Length >= 2)
                {
                    var xyStr = strInfoArray[0];
                    var address = strInfoArray[1];
                    var hitRegion = regionDic.Where(c => address.Contains(c.Key)).FirstOrDefault();


                    if (hitRegion.Key != null)
                    {
                        var updateStr = string.Format(" update MZ_Land set regionGuid='{0}'", hitRegion.Value);
                        updateStr += string.Format(" where x+','+y='{0}' /*{1}*/ \n", xyStr, hitRegion.Key);
                        if (!hasEdit.Contains(updateStr))
                        {
                            curSB.AppendFormat(updateStr);
                        }
                        else
                        {
                            hasEdit.Add(updateStr);
                        }
                    }


                }
            }
            this.richTextBox2.Text = curSB.ToString();
        }

        private void button14_Click(object sender, EventArgs e)
        {
            //var temp= this.richTextBox1.Text.Replace("<p>", "").Replace("</p>", "").Replace("<br>","\n");
            var sb = new StringBuilder();
            var textArr = this.richTextBox1.Text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var text in textArr)
            {
                var resultArr = text.Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                if (resultArr.Length >= 4)
                {
                    var projectName = resultArr[0];
                    var address = resultArr[1];
                    var x = resultArr[2];
                    var y = resultArr[3];
                    sb.AppendFormat("update MZ_Project set x='{0}',y='{1}',address='{2}' where name='{3}' \n", x, y, address, projectName);
                }
            }
            this.richTextBox2.Text = sb.ToString();

        }

        public string stringCodeFix(string s)
        {

            //string s = "\\u91cd\\u5e86\\u5730\\u4ea7\\uff0c";
            string r = Regex.Replace(s, @"\\u([a-f0-9]{4})", m => ((char)ushort.Parse(m.Groups[1].Value, NumberStyles.HexNumber)).ToString());
            return r;
        }

        public string googleMapXYTest()
        {
            var url = "https://maps.googleapis.com/maps/api/js/GeocodeService.Search?4s%E7%A6%8F%E5%BB%BA%E5%8E%A6%E9%97%A8&7sUS&9szh-CN&callback=_xdc_._faspf3&token=40219";
            var urlDecode = HttpUtility.UrlDecode(url);
            DotNet.Utilities.HttpItem item = new DotNet.Utilities.HttpItem()
            {
                URL = url,//URL     必需项    
                Method = "get",//URL     可选项 默认为Get   
                ContentType = "text/html",//返回类型    可选项有默认值 
                ProxyIp = "111.11.228.77:80",
                Timeout = 5000
            };
            DotNet.Utilities.HttpHelper http = new DotNet.Utilities.HttpHelper();
            //请求的返回值对象
            DotNet.Utilities.HttpResult result = http.GetHtml(item);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                this.richTextBox2.Text += result.Html;
                //  Console.WriteLine("{0}{1}", ipProxy.Text("ip"), result.Html);
                // Queue.Enqueue(new StorageData() { Document = new BsonDocument().Add("status", "1"), Name = "IPProxy", Type = StorageType.Update, Query = Query.And(Query.EQ("ip", ipProxy.Text("ip"))) });
            }

            return string.Empty;
        }

        private void button15_Click(object sender, EventArgs e)
        {
            List<string> QueueList = new List<string>();

            UrlQueue.Instance.EnQueue(new UrlInfo("http://www.baidu.com"));
            UrlQueue.Instance.EnQueue(new UrlInfo("http://google.com"));
            if (UrlQueue.Instance.Count > 0)
            {
                while (UrlQueue.Instance.Count > 0)
                {
                    QueueList.Add(UrlQueue.Instance.DeQueue().UrlString);
                }
                SerializerXml<List<string>> serial = new SerializerXml<List<string>>(QueueList);
                serial.BuildXml("UrlQueue.xml");
            }

            List<string> QueueList2 = new List<string>();
            SerializerXml<List<string>> serial2 = new SerializerXml<List<string>>(QueueList2);
            QueueList2 = serial2.BuildObject("UrlQueue.xml");
            this.richTextBox2.Text += QueueList2.Count();
        }

        /// <summary>
        /// 序列化当前队列
        /// </summary>
        public static void SaveUrlQueue()
        {
            List<string> QueueList = new List<string>();
            if (UrlQueue.Instance.Count > 0)
            {
                while (UrlQueue.Instance.Count > 0)
                {
                    QueueList.Add(UrlQueue.Instance.DeQueue().UrlString);
                }
                SerializerXml<List<string>> serial = new SerializerXml<List<string>>(QueueList);
                serial.BuildXml("UrlQueue.xml");
            }
        }
        /// <summary>
        /// 反序列化当前队列
        /// </summary>
        /// <returns></returns>
        public static List<string> LoadUrlQueue()
        {
            List<string> QueueList = new List<string>();
            SerializerXml<List<string>> serial = new SerializerXml<List<string>>(QueueList);
            QueueList = serial.BuildObject("UrlQueue.xml");
            return QueueList;
        }

        private void button16_Click(object sender, EventArgs e)
        {
            var curSB = new StringBuilder();
            var hasEdit = new List<string>();
            var strInfoList = this.richTextBox1.Text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var strInfo in strInfoList)
            {
                if (!hasEdit.Contains(strInfo.Trim()))
                {
                    curSB.AppendFormat(strInfo + "\n");
                    hasEdit.Add(strInfo.Trim());
                }

            }
            this.richTextBox2.Text = curSB.ToString();
        }

        private void button17_Click(object sender, EventArgs e)
        {
            var connStr = "mongodb://sa:dba@59.61.72.34/SimpleCrawler";
            DataOperation dataOp = new DataOperation(new MongoOperation(connStr));
            var cityNameStr = "上海,北京,成都,福州,广州,杭州,黄山,济南,龙岩,南昌,南京,宁波,泉州,深圳,苏州,武汉,西安,厦门,大连,长沙,合肥,镇江,宁波,中山,郑州,昆明,江苏,重庆";
            var cityNameList = cityNameStr.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();

            var allQiXinEnterpriseKey = dataOp.FindAll("QiXinEnterpriseKey").SetFields("name").Select(c => c.Text("name")).Distinct().ToList();
            var allQiXinEnterprise = dataOp.FindAllByQuery("QiXinEnterprise", Query.Exists("城市", false)).SetFields("name").Select(c => c.Text("name")).Distinct().ToList();
            var allLandInfoList = dataOp.FindAllByQuery("LandFang",
                Query.And(Query.Or(Query.In("地区", cityNameList.Select(c => (BsonValue)c)), Query.In("所在地", cityNameList.Select(c => (BsonValue)c))),
                Query.Exists("竞得方", true), Query.NE("竞得方", "暂无"))).SetFields("竞得方", "所在地", "地区").Where(c => !c.Text("竞得方").Contains("*")).ToList();
            foreach (var land in allQiXinEnterprise)
            {

                if (string.IsNullOrEmpty(land) || land == "暂无") continue;
                var updateBson = new BsonDocument().Add("name", land);
                if (allQiXinEnterpriseKey.Contains(land))
                {
                    updateBson.Add("status", "1");
                }
                if (land.Length <= 3)
                {
                    updateBson.Add("isUser", "1");
                    updateBson.Set("status", "1");
                }
                var hitCity = allLandInfoList.Where(c => c.Text("竞得方").Contains(land)).FirstOrDefault();
                if (hitCity == null) continue;
                updateBson.Add("地区", hitCity.Text("地区"));
                updateBson.Add("所在地", hitCity.Text("所在地"));
                if (cityNameList.Contains(hitCity.Text("地区")))
                {
                    updateBson.Set("城市", hitCity.Text("地区"));
                }
                if (cityNameList.Contains(hitCity.Text("所在地")))
                {
                    updateBson.Set("城市", hitCity.Text("所在地"));
                }
                updateBson.Add("isFirst", "1");
                Queue.Enqueue(new StorageData()
                {
                    Document = updateBson,
                    Name = "QiXinEnterprise",
                    Type = StorageType.Update,
                    Query = Query.EQ("name", land)
                });
                if (Queue.Count() >= 200)
                {
                    SaveIpStatus();
                }
            }
            SaveIpStatus();
        }

        private void button18_Click(object sender, EventArgs e)
        {
            var ipArrary = this.richTextBox1.Text.Trim().Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
            var ip = string.Empty;
            if (ipArrary.Length >= 2)
            {
                ip = string.Format("{0}:{1}", ipArrary[0], ipArrary[1]);
            }
            //var url = "http://www.qixin.com/search?key=%E5%8C%97%E4%BA%AC%E5%BE%B7%E4%BF%A1%E8%87%B4%E8%BF%9C%E7%A7%91%E6%8A%80%E6%9C%89%E9%99%90%E5%85%AC%E5%8F%B8&type=enterprise&source=&isGlobal=Y";
            var url = "http://www.baidu.com";
            DotNet.Utilities.HttpHelper http = new DotNet.Utilities.HttpHelper();
            DotNet.Utilities.HttpItem item = new DotNet.Utilities.HttpItem()
                   {
                       URL = url,//URL     必需项    
                       Method = "GET",//URL     可选项 默认为Get   
                       ContentType = "text/html",//返回类型    可选项有默认值 
                       ProxyIp = ip,
                       KeepAlive = true,
                       Timeout = 2000
                   };

            //请求的返回值对象
            DotNet.Utilities.HttpResult result = http.GetHtml(item);
            if (result.Html.Contains("使用验证码过于频繁"))
            {
                this.richTextBox2.Text += "使用验证码过于频繁";
                return;
            }
            if (result.StatusCode == HttpStatusCode.OK)
            {
                this.richTextBox2.Text += "访问成功";
                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.LoadHtml(result.Html);
                var curUpdateBson = new BsonDocument();

                var searchResult = htmlDoc.DocumentNode.SelectSingleNode("//a[@class='search-result-company-name']");
                if (searchResult == null) return;
                var enterpriseName = searchResult.InnerText;
                var _url = searchResult.Attributes["href"] != null ? searchResult.Attributes["href"].Value : string.Empty;
                if (string.IsNullOrEmpty(_url)) return;
                curUpdateBson.Add("name", enterpriseName);
                curUpdateBson.Add("url", string.Format("http://www.qixin.com{0}", _url));
                ///company/fc0de68c-acff-4e5e-9444-7ed41761c2f5
                var startIndex = _url.LastIndexOf("/");
                if (startIndex == -1) return;
                var guid = _url.Substring(startIndex + 1, _url.Length - startIndex - 1);
                this.richTextBox2.Text += guid + curUpdateBson.Text("url") + enterpriseName;
            }
            else
            {
                this.richTextBox2.Text += result.StatusCode + result.Html;
            }

        }

        private void button19_Click(object sender, EventArgs e)
        {
            // return ;
            string connStr = "mongodb://sa:dba@59.61.72.34/SimpleCrawler";
            DataOperation dataop = new DataOperation(new MongoOperation(connStr));
            var cityUrlList = dataop.FindAll("LandFangCityEXURL").ToList();//城市url
            var cityName = "黄山";
            var cityCode = "341000";
            var fieldName = "cityCode";
            var noRegion = false;//是否没有县市
            var defaultRegionName="";
            var cityList = cityUrlList.Where(c => c.Text("type") != "2" && c.Text(fieldName) == cityCode).ToList();
            //var query = Query.Or(Query.EQ("县市", ""), Query.Exists("县市", false), Query.EQ("县市", cityName));
            var query = Query.Or(Query.EQ("县市", "推出时间:"));
            //目前有25万个
            var landUrlList = dataop.FindAllByQuery("LandFang", Query.And(Query.EQ("所在地", cityName),query )).SetFields("name", "url", "位置", "交易状况", "县市", "区域", "地区", "所在地", "地块评估&gt;&gt; 地块编号", "地块编号").OrderBy(c => c.Text("地区")).ToList();//土地url

            foreach (var cityObj in cityList)
            {
                var regionList = cityUrlList.Where(c => c.Text(fieldName) == cityObj.Text(fieldName) && c.Text("type") == "2").ToList();
                if (regionList.Count <= 0 && noRegion)
                {
                    regionList.Add(new BsonDocument().Add("name", defaultRegionName));
                }
                var hitLandUrlList = landUrlList.Where(c => cityObj.Text("name") == c.Text("所在地") || cityObj.Text("name") == c.Text("所在地") + "市" || cityObj.Text("name") == c.Text("地区") || cityObj.Text("name") == c.Text("地区") + "市").ToList();
                if (hitLandUrlList.Count() > 0)
                {
                    foreach (var land in hitLandUrlList)
                    {
                        var landName = land.Text("name");
                        var distinctName = land.Text("所在地");
                        var landCode = land.Text("地块编号");//如萧土
                        var position = land.Text("位置");//如萧土
                        if (string.IsNullOrEmpty(landCode))
                        {
                            landCode = land.Text("地块评估&gt;&gt; 地块编号");//如萧土
                        }
                        var regionName = regionList.Where(c => landName.Contains(c.Text("name").Replace("区", "").Replace("县", "").Replace("市", ""))).FirstOrDefault();

                        if (regionName == null)
                        {
                            regionName = regionList.Where(c => c.Text("remark") != "" && c.Text("remark").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Any(d => landCode.Contains(d))).FirstOrDefault();
                        }
                        if (regionName == null)
                        {
                            regionName = regionList.Where(c => c.Text("remark") != "" && c.Text("remark").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Any(d => landName.Contains(d))).FirstOrDefault();
                            //regionName = regionList.Where(c => distinctName.Contains(c.Text("name"))).FirstOrDefault();
                        }
                        if (regionName == null)
                        {
                            regionName = regionList.Where(c => c.Text("remark") != "" && c.Text("remark").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Any(d => position.Contains(d))).FirstOrDefault();
                            //regionName = regionList.Where(c => distinctName.Contains(c.Text("name"))).FirstOrDefault();
                        }
                        if (regionName == null && noRegion)
                        {
                            regionName = new BsonDocument().Add("name",defaultRegionName);
                        }
                        var updateBson = new BsonDocument();
                        if (regionName == null)
                        {
                            continue;
                        }
                        else
                        {
                            updateBson.Add("县市", regionName.Text("name"));
                            updateBson.Add("needUpdate", "1");
                        }
                        DBChangeQueue.Instance.EnQueue(new StorageData() { Name = "LandFang", Document = updateBson, Query = Query.EQ("url", land.Text("url")), Type = StorageType.Update });
                        this.richTextBox2.AppendText(land.Text("name") + "_" + regionName.Text("name") + "\n");
                    }
                    StartDBChangeProcess(dataop);
                }
            }
        }

        public class WeatherJson
        {
            public string _id;

        }


        public class EnterpriseResult
        {
            public string status { get; set; }
            public EnterpriseData data { get; set; }
        }
        public class EnterpriseData
        {
            public string status { get; set; }
            public string message { get; set; }
            public EnterpriseInfo data { get; set; }
        }

        public class EnterpriseInfo
        {
            public Info info { get; set; }
            public string node_info { get; set; }
        }
        public class Info
        {
            /// <summary>
            /// 企业guid主键
            /// </summary>
            public string id { get; set; }
            /// <summary>
            /// 注册编号
            /// </summary>
            public string reg_no { get; set; }
            /// <summary>
            /// 资质编号
            /// </summary>
            public string credit_no { get; set; }
            /// <summary>
            /// 企业编号
            /// </summary>
            public string org_no { get; set; }
            /// <summary>
            /// 名称
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// 法定代表人
            /// </summary>
            public string oper_name { get; set; }
            /// <summary>
            /// 成立日期
            /// </summary>
            public string start_date { get; set; }
            /// <summary>
            /// 状态
            /// </summary>
            public string status { get; set; }
            /// <summary>
            /// 简写
            /// </summary>
            public string short_name { get; set; }
            /// <summary>
            /// 地址
            /// </summary>
            public string address { get; set; }
            /// <summary>
            /// 电话
            /// </summary>
            public string telephone { get; set; }
            /// <summary>
            /// 注册资本
            /// </summary>
            public string reg_capi { get; set; }
            /// <summary>
            /// 注册资本单位
            /// </summary>
            public string capi_unit { get; set; }
            /// <summary>
            /// 注册资本描述
            /// </summary>
            public string reg_capi_desc { get; set; }
            /// <summary>
            /// 行业领域
            /// </summary>
            public string domain { get; set; }
            /// <summary>
            /// 省份ZJ浙江
            /// </summary>
            public string province { get; set; }
            /// <summary>
            /// 是否有问题
            /// </summary>
            public string has_problem { get; set; }
        }
        public class userRelation
        {
            public string id { get; set; }
            public string name { get; set; }
            public string short_name { get; set; }
            public string title { get; set; }
            public string has_problem { get; set; }
            public string reg_capi { get; set; }
            public string real_capi { get; set; }
            public string shareholding_ratio { get; set; }
            public string type { get; set; }

        }

        public class enterpriseRelation
        {
            public string id { get; set; }
            public string name { get; set; }
            public string short_name { get; set; }
            public string title { get; set; }
            public string status { get; set; }
            public string has_problem { get; set; }
            public string investment_time { get; set; }
            public string reg_capi { get; set; }
            public string real_capi { get; set; }
            public string domain { get; set; }
            /// <summary>
            /// 疑似关系
            /// </summary>
            public string related_by { get; set; }
        }

        private void button20_Click(object sender, EventArgs e)
        {

            string connStr = "mongodb://59.61.72.35:27018/ProductData";
            DataOperation dataop = new DataOperation(new MongoOperation(connStr));

            string logConnStr = "mongodb://59.61.72.35:27018/LogDB";
            DataOperation logDataop = new DataOperation(new MongoOperation(logConnStr));
            var beginDate = DateTime.Parse("2016-06-07 16:00:00");
            var SysLogDataList = logDataop.FindAllByQuery("SysLogData", Query.EQ("logType", 4)).Where(c => c.Date("logTime") >= beginDate).ToList();//城市url
            List<StorageData> UpdateList = new List<StorageData>();
            foreach (var log in SysLogDataList)
            {
                // var ids = log.BsonDocumentList("data");
                var ser = new DataContractJsonSerializer(typeof(List<WeatherJson>));
                var ms = new MemoryStream(Encoding.UTF8.GetBytes(log.Text("data")));
                List<WeatherJson> ids = (List<WeatherJson>)ser.ReadObject(ms);
                foreach (var catId in ids)
                {
                    UpdateList.Add(new StorageData() { Name = "ProjDocCategory", Document = new BsonDocument().Add("_isDelete", 0), Query = Query.EQ("_id", TypeConvert.ToObjectId(catId._id)), Type = StorageType.Update });
                }
            }
            if (UpdateList.Count() > 0)
            {
                dataop.BatchSaveStorageData(UpdateList);
            }

            //var allProjdocCategory = dataop.FindAllByQuery("ProjDocCategory", Query.EQ("_isDelete", 1)).ToList();//城市url


            // //57482bc1e4b5b11aacd36c4a
            // //5756698c2423e6bafc1ca720

            //var root = allProjdocCategory.Where(c =>(c.Text("_id_Project") == "57482bc1e4b5b11aacd36c4a" || c.Text("_id_Project") == "5756698c2423e6bafc1ca720")&&c.Text("name")!="").ToList();
            //foreach (var cat in root)
            //{

            //}

        }

        private void button21_Click(object sender, EventArgs e)
        {
            var connStr = "mongodb://sa:dba@59.61.72.34/SimpleCrawler";
            DataOperation dataOp = new DataOperation(new MongoOperation(connStr));
            var keyNameStr = new string[] { "和", "与" };//个人"和", "与"
            var keyNameList = keyNameStr.ToList();
            var filterName = new string[]{"广东省知识产权研究与发展中心（广东省知识产权维权援助中心）","武汉导航与位置服务工业技术研究院有限责任公司",
                                            "武汉新能源接入装备与技术研究有限公司",
                                            "南昌与德置业发展有限公司",
                                            "斯凯孚（济南）轴承与精密技术产品有限公司",
                                            "商河县工程质量与安全生产监督站",
                                            "上海瑞与祺热交换器制造有限公司",
                                            "重庆与时实业（集团）有限公司",
                                            "重庆市与时房地产开发有限公司",
                                            "重庆资源与环境保护职业学院","武汉生物农业与健康安全研究院有限公司",
                                            "济南国和路桥工程有限公司商河新材料分公司",
                                            "张家港市常阴沙现代农业示范园区和管理委员",
                                            "济南国和路桥工程有限公司商河新材料分公司",
                                            "重庆嘉江房地产开发有限公司(中海和九龙仓",
            };

            var allQiXinEnterprise = dataOp.FindAllByQuery("QiXinEnterprise", Query.And(Query.NotIn("name", filterName.Select(c => (BsonValue)c)),
                Query.EQ("isFirst", "1"), Query.NE("status", "1"))).SetFields("name").Where(c => keyNameList.Any(d => c.Text("name").Contains(d))).Distinct().ToList();
            // var allQiXinEnterprise = dataOp.FindAllByQuery("QiXinEnterprise", Query.And(Query.NotIn("name", filterName.Select(c => (BsonValue)c)),
            //    Query.NE("status", "1"))).SetFields("name").Where(c => c.Text("name") == "林丽珠和吴永忠" && keyNameList.Any(d => c.Text("name").Contains(d))).Distinct().ToList();
            ////林丽珠和吴永忠
            foreach (var landObj in allQiXinEnterprise)
            {
                var land = landObj.Text("name");
                var landBson = new BsonDocument().Add("status", "1").Add("isSplited", "1");
                var enterPriseNameArray = land.Replace("自然人", "").Replace("(", "").Replace(")", "").Replace("（", "").Replace("）", "").Replace("()", "").Replace("（）", "").Replace("的联合体", "").Replace("联合体", "").Replace("的股东", "").Replace("联合竞买", "").Split(keyNameStr, StringSplitOptions.RemoveEmptyEntries);

                foreach (var name in enterPriseNameArray)
                {

                    var updateBson = new BsonDocument().Add("oldName", land).Add("name", name).Add("isSubName", "1").Add("isFirst", landObj.Text("isFirst"));
                    updateBson.Add("城市", landObj.Text("城市"));
                    if (name.Replace("个人", "").Length <= 3 && name != "万科")
                    {
                        updateBson.Add("isUser", "1");
                        updateBson.Add("status", "1");
                        if (landObj.Text("isUser") != "1")
                        {
                            landBson.Set("isUser", "1");
                            landBson.Set("status", "1");
                        }
                    }
                    Queue.Enqueue(new StorageData()
                    {
                        Document = updateBson,
                        Name = "QiXinEnterprise",
                        Type = StorageType.Insert,
                        //Query = Query.EQ("name", land)
                    });
                }
                Queue.Enqueue(new StorageData()
                {
                    Document = landBson,
                    Name = "QiXinEnterprise",
                    Type = StorageType.Update,
                    Query = Query.EQ("name", land)
                });
                if (Queue.Count() >= 200)
                {
                    SaveIpStatus();
                }
            }
            SaveIpStatus();
        }

        string[] filterDateStrList = new string[] { "11-05" };//过滤日期
        string amOrPm = "上午";
        DateTime beginTime = DateTime.Parse("08:00");
        DateTime endTime = DateTime.Parse("11:00");
        string reserveUrl = "http://www.xmsmjk.com/UrpOnline/Home/Doctor/C51ADEA961A224F083298ED66F5FD4B7";// 王依静


        private void button22_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.richTextBox2.Text) && this.richTextBox2.Text.Contains("www.xmsmjk.com"))
            {
                reserveUrl = this.richTextBox2.Text.Trim();
            }
            if (!string.IsNullOrEmpty(this.richTextBox1.Text) && this.richTextBox1.Text.Contains("-"))
            {
                filterDateStrList = new string[] { this.richTextBox1.Text.Trim() };
            }
            if (this.timer1.Enabled == false)
            {
                this.timer1.Enabled = true;
                this.timer1.Start();

            }
            else
            {
                this.timer1.Stop();
                this.timer1.Enabled = false;
                MessageBox.Show("暂停");
            }
        }
        //string reserveUrl = "http://www.xmsmjk.com/UrpOnline/Home/Doctor/9902D350E0B31C99143770F9C27552B2EAA5452DFFE51D280ED5A1D9A319CEDC";
        /// <summary>
        /// 跳转浏览器哎
        /// </summary>
        /// <param name="url"></param>
        public void JumpBrowerUrl(string url)
        {
            if (System.IO.File.Exists(this.textBox3.Text))
            {
                System.Diagnostics.Process.Start(this.textBox3.Text, url);
            }
            else
            {
                System.Diagnostics.Process.Start(@"C:\Users\Administrator\AppData\Local\XSkyWalker\Application\XSkyWalker.exe", url);
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Text = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            var curDateStr = DateTime.Now.ToString("MM-dd");
            DotNet.Utilities.HttpHelper http = new DotNet.Utilities.HttpHelper();

            //创建Httphelper参数对象
            DotNet.Utilities.HttpItem item = new DotNet.Utilities.HttpItem()
            {
                URL = reserveUrl,//陈翠雯 
                // URL = "http://www.xmsmjk.com/UrpOnline/Home/Doctor/9902D350E0B31C99143770F9C27552B2EAA5452DFFE51D280ED5A1D9A319CEDC",//陈翠雯 
                Method = "get",//URL     可选项 默认为Get   
                ContentType = "text/html",//返回类型    可选项有默认值 
                Timeout = 3000
            };

            //请求的返回值对象
            DotNet.Utilities.HttpResult result = http.GetHtml(item);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.LoadHtml(result.Html);
                var root = htmlDoc.DocumentNode;
                var dayStrList = new List<string>();
                //获取标题
                var datetable = root.SelectSingleNode("//div[@class='doctorsubscribe']/div[@class='datetable']/ul");
                if (datetable == null) return;
                foreach (var li in datetable.ChildNodes.Where(c => c.Name == "li").Skip(1))
                {
                    var day = li.FirstChild;
                    if (day == null) continue;
                    var dateStrArr = li.InnerText.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    if (dateStrArr.Length != 0)
                        dayStrList.Add(dateStrArr[0].Trim());
                }

                //获取是否有预约
                var doctorsubscribeDiv = root.SelectSingleNode("//div[@class='whliesubscribe']/ul");
                if (doctorsubscribeDiv == null) return;
                var doctorsubscribeDivNodeList = doctorsubscribeDiv.ChildNodes.Where(c => c.Name == "li").Skip(1).ToList();
                var hitNodeList = new List<HtmlAgilityPack.HtmlNode>();
                //根据时间过滤需要的字段
                if (filterDateStrList.Length > 0)
                {

                    foreach (var filterDateStr in filterDateStrList)
                    {
                        var index = dayStrList.IndexOf(filterDateStr);
                        if (index != -1)
                        {
                            hitNodeList.Add(doctorsubscribeDivNodeList[index]);
                        }
                    }
                }
                else
                {
                    hitNodeList = doctorsubscribeDivNodeList;
                }
                //开始遍历节点
                foreach (var div in hitNodeList)
                {
                    if (!div.InnerText.Contains("预约"))
                    {
                        continue;
                    }
                    var childList = div.ChildNodes.Where(c => c.Name == "div").ToList();
                    if (childList == null || childList.Count < 2) continue;
                    var amDiv = childList[0];
                    var pmDiv = childList[1];
                    switch (amOrPm)
                    {
                        case "上午":
                            if (DealReserveHtmlNode(amDiv)) return;
                            break;
                        case "下午":
                            if (DealReserveHtmlNode(pmDiv)) return;
                            break;
                        default:
                            if (DealReserveHtmlNode(amDiv)) return;
                            if (DealReserveHtmlNode(pmDiv)) return;
                            break;
                    }
                }
            }
        }
        /// <summary>
        /// 处理预约节点
        /// </summary>
        /// <param name="node"></param>
        public bool DealReserveHtmlNode(HtmlAgilityPack.HtmlNode node)
        {
            if (node.InnerText.Contains("预约"))
            {
                var reserveNode = node.SelectSingleNode("./span/a");
                if (reserveNode != null && reserveNode.Attributes.Contains("href"))
                {
                    var link = reserveNode.Attributes["href"].Value.ToString();
                    link = string.Format("http://www.xmsmjk.com/{0}", link);
                    if (!string.IsNullOrEmpty(link))
                    {

                        var timeSelectLink = ReserveTimeSelect(link);
                        if (!string.IsNullOrEmpty(timeSelectLink))
                        {
                            this.timer1.Stop();
                            this.timer1.Enabled = false;
                            this.timer2.Enabled = true;
                            this.timer2.Start();
                            JumpBrowerUrl(link);
                            JumpBrowerUrl(timeSelectLink);
                            SendMailMessage();
                            //MessageBox.Show("1");
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 选择时间进行提交
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string ReserveTimeSelect(string url)
        {

            var hitResultUrl = string.Empty;
            DotNet.Utilities.HttpHelper http = new DotNet.Utilities.HttpHelper();

            //创建Httphelper参数对象
            DotNet.Utilities.HttpItem item = new DotNet.Utilities.HttpItem()
            {
                // URL = "http://www.xmsmjk.com/UrpOnline/Home/Doctor/C51ADEA961A224F083298ED66F5FD4B7",//陈翠雯 
                URL = url,//陈翠雯 
                Method = "get",//URL     可选项 默认为Get   
                ContentType = "text/html",//返回类型    可选项有默认值 
                Timeout = 3000
            };

            //请求的返回值对象
            DotNet.Utilities.HttpResult result = http.GetHtml(item);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.LoadHtml(result.Html);
                var root = htmlDoc.DocumentNode;
                //找出可用的时间a
                var datetable = root.SelectSingleNode("//div[@class='dateInfoDetail']/div[@class='dateSpan']").ChildNodes.Where(c => c.Name == "a");
                foreach (var aNode in datetable)//遍历时间节点
                {
                    var dateArr = aNode.InnerText.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                    if (dateArr.Length == 2)
                    {
                        DateTime curBeginTime;
                        DateTime curEndTime;
                        if (!DateTime.TryParse(dateArr[0], out curBeginTime))
                        {
                            break;
                        }
                        if (!DateTime.TryParse(dateArr[1], out curEndTime))
                        {
                            break;
                        }
                        if (curBeginTime >= beginTime && curEndTime <= endTime)
                        {
                            if (aNode.Attributes.Contains("onclick"))
                            {
                                var linkStr = aNode.Attributes["onclick"].Value.ToString();
                                var beginIndex = linkStr.IndexOf("/");
                                var endIndex = linkStr.LastIndexOf("'");
                                var link = string.Empty;
                                if (beginIndex != -1 && endIndex != -1 && endIndex > beginIndex)
                                {
                                    link = linkStr.Substring(beginIndex, endIndex - beginIndex);
                                }
                                link = string.Format("http://www.xmsmjk.com/{0}", link);
                                return link;
                            }
                        }
                    }
                }
            }
            return hitResultUrl;
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        public void SendMailMessage()
        {
            string connStr = "mongodb://sa:dba@59.61.72.34/WorkPlanManage";
            DataOperation operation = new DataOperation(new MongoOperation(connStr));
            MessagePushQueueHelper msgHelper = new MessagePushQueueHelper(operation);
            msgHelper.PushMessage(new MessagePushEntity()
            {
                arrivedUserIds = "1",
                arrivedUserNames = "admin",
                content = "预约已到请尽快登陆查看",
                title = "预约已到请尽快登陆查看",
                sendUserId = "1",
                registerDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                ,
                sendType = "0",
                sendDate = DateTime.Now.AddSeconds(10).ToString("yyyy-MM-dd HH:mm:ss")
            });
            //msgHelper.PushMessage(new MessagePushEntity()
            //{
            //    arrivedUserIds = "77",
            //    arrivedUserNames = "ykp",
            //    content = "预约已到请尽快登陆查看",
            //    title = "预约已到请尽快登陆查看",
            //    sendUserId = "1",
            //    registerDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") ,
            //    sendType = "0",
            //    sendDate = DateTime.Now.AddSeconds(10).ToString("yyyy-MM-dd HH:mm:ss")
            //});
        }



        private void timer2_Tick(object sender, EventArgs e)
        {
            if (this.timer1.Enabled == false)
            {
                this.timer1.Enabled = true;
                this.timer1.Start();
                this.timer2.Stop();
            }
        }

        private void button23_Click(object sender, EventArgs e)
        {
            var connStr = "mongodb://sa:dba@59.61.72.34/SimpleCrawler";
            DataOperation dataOp = new DataOperation(new MongoOperation(connStr));
            MongoOperation _mongoDBOp = new MongoOperation(connStr);
            var connStr2 = "mongodb://sa:dba@192.168.1.114/CompanyHY";
            DataOperation sourceDataOp = new DataOperation(new MongoOperation(connStr2));
            var cityName = "成都";
            //已存在的企业列表
            var allQiXinEnterprise = dataOp.FindAllByQuery("QiXinEnterprise", Query.EQ("城市", cityName)).SetFields("name").Select(c => c.Text("name")).Distinct().ToList();
            //待添加的企业列表
            var allHitEnterpriseNameList = sourceDataOp.FindAllByQuery("SQ_chengdu_Company", Query.EQ("cityName", cityName)).ToList();//来源
            allHitEnterpriseNameList = allHitEnterpriseNameList.Where(c => !allQiXinEnterprise.Contains(c.Text("name"))).ToList();
            foreach (var enterpriseObj in allHitEnterpriseNameList)
            {
                var addBson = new BsonDocument().Add("城市", "成都").Add("省", "四川省").Add("name", enterpriseObj.Text("name")).Add("source", "SQ_chegndu_Company");
                addBson.Add("isFirst", "1");//优先查找
                DBChangeQueue.Instance.EnQueue(new StorageData() { Name = "QiXinEnterprise", Document = addBson, Type = StorageType.Insert });
            }

            StartDBChangeProcessQuick(_mongoDBOp);
        }

        private void button24_Click(object sender, EventArgs e)
        {
            var connStr = "mongodb://sa:dba@59.61.72.34/SimpleCrawler";
            DataOperation dataOp = new DataOperation(new MongoOperation(connStr));
            MongoOperation _mongoDBOp = new MongoOperation(connStr);
            var connStr2 = "mongodb://sa:dba@192.168.1.43/SimpleCrawler";
            DataOperation sourceDataOp = new DataOperation(new MongoOperation(connStr2));
            var cityNameArr = new string[] { "北京", "上海", "广州", "深圳" };
            var cityName = "深圳";
            var provinceName = "广州省";
            //已存在的企业列表
            var allQiXinEnterprise = dataOp.FindAllByQuery("QiXinEnterprise", Query.In("城市", cityNameArr.Select(c => (BsonValue)c))).SetFields("name").Select(c => c.Text("name")).Distinct().ToList();
            //待添加的企业列表
            var likeStr = string.Format("/{0}/", cityName);
            var allHitEnterpriseNameList = sourceDataOp.FindAllByQuery("EnterpriseCollection", Query.Or(Query.Matches("cityName", likeStr), Query.Matches("companyName", likeStr))).SetFields("companyName").ToList();//来源
            allHitEnterpriseNameList = allHitEnterpriseNameList.Where(c => !allQiXinEnterprise.Contains(c.Text("companyName"))).ToList();
            foreach (var enterpriseObj in allHitEnterpriseNameList)
            {
                var addBson = new BsonDocument().Add("城市", cityName).Add("省", provinceName).Add("name", enterpriseObj.Text("companyName")).Add("source", "_51JobZLEnterprise");
                addBson.Add("isFirst", "1");//优先查找
                DBChangeQueue.Instance.EnQueue(new StorageData() { Name = "QiXinEnterprise", Document = addBson, Type = StorageType.Insert });
            }

            StartDBChangeProcessQuick(_mongoDBOp);
            MessageBox.Show("ok");
        }

        private void button25_Click(object sender, EventArgs e)
        {
            var connStr = "mongodb://sa:dba@59.61.72.34/SimpleCrawler";
            DataOperation dataOp = new DataOperation(new MongoOperation(connStr));
            MongoOperation _mongoDBOp = new MongoOperation(connStr);
            var allAccountNameList = dataOp.FindAll("QCCEnterpriseKey").SetFields("url").Select(c => c.Text("url")).ToList();
            foreach (var url in allAccountNameList)
            {
                var guid = GetGuidFromUrl(url);
                if (!string.IsNullOrEmpty(guid))
                {
                    DBChangeQueue.Instance.EnQueue(new StorageData() { Name = "QCCEnterpriseKey", Document = new BsonDocument().Add("guid", guid), Type = StorageType.Update, Query = Query.EQ("url", url) });
                }
            }
            StartDBChangeProcessQuick(_mongoDBOp);
            MessageBox.Show("ok");
        }
        private static string GetGuidFromUrl(string url)
        {
            var beginStrIndex = url.LastIndexOf("_");
            var endStrIndex = url.IndexOf(".");
            if (beginStrIndex != -1 && endStrIndex != -1)
            {
                var queryStr = url.Substring(beginStrIndex + 1, endStrIndex - beginStrIndex - 1);
                return queryStr;
            }
            return string.Empty;
        }

        private void button3_Click(object sender, EventArgs e)
        {

            DotNet.Utilities.HttpHelper http = new DotNet.Utilities.HttpHelper();

            //创建Httphelper参数对象
            DotNet.Utilities.HttpItem item = new DotNet.Utilities.HttpItem()
            {
                URL = "http://www.tianyancha.com/company/179567089.json",//URL     必需项    

                ContentType = "application/x-www-form-urlencoded; charset=UTF-8",//返回类型    可选项有默认值 

                Timeout = 1500,
                Accept = "application/json, text/plain, */*",
                Encoding = null,//编码格式（utf-8,gb2312,gbk）     可选项 默认类会自动识别
                //Encoding = Encoding.Default,

                //Timeout = 100000,//连接超时时间     可选项默认为100000
                //ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000
                //IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写
                //Cookie = "",//字符串Cookie     可选项
                UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/32.0.1653.0 Safari/537.36",//用户的浏览器类型，版本，操作系统     可选项有默认值
                //ContentType = "application/x-www-form-urlencoded",//返回类型    可选项有默认值
                Referer = "http://www.tianyancha.com/company/179567089",//来源URL     可选项

                Allowautoredirect = true,
                Cookie = "TYCID=369d82abe41d4d68955b7a6e73ef7434; tnet=59.61.72.34; _pk_ref.1.e431=%5B%22%22%2C%22%22%2C1470621159%2C%22http%3A%2F%2Fcn.bing.com%2Fsearch%3Fq%3D%E5%A4%A9%E7%9C%BC%E6%9F%A5%26go%3D%E6%8F%90%E4%BA%A4%26qs%3Dn%26form%3DQBLH%26pq%3D%E5%A4%A9%E7%9C%BC%E6%9F%A5%26sc%3D5-5%26sp%3D-1%26sk%3D%26cvid%3D23445AA5B14045568BFB1F5314CD1499%22%5D; _pk_id.1.e431=17aad35d6984da76.1464775176.5.1470621513.1470621159.; _pk_ses.1.e431=*; Hm_lvt_e92c8d65d92d534b0fc290df538b4758=1469685359,1470621160; Hm_lpvt_e92c8d65d92d534b0fc290df538b4758=1470621513; token=76cc798970b94eb19c0cefc86cf5ee06; _utm=1df3eab12d024829aeb61d48c253ef50"

            };

            //请求的返回值对象
            DotNet.Utilities.HttpResult result = http.GetHtml(item);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                this.richTextBox1.Text = FromUnicodeString(result.Html);
            }
        }
        public string ToUnicodeString(string str)
        {
            StringBuilder strResult = new StringBuilder();
            if (!string.IsNullOrEmpty(str))
            {
                for (int i = 0; i < str.Length; i++)
                {
                    strResult.Append("\\u");
                    strResult.Append(((int)str[i]).ToString("x"));
                }
            }
            return strResult.ToString();
        }

        public string FromUnicodeString(string str)
        {
            //最直接的方法Regex.Unescape(str);
            StringBuilder strResult = new StringBuilder();
            if (!string.IsNullOrEmpty(str))
            {
                string[] strlist = str.Replace("\\", "").Split('u');
                try
                {
                    for (int i = 1; i < strlist.Length; i++)
                    {
                        int charCode = Convert.ToInt32(strlist[i], 16);
                        strResult.Append((char)charCode);
                    }
                }
                catch (FormatException ex)
                {
                    return Regex.Unescape(str);
                }
            }
            return strResult.ToString();
        }
        public static int succeedTimes = 0;
        public static int faildTimes = 0;
        private void button26_Click(object sender, EventArgs e)
        {
            //119.57.149.36:80
            //122.96.59.107:80
            //60.191.164.83:3128


            // 代理服务器
            string proxyHost = "http://proxy.abuyun.com";
            string proxyPort = "9010";

            // 代理隧道验证信息
            string proxyUser = "H8S7986H534384GP";
            string proxyPass = "A7E5939C570FAC8E";
            // 设置代理服务器
            var proxy = new WebProxy();
            proxy.Address = new Uri(string.Format("{0}:{1}", proxyHost, proxyPort));
            proxy.Credentials = new NetworkCredential(proxyUser, proxyPass);

            var url = "http://www.qichacha.com/company_getinfos?unique=ba5aee7262c8c370102586275c35cb6c&companyname=北京万子营餐厅&tab=base";

            //var url = "http://www.baidu.com/";   
            DotNet.Utilities.HttpHelper http = new DotNet.Utilities.HttpHelper();
            succeedTimes = 0; faildTimes = 0;
            DotNet.Utilities.HttpItem item = new DotNet.Utilities.HttpItem()
                    {
                        //URL = "http://luckymn.cn",//URL     必需项 
                        URL = url,
                        Method = "get",//URL     可选项 默认为Get   
                        ContentType = "text/html",//返回类型    可选项有默认值 
                        WebProxy = proxy,
                        Timeout = 2000,
                        Cookie = "gr_user_id=a9035e34-8609-47cf-90e9-7511d7ea1f8e; PHPSESSID=c36nbn2cpf76ehh4s238vu8oc1; CNZZDATA1254842228=615523300-1469528200-http%253A%252F%252Fcn.bing.com%252F%7C1470727108; gr_session_id_9c1eb7420511f8b2=b4a26f44-0387-4c8b-84fc-4b2be5036a62"
                    };

            //请求的返回值对象
            DotNet.Utilities.HttpResult result = http.GetHtml(item);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                this.richTextBox2.Text = result.Html;
            }


        }

        private void button27_Click(object sender, EventArgs e)
        {
            var connStr = "mongodb://sa:dba@192.168.1.159/SimpleCrawler";
            MongoConnectionStringBuilder builder = new MongoConnectionStringBuilder();
            builder.Server = new MongoServerAddress("192.168.1.159", 27017);
            builder.DatabaseName = "SimpleCrawler";
            builder.Username = "sa";
            builder.Password = "dba";
            builder.SocketTimeout = new TimeSpan(00, 01, 59);
            DataOperation dataOp = new DataOperation(new MongoOperation(builder));
            MongoOperation _mongoDBOp = new MongoOperation(builder);
            //////isAnalyse
            //var isAnalyseCount = dataOp.FindCount("QCCEnterpriseDetailInfo", Query.EQ("isAnalyse", "1"));
            //  var detailInfoCount = dataOp.FindCount("QCCEnterpriseKey", Query.And(Query.EQ("cityName", "苏州")));
            //var detailInfoCount2 = dataOp.FindCount("QCCEnterpriseKey", Query.And(Query.EQ("cityName", this.richTextBox1.Text)));
            //this.richTextBox2.Text = detailInfoCount2.ToString();
            //return;
            //var detailInfoCount2 = dataOp.FindCount("QCCEnterpriseKey", Query.And(Query.EQ("cityName", "长沙")));
            //var detailInfoCount2 = dataOp.FindCount("QCCEnterpriseKey", Query.And(Query.EQ("cityName", "重庆"), Query.EQ("isUserUpdate", "1")));
            var tableName = "_51JobCompany";
            var guid = "guid";

            //var allEnterpriseGuidList = dataOp.FindAllByQuery("QCCEnterpriseKey", Query.EQ("cityName", "西安")).SetFields("guid", "_id").Take(2000000).ToList();
            var allEnterpriseGuidList = dataOp.FindAll(tableName).SetFields(guid, "_id", "timestamp").Take(2000000).OrderByDescending(c => c.Text("timestamp")).ToList();
            //return;
            //var hitResult = from c in allEnterpriseGuidList
            //                group c by c.Text("guid") into g
            //                where g.Count() >= 2
            //                select new { Key = g.Key, count = g.Count() };
            //var curResult = hitResult.ToList();
            //var keyList = hitResult.Select(c => c.Key).ToList();

            BloomFilter<string> noDeleteFilter = new BloomFilter<string>(3000000);
            var deleteList = new List<string>();
            var deleteGuidList = new List<string>();
            foreach (var hitEnterprise in allEnterpriseGuidList)
            {
                if (!noDeleteFilter.Contains(hitEnterprise.Text(guid)))
                {
                    noDeleteFilter.Add(hitEnterprise.Text(guid));
                }
                else//已包含
                {
                    //if (keyList.Contains(hitEnterprise.Text("guid")))
                    {
                        deleteList.Add(hitEnterprise.Text("_id"));
                        deleteGuidList.Add(hitEnterprise.Text(guid));
                    }
                }
            }

            foreach (var deleteId in deleteList)
            {
                _mongoDBOp.Delete(tableName, Query.EQ("_id", ObjectId.Parse(deleteId)));
                // DBChangeQueue.Instance.EnQueue(new StorageData() { Name = "QCCEnterpriseKey", Query = Query.EQ("_id", ObjectId.Parse(deleteId)), Type = StorageType.Delete });
            }


            StartDBChangeProcessQuick(_mongoDBOp);

            //var enterpriseDataop = new DataOperation(new MongoOperation(enterpriseNameConnStr));
            //var hitEnterpriseName = enterpriseDataop.FindAllByQuery("QiXinEnterprise", Query.And(Query.NE("isSplited", "1"), Query.NE("isUser", "1"))).SetFields("name").ToList();

        }

        private void button28_Click(object sender, EventArgs e)
        {
            var connStr = "mongodb://sa:dba@192.168.1.230/SimpleCrawler";
            MongoConnectionStringBuilder builder = new MongoConnectionStringBuilder();
            builder.Server = new MongoServerAddress("192.168.1.230", 27017);
            builder.DatabaseName = "SimpleCrawler";
            builder.Username = "sa";
            builder.Password = "dba";
            builder.SocketTimeout = new TimeSpan(00, 01, 59);
            DataOperation dataOp = new DataOperation(new MongoOperation(builder));
            MongoOperation _mongoDBOp = new MongoOperation(builder);
            var sb = new StringBuilder();
            var result = GetSplitStr(new string[] { "\n" });
            foreach (var enterprise in result)
            {
                var name = enterprise.Trim();
                DBChangeQueue.Instance.EnQueue(new StorageData() { Name = "QCCEnterprise_XiAn", Document = new BsonDocument().Add("name", name), Type = StorageType.Insert });
            }

            StartDBChangeProcessQuick(_mongoDBOp);
        }


        private string[] GetSplitStr(string[] splitStr)
        {
            var textArr = this.richTextBox1.Text.Split(splitStr, StringSplitOptions.RemoveEmptyEntries);
            return textArr;
        }

        private void button29_Click(object sender, EventArgs e)
        {
            var phone = this.richTextBox1.Text;
            curResult = PassEnterpriseInfoGeetestChart(phone);

        }
        #region 企查查验证码
        LibCurlNet.HttpInput hi = new LibCurlNet.HttpInput();
        GeetestResult curResult = new GeetestResult();

        /// <summary>
        /// 过企业信息chart验证码
        /// </summary>
        /// <returns></returns>
        public GeetestResult PassEnterpriseInfoGeetestChart(string phoneNum)
        {
            LibCurlNet.HttpManager.Instance.InitWebClient(hi, true, 30, 30);

            //一个时刻只能一个实例运行
            var geetestHelper = new PassGeetestHelper();
            var validUrl = "";
            var postFormat = "geetest_challenge={0}&geetest_validate={1}&geetest_seccode={1}%7Cjordan";
            geetestHelper.GetCapUrl = "http://www.qichacha.com/index_getcap?rand=t={0}&_={0}";
            var passResult = geetestHelper.PassGeetest(hi, postFormat, validUrl, "");
            if (passResult.Status)
            {
                hi.Url = "http://www.qichacha.com/user_regmobileCode";
                hi.Refer = "http://www.qichacha.com/user_login";
                hi.PostData = string.Format("phone={0}&type={1}&geetest_challenge={2}&geetest_validate={3}&geetest_seccode={3}%7Cjordan", phoneNum, 3, passResult.Challenge, passResult.ValidCode);
                var ho = LibCurlNet.HttpManager.Instance.ProcessRequest(hi);
                if (ho.IsOK)
                {
                    return passResult;
                }

            }
            return new GeetestResult();
        }
        /// <summary>
        /// 自动登陆
        /// </summary>
        /// <returns></returns>
        private bool GeetestChartAccountReg(GeetestResult passResult, string phone, string pswd, string mobilecode)
        {

            var geetestHelper = new PassGeetestHelper();

            if (passResult.Status)
            {

                hi.Url = "http://www.qichacha.com/user_registAction";
                hi.Refer = "http://www.qichacha.com/user_login";
                hi.PostData = string.Format("phone={0}&pswd={1}&geetest_challenge={2}&geetest_validate={3}&geetest_seccode={3}%7Cjordan&mobilecode={4}", phone, pswd, passResult.Challenge, passResult.ValidCode, mobilecode);
                var ho = LibCurlNet.HttpManager.Instance.ProcessRequest(hi);
                if (ho.IsOK)
                {
                    if (ho.TxtData.Contains("true"))
                    {

                        return true;
                    }
                }


            }
            return false;
        }
        #endregion

        private void button30_Click(object sender, EventArgs e)
        {
            var result = GeetestChartAccountReg(curResult, this.richTextBox1.Text, "qwer1234", this.richTextBox2.Text);
            MessageBox.Show(result.ToString());

        }
        //  public string accessToken = "Bearer NGJlN2ZlMDUtMTkzYi00MTQ1LTlmZWItMjIyZWRjYTFiMmM2";
        public string deviceToken = "4d0aa9d91df7d815b12f6736a708ef81129728d3";
        public string appId = "80c9ef0fb86369cd25f90af27ef53a9e";
        //public string refreshToken = "92c646ec1713ddc75d6cf9b1ca998d41";
        //public string accessToken = "Bearer ZDhiYjZkNGEtNzNlNy00MzYzLWExNDgtYjJjNWZlNDM0YzI1";
        //public string deviceId = "VyMDBRAKK88DAMMz1sdnhPOP";
        //public string timestamp = "1474872718587";
        //public string sign = "538d5de7e0facb048e35a5df53fd50c016c29f03";



        public class DeviceInfo
        {
            public string appId { get; set; }
            public string deviceToken { get; set; }
            public string refreshToken { get; set; }
            public string accessToken { get; set; }
            public string deviceId { get; set; }
            public string timestamp { get; set; }
            public string sign { get; set; }
            public string isBusy { get; set; }
            public override string ToString()
            {
                return string.Format("appId:{0}\r deviceId={1}\r timestamp={2}\r sign={3} \r refreshToken={4} \r accessToken={5}\r  isBusy:{6} ", appId, deviceId, timestamp, sign, refreshToken, accessToken, isBusy);
            }



        }
        public DeviceInfo curDeviceInfo = new DeviceInfo();
        /// <summary>
        /// 更新设备token刷新
        /// </summary>
        public void SaveDeviceToken()
        {
            var updateBson = new BsonDocument();
            updateBson.Add("refreshToken", curDeviceInfo.refreshToken);
            updateBson.Add("accessToken", curDeviceInfo.accessToken);
            DBChangeQueue.Instance.EnQueue(new StorageData() { Name = "QCCDeviceAccount", Document = updateBson, Type = StorageType.Update, Query = Query.EQ("deviceId", curDeviceInfo.deviceId) });
            StartDBChangeProcessQuick(null);
        }
        public void DeleteDeviceToken()
        {
            DBChangeQueue.Instance.EnQueue(new StorageData() { Document = new BsonDocument().Add("isInvalid", "1"), Query = Query.EQ("deviceId", curDeviceInfo.deviceId), Name = "QCCDeviceAccount", Type = StorageType.Update });
            StartDBChangeProcessQuick(null);
            ShowMessageInfo("删除设备");
        }
        public void InitDeviceInfo()
        {
            curDeviceInfo.appId = "80c9ef0fb86369cd25f90af27ef53a9e";
            curDeviceInfo.refreshToken = "b30c9f1ab0a094668001463e63a2a561";
            curDeviceInfo.accessToken = "Bearer OGM2M2YzZmYtZDg3Zi00OGIzLWE1ODMtMWRmNWNiZDY1NGNl";
            curDeviceInfo.deviceId = "rtxepUOxiEaxlIqsBJJ7vRiq";
            curDeviceInfo.timestamp = "1477038756509";
            curDeviceInfo.sign = "e647d5856a52e8ee6ce5d5f7adbd646575a5984c";
        }
        private void button31_Click(object sender, EventArgs e)
        {

            // var result = TestAPPLogin();
            // if (result.Html.Contains("失效"))
            // {
            //     RefreshToken();//刷新token

            //     if (result.Html.Contains("成功"))
            //     {
            //         var token = Toolslib.Str.Sub(result.Html, "access_token\":\"", "\"");
            //         if (!string.IsNullOrEmpty(token))
            //             curDeviceInfo.accessToken = string.Format("Bearer {0}", token);
            //         result = TestAPPLogin();//重新登陆
            //         this.richTextBox1.Text = result.Html;

            //     }
            // }
            //if (result.Html.Contains("access_token"))
            //{
            //    var token = Toolslib.Str.Sub(result.Html, "access_token\":\"", "\"");
            //    if (!string.IsNullOrEmpty(token))
            //        curDeviceInfo.accessToken = string.Format("Bearer {0}", token);
            //}
            // var url = new UrlInfo("https://app.qichacha.net/app/v1/base/advancedSearch?searchKey=%E5%8E%A6%E9%97%A8%20%E8%AE%A1%E7%AE%97%E6%9C%BA&searchIndex=default&province=FJ&cityCode=2&statusCode=10&startDateYear=1995&isSortAsc=false&industryCode=I&subIndustryCode=64&timestamp=1473158622274&sign=7552bb78a2d96111234c93b0e34bd3ab55511646&p=2");
            var province = "GD";
            var cityCode = "1";
            var statusCode = "enterpriseIp";
            var registCapiBegin = "500";
            var registCapiEnd = "500";
            var startDateBegin = "20160101";
            var startDateEnd = "20161230";
            var searchKey = "{%22scope%22:%22ningye%22}";
            var conditionStr = string.Format("province={0}&cityCode={1}&statusCode={2}&registCapiBegin={3}&registCapiEnd={4}&isSortAsc=false&startDateBegin={5}&startDateEnd={6}", province, cityCode, statusCode, registCapiBegin, registCapiEnd, startDateBegin, startDateEnd);
            //var url = new UrlInfo(string.Format("https://app.qichacha.net/app/v1/base/advancedSearch?searchKey={3}&searchIndex=multicondition&{2}&isSortAsc=false&industryCode=A&subIndustryCode=&timestamp={0}&sign={1}", curDeviceInfo.timestamp, curDeviceInfo.sign, conditionStr, searchKey));  
            //var url = new UrlInfo(string.Format("https://app.qichacha.net/app/v1/base/advancedSearch?searchKey={}&searchIndex=default&province=GD&cityCode=1&statuscode=&registCapiBegin=0&registCapiEnd=500&isSortAsc=false&industryCode=A&subIndustryCode=&timestamp={0}&sign={1}", curDeviceInfo.timestamp, curDeviceInfo.sign));
            searchKey = string.Format("\"scope\":\"{0}\"", "计算机");
            searchKey = "{" + HttpUtility.UrlEncode(searchKey) + "}";
            var uStr = string.Format("https://app.qichacha.net/app/v1/base/advancedSearch?searchKey={0}&searchIndex=multicondition&pageIndex=1&isSortAsc=false&industryCode=L&subIndustryCode=&timestamp={1}&sign={2}", searchKey, curDeviceInfo.timestamp, curDeviceInfo.sign);
            //企业详细
            // var uStr = string.Format("https://app.qichacha.net/app/v1/base/getEntDetail?unique=fa3d9de6fd3ccf354b5ae34c38eb7587&timestamp={1}&sign={2}", searchKey, curDeviceInfo.timestamp, curDeviceInfo.sign);
            // 投资关系
            //var uStr = string.Format("https://app.qichacha.net/app/v1/zscq/getTrademarkList?searchKey=%E6%AD%A6%E6%B1%89%E6%A2%A6%E4%BC%8A%E4%BB%95%E5%AF%9D%E9%A5%B0%E6%9C%89%E9%99%90%E5%85%AC%E5%8F%B8&pageIndex=1&sortField=&isSortAsc=false&timestamp=1477042955819&sign=21de9c139cc32c1687d4393795e309006061c376", searchKey, curDeviceInfo.timestamp, curDeviceInfo.sign, curDeviceInfo.accessToken.Replace("Bearer", "").Trim());

            var url = new UrlInfo(uStr);

            if (!string.IsNullOrEmpty(this.richTextBox1.Text) && this.richTextBox1.Text.Contains("https"))
            {
                url = new UrlInfo(this.richTextBox1.Text);
            }
            //1946index=0
            if (string.IsNullOrEmpty(curDeviceInfo.accessToken))
            {
                RefreshToken();
            }
            var content = GetHttpHtml(url).Html;
            if (!content.Contains("成功"))
            {
                if (content.Contains("失效") || content.Contains("无效") || content.Contains("非法请求") || content.Contains("签名失败"))
                {
                    RefreshToken();
                    content = GetHttpHtml(url).Html;
                }
                else
                {
                    RefreshToken();
                    content = GetHttpHtml(url).Html;
                }
            }

            if (!content.Contains("成功") && content.Contains("异常"))
            {
                DeleteDeviceToken();
            }
            this.richTextBox2.Text = content;
        }
        #region 题库载入
        //private void button23_Click(object sender, EventArgs e)
        //{
        //    var txt = this.richTextBox1.Text;
        //    txt = txt.Replace("A．", "A、").Replace("B．", "B、").Replace("C．", "C、").Replace("D．", "D、").Replace("E．", "E、");
        //    txt = txt.Replace("A.", "A、").Replace("B.", "B、").Replace("C.", "C、").Replace("D.", "D、").Replace("E.", "E、");
        //    //txt = Regex.Replace(txt, @"(\d+.)[^\d]+", @"$1、");  
        //    //将每行开头的数字进行截取替换
        //    var strArray = txt.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
        //    var result = new StringBuilder();
        //    foreach (var str in strArray)
        //    {

        //        if (str.Contains("."))
        //        {
        //            var preText = str.Substring(0, 4);
        //            if (preText.Contains("A") || preText.Contains("B") || preText.Contains("C") || preText.Contains("D"))
        //            {
        //                result.AppendFormat(str + "\n");
        //                continue;
        //            }
        //            for (var i = 0; i <= 9; i++)
        //            {
        //                var changeStr = string.Format("{0}.", i);
        //                var needStr = string.Format("{0}、", i);
        //                preText = preText.Replace(changeStr, needStr);
        //            }
        //            result.AppendFormat("{0}{1}\n", preText, str.Substring(4, str.Length - 4));
        //        }
        //        else
        //        {
        //            result.AppendFormat(str + "\n");
        //        }
        //    }

        //    ////可能导致吧答案中的1.5误认为答案开头 4.案例


        //    this.richTextBox2.Text = result.ToString();
        //}

        //private void button24_Click(object sender, EventArgs e)
        //{
        //    var libararyName = "施工员（装饰装修）专业知识练习题（重点掌握类）";
        //    var type = "重点掌握类";
        //    string path = string.Format(@"C:\Users\MN\Desktop\{0}.txt", libararyName);
        //    StreamReader reader = null;
        //    List<Question> QuestionList = new List<Question>();

        //    reader = new StreamReader(path);
        //    Question question = null;
        //    var groupIndex = QuestionGroup.Singel;
        //    var QuestionIndex = 0;
        //    for (string textInfo = reader.ReadLine(); textInfo != null; )
        //    {

        //        if (string.IsNullOrEmpty(textInfo)) continue;
        //        textInfo = TextFix(textInfo);
        //        //该行包含
        //        if (GroupStart(textInfo))//生成question新实例
        //        {
        //            groupIndex = GetQuestionGroup(textInfo);
        //            textInfo = reader.ReadLine();
        //            continue;//进入下一轮读取
        //        }

        //        if (IsQuestionStart(textInfo))//是否答案开始
        //        {
        //            var curQuestionStr = new StringBuilder();
        //            curQuestionStr.Append(textInfo);
        //            do
        //            {
        //                textInfo = reader.ReadLine();
        //                textInfo = TextFix(textInfo);
        //                if (!IsQuestionStart(textInfo) && !GroupStart(textInfo) && !InstanceStart(textInfo))
        //                {
        //                    if (OptionStart(textInfo))
        //                    {
        //                        curQuestionStr.AppendFormat("|Y|{0}", textInfo);
        //                    }
        //                    else
        //                    {
        //                        curQuestionStr.AppendFormat("{0}", textInfo);
        //                    }
        //                }
        //            }
        //            while (textInfo != null && !IsQuestionStart(textInfo) && !GroupStart(textInfo));
        //            question = QuestionInit(curQuestionStr.ToString());
        //            question.group = groupIndex;
        //            QuestionList.Add(question);
        //            //解析此处curQuestionStr是完整的question字符串
        //            continue;
        //        }

        //        textInfo = reader.ReadLine();

        //    }//文件读取结束

        //    reader.Close();

        //    //添加到数据库
        //    string connStr = "mongodb://sa:dba@59.61.72.34/WorkPlanManage";
        //    DataOperation operation = new DataOperation(new MongoOperation(connStr));

        //    var hitLib = operation.FindOneByKeyVal("WPM_QuestionLibrary", "name", libararyName);
        //    var libId = string.Empty;
        //    if (hitLib != null)
        //    {
        //        libId = hitLib.Text("libId");
        //    }
        //    else
        //    {
        //        var result = operation.Insert("WPM_QuestionLibrary", new BsonDocument().Add("name", libararyName).Add("type", type));
        //        if (result.Status == Status.Successful)
        //        {
        //            libId = result.BsonInfo.Text("libId");
        //        }
        //    }
        //    List<StorageData> updateList = new List<StorageData>();
        //    foreach (var qst in QuestionList)
        //    {
        //        var curQst = new BsonDocument();
        //        curQst.Add("group", qst.group.ToString());
        //        curQst.Add("index", qst.index.ToString());
        //        // curQst.Add("instance",qst.instance.ToString());
        //        curQst.Add("question", qst.question);
        //        curQst.Add("answer", qst.answer);
        //        var optionList = new BsonDocument();

        //        foreach (var option in qst.OptionList)
        //        {
        //            //var optionBsonDocument = new BsonDocument();
        //            //optionBsonDocument.Add("content", option.content);
        //            //optionBsonDocument.Add("index", option.index);
        //            optionList.Add(option.index, option.content);
        //        }

        //        curQst.Set("OptionList", optionList);
        //        curQst.Add("libId", libId);
        //        updateList.Add(new StorageData() { Document = curQst, Name = "WPM_Question", Type = StorageType.Insert });
        //    }
        //    if (updateList.Count() > 0)
        //    {
        //        operation.BatchSaveStorageData(updateList);
        //    }
        //}

        //#region 题库基础
        //List<string> groupList = new List<string> { "单选题", "多选题", "是非题", "案例题" };

        ////获取答案分组
        //private QuestionGroup GetQuestionGroup(string txt)
        //{
        //    if (txt.Contains("单选题"))
        //    {
        //        return QuestionGroup.Singel;
        //    }
        //    if (txt.Contains("多选题"))
        //    {
        //        return QuestionGroup.Mutiple;
        //    }
        //    if (txt.Contains("是非题"))
        //    {
        //        return QuestionGroup.YesOrNO;
        //    }
        //    if (txt.Contains("案例题"))
        //    {
        //        return QuestionGroup.Instance;
        //    }
        //    return QuestionGroup.Singel;
        //}
        ////是否问题开始
        //private bool IsQuestionStart(string textInfo)
        //{
        //    if (string.IsNullOrEmpty(textInfo)) return false;
        //    Regex reg = new Regex(@"\d+、");
        //    if (reg.IsMatch(textInfo))
        //    {
        //        return true;
        //    }
        //    return false;
        //}

        ////是否分组开始
        //private bool GroupStart(string textInfo)
        //{
        //    if (string.IsNullOrEmpty(textInfo)) return false;
        //    return groupList.Any(c => textInfo.Contains(c));

        //}


        //// 案例开始
        //private bool InstanceStart(string textInfo)
        //{
        //    if (string.IsNullOrEmpty(textInfo)) return false;
        //    //Regex reg = new Regex(@"（[案例]?\d+）|\([案例]?\d+\)");
        //    Regex reg = new Regex(@"[(（][案例]*\d+[)）]");
        //    if (reg.IsMatch(textInfo))
        //    {
        //        return true;
        //    }
        //    return false;
        //}
        ///// <summary>
        /////是否题目开始
        ///// </summary>
        ///// <param name="textInfo"></param>
        ///// <returns></returns>
        //private bool QuestionStart(string textInfo)
        //{
        //    if (string.IsNullOrEmpty(textInfo)) return false;
        //    Regex reg = new Regex(@"\d+、|（\d+）|\(\d+\)");
        //    if (reg.IsMatch(textInfo))
        //    {
        //        return true;
        //    }
        //    return false;
        //}

        ///// <summary>
        /////是否答案开始
        ///// </summary>
        ///// <param name="textInfo"></param>
        ///// <returns></returns>
        //private bool OptionStart(string textInfo)
        //{
        //    if (string.IsNullOrEmpty(textInfo)) return false;
        //    Regex reg = new Regex(@"(A[\s\S^B]*)(B[\s\S^C]*)(C[\s\S^D]*)?(D[\s\S]*)?");
        //    if (reg.IsMatch(textInfo))
        //    {
        //        return true;
        //    }
        //    return false;
        //}

        //private string YesOrNoFix(string fullQuestionText)
        //{
        //    if (fullQuestionText.Contains("√") || fullQuestionText.Contains("×"))
        //    {
        //        var fixReulst = fullQuestionText.Replace("√", "A").Replace("×", "B");
        //        fixReulst += " |Y|A、对      B、错";
        //        return fixReulst;
        //    }
        //    return fullQuestionText;

        //}


        ////是否题目开始
        //private Question QuestionInit(string fullQuestionText)
        //{
        //    fullQuestionText = YesOrNoFix(fullQuestionText);
        //    var question = new Question();
        //    Regex reg = new Regex(@"[（(][\sABCDEF]+[）)]");
        //    MatchCollection mc = reg.Matches(fullQuestionText);
        //    foreach (Match m in mc)
        //    {
        //        question.answer = m.Value.Replace("（ ", "").Replace("）", "").Replace("(", "").Replace(")", "").Replace("（", "").Trim();

        //    }

        //    //获取序号
        //    var index = -1;
        //    var questionIndex = string.Empty;
        //    Regex regIndex = new Regex(@"\d+、|（\d+）|\(\d+\)");
        //    MatchCollection mcIndex = regIndex.Matches(fullQuestionText);
        //    foreach (Match m in mcIndex)
        //    {
        //        questionIndex = m.Value.Replace("（ ", "").Replace("）", "").Replace("(", "").Replace(")", "").Replace("、", "").Trim();
        //        index = fullQuestionText.IndexOf(questionIndex);
        //        break;
        //    }

        //    var curQuestionIndex = 0;
        //    if (int.TryParse(questionIndex, out curQuestionIndex))
        //    {
        //        question.index = curQuestionIndex;
        //    }
        //    var questionContent = string.Empty;
        //    var contentEndIndex = fullQuestionText.IndexOf("|Y|");
        //    if (contentEndIndex == -1)
        //    {
        //        contentEndIndex = fullQuestionText.IndexOf("A、");
        //        if (contentEndIndex == -1)
        //        {
        //            contentEndIndex = fullQuestionText.IndexOf("A.");
        //        }
        //        if (contentEndIndex == -1)
        //        {
        //            contentEndIndex = fullQuestionText.IndexOf("A");
        //        }

        //        questionContent = fullQuestionText.Substring(index + questionIndex.Length, contentEndIndex - index - questionIndex.Length).Replace("A、", "");
        //    }
        //    else
        //    {
        //        questionContent = fullQuestionText.Substring(index + questionIndex.Length, contentEndIndex - index - questionIndex.Length).Replace("、", "");
        //    }
        //    question.question = questionContent.Replace(question.answer, "");
        //    //获取A B C D 答案选项
        //    var optionStartIndex = contentEndIndex;
        //    if (optionStartIndex == -1 || optionStartIndex > fullQuestionText.Length)
        //    {
        //        optionStartIndex = fullQuestionText.IndexOf(")");
        //        if (optionStartIndex == -1)
        //        {
        //            optionStartIndex = fullQuestionText.IndexOf("）");
        //        }
        //    }
        //    var OptionStr = fullQuestionText.Substring(optionStartIndex, fullQuestionText.Length - optionStartIndex).Replace("|Y|", "");
        //    var OptionList = new List<Option>();
        //    var aIndex = OptionStr.IndexOf("A");
        //    var bIndex = OptionStr.IndexOf("B");
        //    var cIndex = OptionStr.IndexOf("C");
        //    var dIndex = OptionStr.IndexOf("D");
        //    if (aIndex != -1)
        //    {
        //        if (bIndex == -1)
        //        {
        //            bIndex = OptionStr.Length;
        //        }
        //        if (bIndex > aIndex)
        //        {
        //            OptionList.Add(new Option() { index = "A", content = OptionStr.Substring(aIndex, bIndex - aIndex).Replace("A", "").Replace("、", "").Trim() });
        //        }
        //    }
        //    if (bIndex != -1)
        //    {
        //        if (cIndex == -1)
        //        {
        //            cIndex = OptionStr.Length;
        //        }
        //        if (cIndex > bIndex)
        //        {
        //            OptionList.Add(new Option() { index = "B", content = OptionStr.Substring(bIndex, cIndex - bIndex).Replace("B", "").Replace("、", "").Trim() });
        //        }
        //    }
        //    if (cIndex != -1)
        //    {
        //        if (dIndex == -1)
        //        {
        //            dIndex = OptionStr.Length;
        //        }
        //        if (dIndex > cIndex)
        //        {
        //            OptionList.Add(new Option() { index = "C", content = OptionStr.Substring(cIndex, dIndex - cIndex).Replace("C", "").Replace("、", "").Trim() });
        //        }
        //    }
        //    if (dIndex != -1 & OptionStr.Length - 1 > cIndex)
        //    {
        //        OptionList.Add(new Option() { index = "D", content = OptionStr.Substring(dIndex, OptionStr.Length - dIndex).Replace("D", "").Replace("、", "").Trim() });
        //    }
        //    question.OptionList = OptionList;
        //    return question;
        //}
        ///// <summary>
        ///// 字符串替换
        ///// </summary>
        ///// <param name="txt"></param>
        ///// <returns></returns>
        //private string TextFix(string txt)
        //{
        //    var BoldLetters = new Dictionary<string, string> { { "Ａ", "A" }, { "Ｂ", "B" }, { "Ｃ", "C" }, { "Ｄ", "D" } };
        //    if (!string.IsNullOrEmpty(txt))
        //    {
        //        var hitLetter = BoldLetters.Where(c => txt.Contains(c.Key)).ToList();
        //        foreach (var letter in hitLetter)
        //        {
        //            txt = txt.Replace(letter.Key, letter.Value);
        //        }

        //    }

        //    return txt;
        //}
        //#endregion
        #endregion

        #region 企查查app
        string proxyHost = "http://proxy.abuyun.com";
        string proxyPort = "9010";
        // 代理隧道验证信息
        //string proxyUser = "H1880S335RB41F8P";
        //string proxyPass = "ECB2CD5B9D783F4E";
        string proxyUser = "H1538UM3D6R2133P";//"H1880S335RB41F8P";////HVW8J9B1F7K4W83P
        string proxyPass = "511AF06ABED1E7AE";//"ECB2CD5B9D783F4E";////C835A336CD070F9D
        SimpleCrawler.HttpHelper http = new SimpleCrawler.HttpHelper();
        public WebProxy GetWebProxy()
        {
            // 设置代理服务器
            var proxy = new WebProxy();
            proxy.Address = new Uri(string.Format("{0}:{1}", proxyHost, proxyPort));
            proxy.Credentials = new NetworkCredential(proxyUser, proxyPass);
            return proxy;
        }
        public string GetWebProxyString()
        {

            return string.Format("{0}:{1}@{2}:{3}", proxyUser, proxyPass, proxyHost, proxyPort);
        }
        public string GetWebBrowserProxyString()
        {

            return string.Format("{0}:{1}", proxyHost, proxyPort);
        }
        /// <summary>
        /// 获取QCCpost 搜索关键字
        /// </summary>
        /// <param name="keyWord"></param>
        /// <returns></returns>
        public SimpleCrawler.HttpResult TestAPPLogin(string accountName = "")
        {
            var userName = "15189554901";
            if (string.IsNullOrEmpty(accountName))
            {
                accountName = userName;
            }
            //创建Httphelper参数对象
            SimpleCrawler.HttpItem item = new SimpleCrawler.HttpItem()
            {
                URL = "https://app.qichacha.net/app/v1/admin/login",//URL     必需项    

                ContentType = "application/x-www-form-urlencoded",//返回类型    可选项有默认值 

                Timeout = 1500,
                Accept = "*/*",
                // Encoding = null,//编码格式（utf-8,gb2312,gbk）     可选项 默认类会自动识别
                //Encoding = Encoding.Default,
                Method = "post",//URL     可选项 默认为Get
                //Timeout = 100000,//连接超时时间     可选项默认为100000
                //ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000
                //IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写
                //Cookie = "",//字符串Cookie     可选项
                UserAgent = "okhttp/3.2.0",//用户的浏览器类型，版本，操作系统     可选项有默认值
                Referer = "app.qichacha.net",//来源URL     可选项


                Postdata = string.Format("loginType=2&accountType=1&account={3}&password=b412a4532991798fcddf698e31125c03&identifyCode=&key=&token=&timestamp={1}&sign={2}", deviceToken, curDeviceInfo.timestamp, curDeviceInfo.sign, accountName),

                //  Postdata = "loginType=2&accountType=1&account=15916800070&password=b412a4532991798fcddf698e31125c03&identifyCode=&key=&token=timestamp=1473249028088&sign=55cb4ab13f780ec74a23344709e94dc163d8f771"
                //Allowautoredirect = true,
                // Cookie = Settings.SimulateCookies
            };
            //item.PostEncoding = System.Text.Encoding.GetEncoding("utf-8");

            //item.WebProxy = GetWebProxy();

            item.Header.Add("Accept-Encoding", "gzip");
            // item.Header.Add("Host", "app.qichacha.net");
            item.Header.Add("Authorization", curDeviceInfo.accessToken);
            //item.Header.Add("Accept-Language", "zh-CN");
            //item.Header.Add("charset", "UTF-8");

            //item.Header.Add("X-Requested-With", "XMLHttpRequest");
            //请求的返回值对象
            var result = GetPostDataFix(item, curDeviceInfo.accessToken);
            return result;
        }

        public SimpleCrawler.HttpResult RefreshToken()
        {
            if (string.IsNullOrEmpty(curDeviceInfo.accessToken))
            {

                return GetAccessToken();
            }

            var result = GetPostData(new UrlInfo("https://app.qichacha.net/app/v1/admin/refreshToken"), curDeviceInfo.accessToken);
            if (result.Html.Contains("成功"))
            {
                var token = Toolslib.Str.Sub(result.Html, "access_token\":\"", "\"");
                if (!string.IsNullOrEmpty(token))
                {
                    curDeviceInfo.accessToken = string.Format("Bearer {0}", token);
                    SaveDeviceToken();
                }
            }
            else
            {
                result = GetAccessToken();
            }
            this.richTextBox1.Text = string.Format("accessToken:{0},refreshToken:{1}", curDeviceInfo.accessToken, curDeviceInfo.refreshToken);
            return result;
        }
        /// <summary>
        /// 获取QCCpost 搜索关键字
        /// </summary>
        /// <param name="keyWord"></param>
        /// <returns></returns>
        public SimpleCrawler.HttpResult GetPostData(UrlInfo curUrlObj, string accessToken)
        {
            //创建Httphelper参数对象
            SimpleCrawler.HttpItem item = new SimpleCrawler.HttpItem()
            {
                URL = "https://app.qichacha.net/app/v1/admin/refreshToken",//URL     必需项    

                ContentType = "application/x-www-form-urlencoded",//返回类型    可选项有默认值 

                Timeout = 1500,
                Accept = "*/*",
                // Encoding = null,//编码格式（utf-8,gb2312,gbk）     可选项 默认类会自动识别
                //Encoding = Encoding.Default,
                Method = "post",//URL     可选项 默认为Get
                //Timeout = 100000,//连接超时时间     可选项默认为100000
                //ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000
                //IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写
                //Cookie = "",//字符串Cookie     可选项
                UserAgent = "okhttp/3.2.0",//用户的浏览器类型，版本，操作系统     可选项有默认值
                //Referer = "app.qichacha.net",//来源URL     可选项

                Postdata = string.Format("refreshToken={0}&timestamp={1}&appId={2}&sign={3}", curDeviceInfo.refreshToken, curDeviceInfo.timestamp, curDeviceInfo.appId, curDeviceInfo.sign),
                // Allowautoredirect = true,
                // Cookie = Settings.SimulateCookies
            };

            item.Header.Add("Accept-Encoding", "gzip");
            // item.Header.Add("Host", "app.qichacha.net");
            item.Header.Add("Authorization", accessToken);
            //item.Header.Add("Accept-Language", "zh-CN");
            item.Header.Add("charset", "UTF-8");
            //item.Header.Add("X-Requested-With", "XMLHttpRequest");
            //请求的返回值对象
            var result = GetPostDataFix(item, accessToken);
            return result;
        }

        public SimpleCrawler.HttpResult GetAccessToken()
        {
            //创建Httphelper参数对象
            SimpleCrawler.HttpItem item = new SimpleCrawler.HttpItem()
            {
                URL = "https://app.qichacha.net/app/v1/admin/getAccessToken",//URL     必需项    

                ContentType = "application/x-www-form-urlencoded",//返回类型    可选项有默认值 

                Timeout = 1500,
                Accept = "*/*",
                // Encoding = null,//编码格式（utf-8,gb2312,gbk）     可选项 默认类会自动识别
                //Encoding = Encoding.Default,
                Method = "post",//URL     可选项 默认为Get
                //Timeout = 100000,//连接超时时间     可选项默认为100000
                //ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000
                //IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写
                //Cookie = "",//字符串Cookie     可选项
                UserAgent = "okhttp/3.2.0",//用户的浏览器类型，版本，操作系统     可选项有默认值
                //Referer = "app.qichacha.net",//来源URL     可选项

                //Postdata = "appId=80c9ef0fb86369cd25f90af27ef53a9e&deviceId=Vz0qxB%2F7F6gDACjW7yx%2Bq%2FWl&version=9.2.0&deviceType=android&os=&timestamp=1473410989361&sign=900d94ca206f4bca0019bc1c7a07ceac2e171b60",
                Postdata = string.Format("appId={0}&deviceId={1}&version=9.2.0&deviceType=android&os=&timestamp={2}&sign={3}", curDeviceInfo.appId, curDeviceInfo.deviceId, curDeviceInfo.timestamp, curDeviceInfo.sign),


                //Allowautoredirect = true,
                // Cookie = Settings.SimulateCookies
            };

            item.Header.Add("Accept-Encoding", "gzip");
            // item.Header.Add("Host", "app.qichacha.net");
            //item.Header.Add("Authorization", accessToken);
            //item.Header.Add("Accept-Language", "zh-CN");
            item.Header.Add("charset", "UTF-8");
            //item.Header.Add("X-Requested-With", "XMLHttpRequest");
            //请求的返回值对象
            var result = GetPostDataFix(item, curDeviceInfo.accessToken);
            var token = Toolslib.Str.Sub(result.Html, "access_token\":\"", "\"");
            if (!string.IsNullOrEmpty(token))
                curDeviceInfo.accessToken = string.Format("Bearer {0}", token);
            var _refleshtoken = Toolslib.Str.Sub(result.Html, "refresh_token\":\"", "\"");
            if (!string.IsNullOrEmpty(_refleshtoken))
                curDeviceInfo.refreshToken = _refleshtoken;
            SaveDeviceToken();
            return result;
        }

        /// <summary>
        /// 返回请求数据
        /// </summary>
        /// <param name="curUrlObj"></param>
        /// <returns></returns>
        public SimpleCrawler.HttpResult GetHttpHtml(UrlInfo curUrlObj)
        {
            var url = FixUrlSignStr(curUrlObj);
            // return GetPostDataFix(curUrlObj, accessToken);
            SimpleCrawler.HttpResult result = new SimpleCrawler.HttpResult();
            try
            {
                var item = new SimpleCrawler.HttpItem()
                {
                    URL = url,
                    Method = "get",//URL     可选项 默认为Get   
                    // ContentType = "text/html",//返回类型    可选项有默认值 
                    UserAgent = "okhttp/3.2.0",
                    ContentType = "application/x-www-form-urlencoded",
                };

                // item.Header.Add("Content-Type", "application/x-www-form-urlencoded");
                // hi.HeaderSet("Content-Length","154");
                // hi.HeaderSet("Connection","Keep-Alive");

                item.Header.Add("Accept-Encoding", "gzip");
                item.Header.Add("Authorization", curDeviceInfo.accessToken);
                item.WebProxy = GetWebProxy();
                result = http.GetHtml(item);

            }
            catch (WebException ex)
            {

            }
            catch (TimeoutException ex)
            {

            }
            catch (Exception ex)
            {

            }
            return result;
        }
        #endregion
        public string FixUrlSignStr(UrlInfo curUrlObj)
        {
            var url = curUrlObj.UrlString;
            var _timestamp = GetUrlParam(curUrlObj.UrlString, "timestamp");
            var _sign = GetUrlParam(curUrlObj.UrlString, "sign");
            var _token = GetUrlParam(curUrlObj.UrlString, "token");
            if (!string.IsNullOrEmpty(_timestamp) && _timestamp != curDeviceInfo.timestamp)
            {
                url = url.Replace(_timestamp, curDeviceInfo.timestamp);
            }
            if (!string.IsNullOrEmpty(_sign) && _sign != curDeviceInfo.sign)
            {
                url = url.Replace(_sign, curDeviceInfo.sign);
            }
            if (!string.IsNullOrEmpty(_token) && _sign != curDeviceInfo.accessToken)
            {
                url = url.Replace(_token, curDeviceInfo.accessToken.Replace("Bearer", "").Trim());
            }
            return url;
        }

        public SimpleCrawler.HttpResult GetPostDataFix(SimpleCrawler.HttpItem item, string accessToken)
        {
            //string.Format("refreshToken=f128619e442a6efe44c1544b4c926824&timestamp=1473757386869&appId=80c9ef0fb86369cd25f90af27ef53a9e&sign=a5ae576bcddcba5df634f041995e45cd54b255e6");
            hi.Url = item.URL;
            //hi.Refer = "http://app.qichacha.net";
            if (!string.IsNullOrEmpty(item.Postdata))
            {
                hi.PostData = item.Postdata;
            }
            hi.UserAgent = "okhttp/3.2.0";
            hi.HeaderSet("Content-Type", "application/x-www-form-urlencoded");
            // hi.HeaderSet("Content-Length","154");
            // hi.HeaderSet("Connection","Keep-Alive");
            hi.HeaderSet("Accept-Encoding", "gzip");
            hi.HeaderSet("Authorization", accessToken);
            var ho = LibCurlNet.HttpManager.Instance.ProcessRequest(hi);
            if (ho.IsOK)
            {
                return new SimpleCrawler.HttpResult() { StatusCode = HttpStatusCode.OK, Html = ho.TxtData };
            }
            else
            {
                return new SimpleCrawler.HttpResult() { StatusCode = HttpStatusCode.Forbidden };
            }

        }

        public SimpleCrawler.HttpResult GetPostDataFix(UrlInfo curUrlObj, string accessToken)
        {
            //string.Format("refreshToken=f128619e442a6efe44c1544b4c926824&timestamp=1473757386869&appId=80c9ef0fb86369cd25f90af27ef53a9e&sign=a5ae576bcddcba5df634f041995e45cd54b255e6");
            hi.Url = curUrlObj.UrlString.ToString();
            //hi.Refer = "http://app.qichacha.net";
            if (!string.IsNullOrEmpty(curUrlObj.PostData))
                hi.PostData = curUrlObj.PostData;
            hi.UserAgent = "okhttp/3.2.0";
            hi.HeaderSet("Content-Type", "application/x-www-form-urlencoded");
            // hi.HeaderSet("Content-Length","154");
            // hi.HeaderSet("Connection","Keep-Alive");
            hi.HeaderSet("Accept-Encoding", "gzip");
            hi.HeaderSet("Authorization", accessToken);

            var ho = LibCurlNet.HttpManager.Instance.ProcessRequest(hi);
            if (ho.IsOK)
            {
                return new SimpleCrawler.HttpResult() { StatusCode = HttpStatusCode.OK, Html = ho.TxtData };
            }
            else
            {
                return new SimpleCrawler.HttpResult() { StatusCode = HttpStatusCode.Forbidden };
            }

        }

        private void RefreshToken1()
        {

            hi.Url = "https://app.qichacha.net/app/v1/admin/refreshToken";
            //hi.Refer = "http://app.qichacha.net";
            hi.PostData = "refreshToken=16748db924743a04e523ad4988119d21&timestamp=1474264731774&appId=80c9ef0fb86369cd25f90af27ef53a9e&sign=68d5b2bb3352a215cb6eff8aaf2a3acc0ac95225";
            hi.UserAgent = "okhttp/3.2.0";
            hi.HeaderSet("Content-Type", "application/x-www-form-urlencoded");
            // hi.HeaderSet("Content-Length","154");
            // hi.HeaderSet("Connection","Keep-Alive");
            hi.HeaderSet("Accept-Encoding", "gzip");
            hi.HeaderSet("Authorization", curDeviceInfo.accessToken);
            var ho = LibCurlNet.HttpManager.Instance.ProcessRequest(hi);
            if (ho.IsOK)
            {
                this.richTextBox2.Text = ho.TxtData;
            }
        }
        private void SendValidateToken()
        {

            hi.Url = "https://app.qichacha.net/app/v1/admin/sendValidateToken";
            //hi.Refer = "http://app.qichacha.net";
            hi.PostData = "account=15959266823&timestamp=1474264991264&sign=3ef18e8fbe43caa476386c27cee33042921b6341";
            hi.UserAgent = "okhttp/3.2.0";
            hi.HeaderSet("Content-Type", "application/x-www-form-urlencoded");
            // hi.HeaderSet("Content-Length","154");
            // hi.HeaderSet("Connection","Keep-Alive");
            hi.HeaderSet("Accept-Encoding", "gzip");
            hi.HeaderSet("Authorization", "Bearer MmQ1MWNjNDctMjQwZC00OGE3LWE5NzUtYzRhN2JhZDk1YTBk");
            var ho = LibCurlNet.HttpManager.Instance.ProcessRequest(hi);
            if (ho.IsOK)
            {
                this.richTextBox2.Text = ho.TxtData;
            }
        }
        private List<BsonDocument> allAccountList = new List<BsonDocument>();
        private void button32_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            //GetAccessToken();
            var connStr = "mongodb://sa:dba@59.61.72.34/SimpleCrawler";
            DataOperation dataOp = new DataOperation(new MongoOperation(connStr));
            MongoOperation _mongoDBOp = new MongoOperation(connStr);
            var tableName = "QCCDeviceAccount";//设备账号注册
            //appId=80c9ef0fb86369cd25f90af27ef53a9e&deviceId=UygxNMGUAhsBAFbml5Qf92Ie&version=9.2.0&deviceType=android&os=&timestamp=1474629307274&sign=257c174551b3bf1660daa3797f71ce3014bf0827
            allAccountList = dataOp.FindAll(tableName).ToList();
            foreach (var account in allAccountList.Where(c => c.Int("isInvalid") == 0 && c.Int("status") == 0))
            {
                this.comboBox1.Items.Add(account.Text("deviceId"));
                //  DBChangeQueue.Instance.EnQueue(new StorageData() { Name = tableName, Document = new BsonDocument().Add("isInvalid", "1"), Query = Query.EQ("deviceId", account.Text("deviceId")), Type = StorageType.Update });
            }
            //   StartDBChangeProcessQuick(_mongoDBOp);
        }
        private static string GetUrlParam(string queryStr, string name)
        {

            var dic = HttpUtility.ParseQueryString(queryStr);
            var industryCode = dic[name] != null ? dic[name].ToString() : string.Empty;//行业代码
            return industryCode;
        }
        private void button33_Click(object sender, EventArgs e)
        {
            var connStr = "mongodb://sa:dba@59.61.72.34/SimpleCrawler";
            DataOperation dataOp = new DataOperation(new MongoOperation(connStr));
            MongoOperation _mongoDBOp = new MongoOperation(connStr);
            var tableName = "QCCDeviceAccount";//设备账号注册
            //appId=80c9ef0fb86369cd25f90af27ef53a9e&deviceId=UygxNMGUAhsBAFbml5Qf92Ie&version=9.2.0&deviceType=android&os=&timestamp=1474629307274&sign=257c174551b3bf1660daa3797f71ce3014bf0827
            var allAccountNameList = dataOp.FindAll(tableName).Select(c => c.Text("deviceId")).ToList();


            var textStr = this.richTextBox1.Text.Replace("sig\nn", "sign").Replace("\t", " ").Split(new string[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();
            var insertCount = 0; var updateCount = 0;
            foreach (var textStrArr in textStr)
            {

                if (string.IsNullOrEmpty(textStrArr)) continue;
                var updateBson = new BsonDocument();
                var appId = GetUrlParam(textStrArr, "appId");
                var deviceId = GetUrlParam(textStrArr, "deviceId");
                if (string.IsNullOrEmpty(deviceId)) continue;
                var timestamp = GetUrlParam(textStrArr, "timestamp");
                var sign = GetUrlParam(textStrArr, "sign");
                var AccessToken = GetUrlParam(textStrArr, "access_token");
                var RefleshToken = GetUrlParam(textStrArr, "refresh_token");
                if (string.IsNullOrEmpty(deviceId) || string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(sign)) continue;
                updateBson.Add("appId", appId.Trim());
                updateBson.Add("deviceId", deviceId.Trim());
                updateBson.Add("name", deviceId.Trim());
                updateBson.Add("timestamp", timestamp.Trim());
                updateBson.Add("sign", sign.Trim());
                updateBson.Add("AccessToken", AccessToken.Trim());
                updateBson.Add("RefleshToken", RefleshToken.Trim());
                if (!allAccountNameList.Contains(deviceId))//已存在
                {

                    DBChangeQueue.Instance.EnQueue(new StorageData() { Name = tableName, Document = updateBson, Type = StorageType.Insert });
                    allAccountNameList.Add(deviceId);
                    insertCount++;
                }
                else
                {
                    updateCount++;
                    DBChangeQueue.Instance.EnQueue(new StorageData() { Name = tableName, Document = updateBson, Query = Query.EQ("deviceId", deviceId), Type = StorageType.Update });
                }

                sb.AppendFormat("appId={0}\n", appId.Trim());
                sb.AppendFormat("deviceId={0}\n", deviceId.Trim());
                sb.AppendFormat("timestamp={0}\n", timestamp.Trim());
                sb.AppendFormat("sign={0}\n", sign.Trim());
                sb.AppendFormat("RefleshToken={0}\n", RefleshToken.Trim());
                sb.AppendFormat("AccessToken={0}\n", AccessToken.Trim());
            }
            this.richTextBox2.Text = sb.ToString();
            StartDBChangeProcessQuick(_mongoDBOp);
            MessageBox.Show("succeed" + insertCount.ToString() + "|" + updateCount.ToString());
        }

        private void button34_Click(object sender, EventArgs e)
        {
            //var htmlDoc = new JToken(){};
            //htmlDoc
            MessageBox.Show( ConvertDecimalNCRToString("&#27468;&#33673;&#23045;"));
            this.richTextBox1.Text+=HttpUtility.HtmlDecode("sds&#27468;&#33673;&#23045;");
        }

        public string ConvertDecimalNCRToString(string hex)
        {
            string myString = hex.Replace("&#", "");
            String[] split = myString.Split(new string[]{";"},StringSplitOptions.RemoveEmptyEntries);
            StringBuilder strResult = new StringBuilder();
            for (int i = 0; i <= split.Length - 1; i++)
            {
                int charCode = Convert.ToInt32(split[i]);
                strResult.Append((char)charCode);
            }
            return strResult.ToString();

        }
        private void button35_Click(object sender, EventArgs e)
        {
            var str = "20151001";
            var dateTime = DateTime.ParseExact(str, "yyyyMMdd", null);

            if (allAccountList != null && allAccountList.Count() > 0)
            {
                this.richTextBox1.Text += GetRandString(24, 100) + "\n\r";
            }
            else
            {
                MessageBox.Show("请先加载设备");
            }
        }
        private const string CHAR = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private string GetRandString(int len, int count)
        {
            var deviceIds = allAccountList.Select(c => c.Text("deviceId")).ToList();
            StringBuilder str = new StringBuilder();
            Random rand = new Random();
            for (var index = 1; index <= count; index++)
            {
                List<string> list = new List<string>();

                for (var begin = 0; begin <= len - 1; begin++)
                {
                    var value = rand.Next(0, 59);
                    list.Add(GetChart(value));

                }
                list = SortByRandom(list);
                var deviceAccount = string.Join("", list.ToArray());
                if (!deviceIds.Contains(deviceAccount))
                    str.AppendFormat("update system set value='{0}'  where name='mqBRboGZkQPcAkyk';\r", deviceAccount);
            }
            return str.ToString();
        }

        private string GetChart(int index)
        {
            StringBuilder str = new StringBuilder();
            str.Append(CHAR[index]);
            return str.ToString();
        }
        private List<string> SortByRandom(List<string> charList)
        {
            Random rand = new Random();
            for (int i = 0; i < charList.Count; i++)
            {
                int index = rand.Next(0, charList.Count);
                string temp = charList[i];
                charList[i] = charList[index];
                charList[index] = temp;
            }

            return charList;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var comboBox1 = sender as ComboBox;
            if (comboBox1 != null)
            {
                var hitDevice = allAccountList.Where(c => c.Text("deviceId") == comboBox1.Text).FirstOrDefault();
                if (hitDevice != null)
                {
                    curDeviceInfo.deviceId = hitDevice.Text("deviceId");
                    curDeviceInfo.accessToken = hitDevice.Text("accessToken");
                    curDeviceInfo.refreshToken = hitDevice.Text("refreshToken");
                    curDeviceInfo.timestamp = hitDevice.Text("timestamp");
                    curDeviceInfo.sign = hitDevice.Text("sign");
                    curDeviceInfo.isBusy = hitDevice.Text("isBusy");
                    this.richTextBox2.Text = curDeviceInfo.ToString();
                }
            }
        }

        private void button36_Click(object sender, EventArgs e)
        {

            var connStr = "mongodb://sa:dba@59.61.72.34/SimpleCrawler";
            DataOperation dataOp = new DataOperation(new MongoOperation(connStr));
            MongoOperation _mongoDBOp = new MongoOperation(connStr);
            var tableName = "QCCEnterpriseKey";
            var tableNameCity = tableName + "City";
            //"name", "address", 北京 2016-09-29-16 00
            var column = "address";
            var allAccountNameList = dataOp.FindAllByQuery(tableName, Query.And(Query.EQ("cityName", "南通"), Query.GT("updateDate", "2016-09-29"), Query.Exists(column, true))).SetFields("guid", column).ToList();
            var cityList = dataOp.FindAllByQuery(tableNameCity, Query.EQ("type", "1")).ToList();
            var cityUrlList = dataOp.FindAll("LandFangCityEXURL").ToList();//城市url
            var otherCityList = cityUrlList.Where(c => c.Text("type") != "2").ToList();
            var sb = new StringBuilder();
            foreach (var enterprise in allAccountNameList)
            {

                var guid = enterprise.Text("guid");
                var registrar = enterprise.Text(column);
                sb.AppendFormat("{0}\r", registrar);
                if (registrar.Contains("南通") || registrar.Contains("如皋"))
                {
                    var addBson = new BsonDocument();
                    addBson.Add("cityName", "南通").Add("updateDate", "2016-09-25 18:26:29");
                    DBChangeQueue.Instance.EnQueue(new StorageData() { Name = tableName, Document = addBson, Query = Query.EQ("guid", guid), Type = StorageType.Update });
                    continue;
                }

                if (registrar.Contains("溧阳"))
                {
                    var addBson = new BsonDocument();
                    addBson.Add("cityName", "常州");
                    DBChangeQueue.Instance.EnQueue(new StorageData() { Name = tableName, Document = addBson, Query = Query.EQ("guid", guid), Type = StorageType.Update });
                    continue;
                }

                if (registrar.Contains("昆明"))
                {
                    var addBson = new BsonDocument();
                    addBson.Add("cityName", "郑州");
                    DBChangeQueue.Instance.EnQueue(new StorageData() { Name = tableName, Document = addBson, Query = Query.EQ("guid", guid), Type = StorageType.Update });
                    continue;
                }
                if (registrar.Contains("郑州"))
                {
                    var addBson = new BsonDocument();
                    addBson.Add("cityName", "郑州");
                    DBChangeQueue.Instance.EnQueue(new StorageData() { Name = tableName, Document = addBson, Query = Query.EQ("guid", guid), Type = StorageType.Update });
                    continue;
                }
                if (registrar.Contains("南昌"))
                {
                    var addBson = new BsonDocument();
                    addBson.Add("cityName", "南昌");
                    DBChangeQueue.Instance.EnQueue(new StorageData() { Name = tableName, Document = addBson, Query = Query.EQ("guid", guid), Type = StorageType.Update });
                    continue;
                }
                if (registrar.Contains("张家港") || registrar.Contains("太仓"))
                {
                    var addBson = new BsonDocument();
                    addBson.Add("cityName", "苏州");
                    DBChangeQueue.Instance.EnQueue(new StorageData() { Name = tableName, Document = addBson, Query = Query.EQ("guid", guid), Type = StorageType.Update });
                    continue;
                }
                if (registrar.Contains("重庆"))
                {
                    var addBson = new BsonDocument();
                    addBson.Add("cityName", "重庆");
                    DBChangeQueue.Instance.EnQueue(new StorageData() { Name = tableName, Document = addBson, Query = Query.EQ("guid", guid), Type = StorageType.Update });
                    continue;
                }
                if (registrar.Contains("苏州"))
                {
                    var addBson = new BsonDocument();
                    addBson.Add("cityName", "苏州");
                    DBChangeQueue.Instance.EnQueue(new StorageData() { Name = tableName, Document = addBson, Query = Query.EQ("guid", guid), Type = StorageType.Update });
                    continue;
                }
                if (registrar.Contains("昆山"))
                {
                    var addBson = new BsonDocument();
                    addBson.Add("cityName", "昆山");
                    DBChangeQueue.Instance.EnQueue(new StorageData() { Name = tableName, Document = addBson, Query = Query.EQ("guid", guid), Type = StorageType.Update });
                    continue;
                }
                if (registrar.Contains("天津"))
                {
                    var addBson = new BsonDocument();
                    addBson.Add("cityName", "天津");
                    DBChangeQueue.Instance.EnQueue(new StorageData() { Name = tableName, Document = addBson, Query = Query.EQ("guid", guid), Type = StorageType.Update });
                    continue;
                }
                if (registrar.Contains("北京"))
                {
                    var addBson = new BsonDocument();
                    addBson.Add("cityName", "北京");
                    DBChangeQueue.Instance.EnQueue(new StorageData() { Name = tableName, Document = addBson, Query = Query.EQ("guid", guid), Type = StorageType.Update });
                    continue;
                }
                if (registrar.Contains("上海"))
                {
                    var addBson = new BsonDocument();
                    addBson.Add("cityName", "上海");
                    DBChangeQueue.Instance.EnQueue(new StorageData() { Name = tableName, Document = addBson, Query = Query.EQ("guid", guid), Type = StorageType.Update });
                    continue;
                }
                if (registrar.Contains("重庆"))
                {
                    var addBson = new BsonDocument();
                    addBson.Add("cityName", "重庆");
                    DBChangeQueue.Instance.EnQueue(new StorageData() { Name = tableName, Document = addBson, Query = Query.EQ("guid", guid), Type = StorageType.Update });
                    continue;
                }



                //var hitCityList = cityList.Where(c => registrar.Contains(c.Text("name").Replace("市", "").Replace("区", ""))).ToList();
                //if (hitCityList.Count() > 0)
                //{
                //    if (hitCityList.Count == 1 && !string.IsNullOrEmpty("guid"))
                //    {
                //        var cityNameBson = hitCityList.FirstOrDefault();
                //        var cityName = cityNameBson.Text("name").Replace("市", "");
                //        var addBson = new BsonDocument();
                //        addBson.Add("cityName", cityName);
                //        DBChangeQueue.Instance.EnQueue(new StorageData() { Name = tableName, Document = addBson, Query = Query.EQ("guid", guid), Type = StorageType.Update });
                //    }
                //    else
                //    {
                //        var cityNameBson = hitCityList.OrderByDescending(c => c.Text("name").Length).FirstOrDefault();
                //        var cityName = cityNameBson.Text("name").Replace("市", "");
                //        var addBson = new BsonDocument();
                //        addBson.Add("cityName", cityName);
                //        DBChangeQueue.Instance.EnQueue(new StorageData() { Name = tableName, Document = addBson, Query = Query.EQ("guid", guid), Type = StorageType.Update });
                //    }
                //}

                //var hitCityList = otherCityList.Where(c => c.Text("cityCode") != "" && c.Text("type") != "2").Where(c => registrar.Contains(c.Text("name").Replace("市", "").Replace("区", ""))).ToList();
                //if (hitCityList.Count() > 0)
                //{
                //    if (hitCityList.Count == 1 && !string.IsNullOrEmpty("guid"))
                //    {
                //        var cityNameBson = hitCityList.FirstOrDefault();
                //        var cityName = cityNameBson.Text("name").Replace("市", "");
                //        var addBson = new BsonDocument();
                //        addBson.Add("cityName", cityName);
                //        DBChangeQueue.Instance.EnQueue(new StorageData() { Name = tableName, Document = addBson, Query = Query.EQ("guid", guid), Type = StorageType.Update });
                //    }
                //    else
                //    {
                //        var cityNameBson = hitCityList.OrderByDescending(c => c.Text("name").Length).FirstOrDefault();
                //        var cityName = cityNameBson.Text("name").Replace("市", "");
                //        var addBson = new BsonDocument();
                //        addBson.Add("cityName", cityName);
                //        DBChangeQueue.Instance.EnQueue(new StorageData() { Name = tableName, Document = addBson, Query = Query.EQ("guid", guid), Type = StorageType.Update });
                //    }
                //    continue;
                //}

                var hitRegionList = cityUrlList.Where(c => c.Text("type") == "2" && registrar.Contains(c.Text("name"))).ToList();
                if (hitRegionList.Count() > 0)
                {
                    if (hitRegionList.Count == 1 && !string.IsNullOrEmpty("guid"))
                    {
                        var regionObj = hitRegionList.FirstOrDefault();
                        var hitCity = cityUrlList.Where(c => c.Text("cityCode") == regionObj.Text("cityCode")).FirstOrDefault();
                        var cityName = hitCity.Text("name").Replace("市", "");
                        var addBson = new BsonDocument();
                        addBson.Add("cityName", cityName);
                        if (cityName == "南通")
                        {
                            addBson.Add("updateDate", "2016-09-25 18:26:29");
                        }
                        DBChangeQueue.Instance.EnQueue(new StorageData() { Name = tableName, Document = addBson, Query = Query.EQ("guid", guid), Type = StorageType.Update });
                    }
                    else
                    {
                        continue;
                        var regionObj = hitRegionList.OrderByDescending(c => c.Text("name").Length).FirstOrDefault();
                        var hitCity = cityUrlList.Where(c => c.Text("cityCode") == regionObj.Text("cityCode")).FirstOrDefault();
                        var cityName = hitCity.Text("name").Replace("市", "");
                        var addBson = new BsonDocument();
                        addBson.Add("cityName", cityName);
                        if (cityName == "南通")
                        {
                            addBson.Add("updateDate", "2016-09-25 18:26:29");
                        }
                        DBChangeQueue.Instance.EnQueue(new StorageData() { Name = tableName, Document = addBson, Query = Query.EQ("guid", guid), Type = StorageType.Update });
                    }
                }


            }
            this.richTextBox1.Text = sb.ToString();
            StartDBChangeProcessQuick(_mongoDBOp);
        }
        string filterColumn = "telephone";
        private void button37_Click(object sender, EventArgs e)
        {
            MongoConnectionStringBuilder builder = new MongoConnectionStringBuilder();
            builder.Server = new MongoServerAddress("59.61.72.34", 27017);
            builder.DatabaseName = "SimpleCrawler";
            builder.Username = "sa";
            builder.Password = "dba";
            builder.SocketTimeout = new TimeSpan(00, 03, 59);
            var _mongoDBOp = new MongoOperation(builder);
            var dataOp = new DataOperation(new MongoOperation(builder));
            var cityName = this.richTextBox1.Text.Trim();
            var tableName = "QCCEnterpriseKey";//设备账号注册
            //appId=80c9ef0fb86369cd25f90af27ef53a9e&deviceId=UygxNMGUAhsBAFbml5Qf92Ie&version=9.2.0&deviceType=android&os=&timestamp=1474629307274&sign=257c174551b3bf1660daa3797f71ce3014bf0827
            //var enterpriseList = dataOp.FindAllByQuery(tableName, Query.And(Query.EQ("cityName", "厦门"))).OrderBy(c => c.Text("cityName")).ThenBy(c => c.Text("domain")).ThenBy(c => c.Text("reg_capi_desc")).ToList();
            var query = Query.And(Query.EQ("cityName", cityName), Query.Matches("type", "个"), Query.And(Query.NE("status", "吊销"), Query.NE("status", "注销")));
           // var query = Query.And(Query.EQ("cityName", cityName), Query.Matches("date", "2016"));
           // var query = Query.And(Query.EQ("cityName", cityName),Query.Matches("date", "2016"), Query.Exists("telephone", true), Query.NE("telephone", ""), Query.And(Query.NE("status", "吊销"), Query.NE("status", "注销")));
            //var query = Query.And(Query.EQ("cityName", "嘉兴"), Query.EQ("isUserUpdate", "1"), Query.Exists("webSite", true), Query.NE("webSite", ""), Query.Or(Query.EQ("shortStatus", "存续"), Query.EQ("shortStatus", "在业")));
            //var query = Query.And(Query.EQ("cityName", "深圳"), Query.EQ("isUserUpdate", "1"), Query.And(Query.NE("status", "吊销"), Query.NE("status", "注销")));
            var allCount = dataOp.FindCount(tableName, query);
            //var enterpriseList = dataOp.FindAllByQuery(tableName, query);//oper_name
            var elementList = new List<string>() { "cityName", "name", "telephone", "email", "webSite", "oper_name", "domain", "operationDomain", "shortStatus", "type", "reg_capi_desc", "address", "registrar", "date", "limitDate" };

            //var sb=new StringBuilder();

            //foreach (var enterprise in enterpriseList )
            //{

            //    foreach (var elem in elementList)
            //    {
            //        sb.AppendFormat("{0}\t", enterprise.Text(elem));
            //    }
            //     sb.AppendFormat("\n");
            //}


            long pageCount;
            var pageSize = 50000;
            if (allCount % pageSize == 0)
                pageCount = allCount / pageSize;
            else
                pageCount = allCount / pageSize + 1;
            for (var index = 1; index <= pageCount; index++)
            {
                var xlsDoc = new XlsDocument();

                var skipCount = (index - 1) * pageSize;

                //var enterpriseList = dataOp.FindLimitByQuery(tableName, query, new SortByDocument(), skipCount, pageSize).SetFields(elementList.ToArray()).ToList();
                var enterpriseList = dataOp.FindLimitFieldsByQuery(tableName, query, new SortByDocument(), skipCount, pageSize, elementList.ToArray()).ToList();
                BsonListToExcel(xlsDoc, enterpriseList, elementList, index);
                using (MemoryStream ms = new MemoryStream())
                {
                    xlsDoc.FileName = index.ToString() + ".xls";
                    xlsDoc.Save(true);

                }
            }

            this.richTextBox1.Text = "succeed";
        }
        private BloomFilter<string> enterpriseNameList = new BloomFilter<string>(2000000);
        private void BsonListToExcel(XlsDocument xlsDoc, List<BsonDocument> docList, List<string> elementList, int page)
        {


            var sheet = xlsDoc.Workbook.Worksheets.Add("企业数据" + page.ToString());

            var dataXF = xlsDoc.NewXF();
            dataXF.VerticalAlignment = VerticalAlignments.Centered;

            var titleXF = xlsDoc.NewXF();
            titleXF.HorizontalAlignment = HorizontalAlignments.Centered;
            titleXF.VerticalAlignment = VerticalAlignments.Centered;
            titleXF.PatternBackgroundColor = Colors.Grey;
            titleXF.Font.Bold = true;

            var cells = sheet.Cells;
            #region 宽度调整
            ushort index = 0;
            var rowIndex = 1;
            foreach (var elem in elementList)
            {
                ColumnInfo firstCol = new ColumnInfo(xlsDoc, sheet);
                firstCol.Width = 40 * 64;
                firstCol.ColumnIndexStart = index;
                firstCol.ColumnIndexEnd = index;

                sheet.AddColumnInfo(firstCol);
                index++;
                cells.Add(rowIndex, index, elem, titleXF);

            }

            rowIndex++;
            foreach (var doc in docList)
            {
                var colIndex = 1;
                foreach (var elem in elementList)
                {
                    var value = doc.Text(elem);
                    if (elem == "oper_name" && value.Length >= 2)
                    {
                        value = value.Substring(0, 1) + "姓";
                    }
                    if (elem == filterColumn)
                    {
                        if (elem == "name" && enterpriseNameList.Contains(value))
                        {
                           //已存在重复的字段
                            break;
                        }
                        else
                        {
                            enterpriseNameList.Add(value);
                        }
                    }
                    cells.Add(rowIndex, colIndex++, value, dataXF);
                }

                rowIndex++;
            }
            #endregion



        }
        private void button38_Click(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(StartDataDeal);

        }
        private void StartDataDeal(object i)
        {
            MongoConnectionStringBuilder builder = new MongoConnectionStringBuilder();
            builder.Server = new MongoServerAddress("192.168.1.134", 27017);
            builder.DatabaseName = "SimpleCrawler";
            builder.Username = "sa";
            builder.Password = "dba";
            builder.SocketTimeout = new TimeSpan(00, 01, 59);
            var _mongo = new MongoOperation(builder);
            var _dataop = new DataOperation(new MongoOperation(builder));
            var hitUserEnterpriseKeyTemp = _dataop.FindAllByQuery("QCCEnterpriseKeyTemp", Query.EQ("category", "2")).Select(c => c.Text("keyno"));
            var index = 1;


            foreach (var guid in hitUserEnterpriseKeyTemp)
            {

                DBChangeQueue.Instance.EnQueue(new StorageData()
                {
                    Name = "QiXinEnterpriseDetailInfo",
                    Document = new BsonDocument().Add("category", "2"),
                    Query = Query.EQ("eGuid", guid),
                    Type = StorageType.Update
                });
                StartDBChangeProcessQuick(_mongo);
                index++;

                if (index % 100 == 0)
                {

                    ShowMessageInfo(index.ToString());
                }

            }
            ShowMessageInfo("完成", true);

        }
        public void ShowMessageInfo(string str, bool isAppend = false)
        {
            richTextBox1.BeginInvoke(new Action(() =>
            {
                if (isAppend == false)
                {
                    this.richTextBox1.Clear();
                }


                this.richTextBox1.AppendText(str);


            })
           );
        }

        private void button39_Click(object sender, EventArgs e)
        {
            MongoConnectionStringBuilder builder = new MongoConnectionStringBuilder();
            builder.Server = new MongoServerAddress("59.61.72.34", 27017);
            builder.DatabaseName = "SimpleCrawler";
            builder.Username = "sa";
            builder.Password = "dba";
            builder.SocketTimeout = new TimeSpan(00, 01, 59);
            var _mongo = new MongoOperation(builder);
            var _dataop = new DataOperation(new MongoOperation(builder));
            var allCityUrlList = _dataop.FindAll("LandFangCityEXURL").ToList();//城市url
            var cityNameList = allCityUrlList.Where(c => c.Int("type") == 1).Select(c => c.Text("name")).ToList();//城市
            var cityList = allCityUrlList.Where(c => c.Int("type") == 1).ToList();//城市
            var provinceList = allCityUrlList.Where(c => c.Int("type") == 0).ToList();//省份
            var regionList = allCityUrlList.Where(c => c.Int("type") == 2).ToList();//区域]
            var filterCity = new string[] { "北京", "上海", "重庆", "天津" };
            var hitLandFangList = _dataop.FindAllByQuery("LandFang", Query.And(Query.NotIn("地区", filterCity.Select(c => (BsonValue)c)))).SetFields("_id", "所在地", "地区", "url", "县市");
            var index = 1;


            foreach (var landFang in hitLandFangList)
            {
                if (!cityNameList.Contains(landFang.Text("所在地")))
                {
                    index++;
                    var updateBson = new BsonDocument();
                    if (index % 100 == 0)
                    {

                        ShowMessageInfo(index.ToString(), true);
                    }

                    if (landFang.Text("地区") == "重庆" && string.IsNullOrEmpty(landFang.Text("所在地")))
                    {
                        updateBson.Set("所在地", "重庆");
                    }

                    var hitProvince = provinceList.Where(c => c.Text("name") == landFang.Text("地区")).FirstOrDefault();
                    if (hitProvince != null)
                    {
                        var hitRegionList = regionList.Where(c => c.Text("provinceCode") == hitProvince.Text("provinceCode")).ToList();
                        if (string.IsNullOrEmpty(landFang.Text("所在地").Trim())) { continue; }
                        var hitRegionObj = hitRegionList.Where(c => c.Text("name").Contains(landFang.Text("所在地").Trim())).FirstOrDefault();
                        if (hitRegionObj != null)
                        {
                            var hitCity = cityList.Where(c => c.Text("cityCode") == hitRegionObj.Text("cityCode")).FirstOrDefault();
                            if (hitCity != null)
                            {
                                updateBson.Set("地区", landFang.Text("地区"));
                                updateBson.Set("所在地", hitCity.Text("name"));
                                if (string.IsNullOrEmpty(landFang.Text("县市")))
                                {
                                    updateBson.Set("县市", hitRegionObj.Text("name"));
                                }
                                DBChangeQueue.Instance.EnQueue(new StorageData()
                                {
                                    Name = "LandFang",
                                    Document = updateBson,
                                    Query = Query.EQ("url", landFang.Text("url")),
                                    Type = StorageType.Update
                                });

                            }
                        }
                    }
                    else  ///连省都没有
                    {
                        // if (landFang.Text("地区") == "新疆维吾尔自治区" || landFang.Text("地区") == "")
                        // {
                        //     updateBson.Set("地区", "新疆");
                        // }
                        updateBson.Set("isNull", "1");
                        DBChangeQueue.Instance.EnQueue(new StorageData()
                        {
                            Name = "LandFang",
                            Document = updateBson,
                            Query = Query.EQ("url", landFang.Text("url")),
                            Type = StorageType.Update
                        });

                    }
                }
                //DBChangeQueue.Instance.EnQueue(new StorageData()
                //{
                //    Name = "LandFang",
                //    Document = new BsonDocument().Add("category", "2"),
                //    Query = Query.EQ("eGuid", guid),
                //    Type = StorageType.Update
                //});



            }
            if (DBChangeQueue.Instance.Count > 0)
            {
                StartDBChangeProcessQuick(_mongo);
            }
            ShowMessageInfo("完成", true);
        }

        private void button40_Click(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(FilterRepeatData);

        }
        public DataOperation GetDataOperation()
        {

            DataOperation dataOp = new DataOperation(GetMongoOperation());
            return dataop;
        }
        public DataOperation GetDataOperation(MongoOperation _mongoOp)
        {

            DataOperation dataOp = new DataOperation(_mongoOp);
            return dataop;
        }
        public MongoOperation GetMongoOperation()
        {
            var connStr = "mongodb://sa:dba@192.168.1.230/SimpleCrawler";
            MongoConnectionStringBuilder builder = new MongoConnectionStringBuilder();
            builder.Server = new MongoServerAddress("192.168.1.230", 27017);
            builder.DatabaseName = "SimpleCrawler";
            builder.Username = "sa";
            builder.Password = "dba";
            builder.SocketTimeout = new TimeSpan(00, 01, 59);
            return new MongoOperation(builder);
        }

        private void FilterRepeatData(object i)
        {

            // var tableName = "LandFang";
            // var guid = "url";
            var cityNameStr = "无锡,南通,西安,烟台,佛山,泉州,北京,上海,广州,深圳,成都,昆明,大连,青岛,哈尔滨,沈阳,日照,南宁,武汉,长沙,合肥,济南,郑州,南昌,天津,杭州,兰州,长春,海口,西宁,石家庄,宁波,贵阳,西宁,乌鲁木齐,呼和浩特,银川,拉萨,福州,厦门,漳州,莆田,三明,南平,龙岩,宁德市,宁德地区,东莞,重庆";
            // var cityNameStr = "上海";
            var cityNameArr = cityNameStr.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            //var cityName = this.richTextBox1.Text.Trim();
            //if (string.IsNullOrEmpty(cityName))
            //{
            //    cityName = "西安";
            //}
            var dataOp = GetDataOperation();
            var _mongoDBOp = GetMongoOperation();
            foreach (var cityName in cityNameArr)
            {
                var tableName = "QCCEnterpriseKey";
                var guid = "guid";
                var allEnterpriseGuidList = new List<BsonDocument>();
                try
                {
                    allEnterpriseGuidList = dataOp.FindAllByQuery(tableName, Query.EQ("cityName", cityName)).SetFields("guid", "_id").Take(2000000).ToList();
                    //var allEnterpriseGuidList = dataOp.FindAll(tableName).SetFields(guid, "_id", "isUserUpdated", "所在地").Take(2000000).OrderByDescending(c => c.Int("所在地")).ThenByDescending(c=>c.Int("isUserUpdated")).ToList();
                }
                catch (Exception ex)
                {
                    ShowMessageInfo(string.Format("{0}无法读取数据\n\r", cityName), true);
                }

                BloomFilter<string> noDeleteFilter = new BloomFilter<string>(3000000);
                var deleteList = new List<string>();
                var deleteGuidList = new List<string>();
                foreach (var hitEnterprise in allEnterpriseGuidList)
                {
                    if (!noDeleteFilter.Contains(hitEnterprise.Text(guid)))
                    {
                        noDeleteFilter.Add(hitEnterprise.Text(guid));
                    }
                    else//已包含
                    {
                        //if (keyList.Contains(hitEnterprise.Text("guid")))
                        {
                            deleteList.Add(hitEnterprise.Text("_id"));
                            deleteGuidList.Add(hitEnterprise.Text(guid));
                        }
                    }
                }
                if (deleteList.Count() <= 5000)
                {
                    foreach (var deleteId in deleteList)
                    {
                        _mongoDBOp.Delete(tableName, Query.EQ("_id", ObjectId.Parse(deleteId)));
                        // DBChangeQueue.Instance.EnQueue(new StorageData() { Name = "QCCEnterpriseKey", Query = Query.EQ("_id", ObjectId.Parse(deleteId)), Type = StorageType.Delete });
                    }
                    ShowMessageInfo(string.Format("{0}处理结束，总共{1}个 删除{2}\n\r", cityName, allEnterpriseGuidList.Count(), deleteList.Count()), true);
                }
                else
                {
                    ShowMessageInfo(string.Format("{0}删除失败超出1000，总共{0}个需要删除{2}\n\r", cityName, allEnterpriseGuidList.Count(), deleteList.Count()), true);
                }


                // StartDBChangeProcessQuick(_mongoDBOp);
            }
        }

        private void button41_Click(object sender, EventArgs e)
        {
            var dataOp = GetDataOperation();
            //var allEnterpriseGuidList = dataOp.FindAllByQuery("QCCEnterpriseKey", Query.Exists("cityName", false)).SetFields("guid", "_id").Take(2000000).ToList();
            var GetCount = dataOp.FindCount("QCCEnterpriseKey", Query.EQ("cityName", ""));
            this.richTextBox1.Text = GetCount.ToString();
        }

        private void button42_Click(object sender, EventArgs e)
        {
            hi.Url = "http://gzhd.saic.gov.cn/saicsearch/qyjindex.jsp";

            //saics
            hi.PostData = "date=&end_date=&title=&content=&key=%E6%A0%B8%E5%87%86%E5%85%AC%E5%91%8A&database=qyj&search_field=all&search_type=yes&page=1";
            //date=&end_date=&title=&content=&key=%E5%85%AC%E5%8F%B8&database=saic&search_field=all&search_type=yes&page=2
            hi.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";
            hi.HeaderSet("Content-Type", "application/x-www-form-urlencoded");
            hi.HeaderSet("Accept", "text/html, application/xhtml+xml, */*");
            hi.HeaderSet("Accept-Language", "zh-CN");
            // hi.HeaderSet("Content-Length","154");
            // hi.HeaderSet("Connection","Keep-Alive");
            hi.HeaderSet("Accept-Encoding", "gzip");
            hi.HeaderSet("Host", "gzhd.saic.gov.cn");
            hi.Cookies = "yunsuo_session_verify=760b6fed201cabba9dbbc08d6ee95433; yoursessionname1=217703E3DF30F6B367DEC0B3B4EBD13F; yoursessionname0=2ACF87288AD50420E1507A8F20B69186";

            hi.Refer = "http://gzhd.saic.gov.cn/saicsearch/qyjindex.jsp";
            var ho = LibCurlNet.HttpManager.Instance.ProcessRequest(hi);
            if (ho.IsOK)
            {
                this.richTextBox1.Text = ho.TxtData;

            }
            //SimpleCrawler.HttpHelper http = new SimpleCrawler.HttpHelper();
            //SimpleCrawler.HttpItem item = null;

            //item = new SimpleCrawler.HttpItem()
            //{
            //    URL = "http://gzhd.saic.gov.cn/saicsearch/qyjindex.jsp",//URL     必需项    
            //    //URL = "http://luckymn.cn/QuestionAnswer",
            //    Method = "post",//URL     可选项 默认为Get   
            //    ContentType = "application/x-www-form-urlencoded",//返回类型    可选项有默认值 
            //    UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko",
            //    Referer = "http://gzhd.saic.gov.cn/saicsearch/qyjindex.jsp",
            //    Postdata="date=&end_date=&title=&content=&key=%E6%A0%B8%E5%87%86%E5%85%AC%E5%91%8A&database=qyj&search_field=all&search_type=yes&page=1",
            //    Accept = "text/html, application/xhtml+xml, */*",
            //    Encoding = System.Text.Encoding.GetEncoding("gb2312"), Allowautoredirect=true, KeepAlive=true,

            //};
            //item.Header.Add("Accept-Language", "zh-CN");
            //item.Header.Add("Accept-Encoding", "gzip");

            //item.Cookie = "yunsuo_session_verify=760b6fed201cabba9dbbc08d6ee95433; yoursessionname1=217703E3DF30F6B367DEC0B3B4EBD13F; yoursessionname0=2ACF87288AD50420E1507A8F20B69186";
            //item.PostEncoding = System.Text.Encoding.GetEncoding("gb2312");
            //var result = http.GetHtml(item);
            //this.richTextBox1.Text = result.Html;
        }

        private void button43_Click(object sender, EventArgs e)
        {
            var curPage = "1";
            var result = this.richTextBox1.Text.Trim();
            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(result);
            var searchPageDiv = htmlDoc.GetElementbyId("search_page");
            var allPage = 0;
            if (searchPageDiv != null && searchPageDiv.ParentNode != null)
            {
                var pageSize = Toolslib.Str.Sub(searchPageDiv.ParentNode.InnerText, "共", "页");
                curPage = Toolslib.Str.Sub(searchPageDiv.ParentNode.InnerText, "当前第", "页");
                if (!int.TryParse(pageSize, out allPage))
                {
                    allPage = 212;
                }


            }

            if (curPage == "1")
            {

                //添加到待爬取队列
            }
            ///获取url列表
            ///
            var searchResultDiv = htmlDoc.GetElementbyId("documentContainer");
            if (searchResultDiv == null) return;
            var searchResultList = searchResultDiv.SelectNodes("./div/a");
            if (searchResultList == null) return;
            //http://qyj.saic.gov.cn/ggxx/ 规则匹配
            foreach (var aNode in searchResultList)
            {
                if (aNode.Attributes["href"] != null && aNode.Attributes["href"].Value.Contains("qyj.saic.gov.cn/ggxx/"))
                {
                    this.richTextBox2.Text += String.Format("{0}\n\r", aNode.Attributes["href"].Value);
                }
            }

        }

        private void button44_Click(object sender, EventArgs e)
        {
            hi.Url = "http://qyj.saic.gov.cn/ggxx/201609/t20160930_171523.html";


            //date=&end_date=&title=&content=&key=%E5%85%AC%E5%8F%B8&database=saic&search_field=all&search_type=yes&page=2
            hi.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";
            hi.HeaderSet("Content-Type", "application/x-www-form-urlencoded");
            hi.HeaderSet("Accept", "text/html, application/xhtml+xml, */*");
            hi.HeaderSet("Accept-Language", "zh-CN");
            // hi.HeaderSet("Content-Length","154");
            // hi.HeaderSet("Connection","Keep-Alive");
            hi.HeaderSet("Accept-Encoding", "gzip");
            hi.Cookies = "yunsuo_session_verify=760b6fed201cabba9dbbc08d6ee95433; yoursessionname1=217703E3DF30F6B367DEC0B3B4EBD13F; yoursessionname0=2ACF87288AD50420E1507A8F20B69186";
            hi.HeaderSet("Host", "gzhd.saic.gov.cn");
            hi.Refer = "http://gzhd.saic.gov.cn/saicsearch/qyjindex.jsp";
            var ho = LibCurlNet.HttpManager.Instance.ProcessRequest(hi);
            if (ho.IsOK)
            {

                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.LoadHtml(ho.TxtData);

                var searchResultDiv = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='TRS_Editor']");
                if (searchResultDiv != null)
                {
                    try
                    {
                        if (searchResultDiv.ChildNodes.Count() > 0)
                        {
                            searchResultDiv.FirstChild.Remove();

                        }
                    }
                    catch (Exception ex)
                    {
                        ShowMessageInfo(ex.Message + "searchResultDiv.FirstChild.Remove");
                    }
                    //2016-10-13&nbsp;(国)登记内名预核字[2016]第11223号&nbsp;新星联盟影业有限公司&nbsp;&nbsp;<br>
                    //2016-10-13&nbsp;(国)登记内名预核字[2016]第11421号&nbsp;鸿庆楼博物馆有限公司&nbsp;&nbsp;<br>
                    var result = searchResultDiv.InnerText;
                    var splitArray = result.Split(new string[] { "<br>", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var enterpriseInfo in splitArray)
                    {
                        var columnInfoArr = enterpriseInfo.Split(new string[] { "&nbsp;" }, StringSplitOptions.RemoveEmptyEntries);
                        if (columnInfoArr.Length >= 3)
                        {
                            var date = columnInfoArr[0];
                            var info = columnInfoArr[1];
                            var name = columnInfoArr[2];
                            ShowMessageInfo(String.Format("{0}{1}{2}\n\r", date, info, name), true);
                        }

                    }

                }
            }
        }
        public static string getCheckSum(String appSecret, String nonce, String curTime)
        {
            var str=appSecret + nonce + curTime;
            
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] bytes_sha1_in = UTF8Encoding.Default.GetBytes(str);
            byte[] bytes_sha1_out = sha1.ComputeHash(bytes_sha1_in);
            string str_sha1_out = BitConverter.ToString(bytes_sha1_out);
             str_sha1_out = str_sha1_out.Replace("-", "");
            return str_sha1_out;
        }
      
        private void button45_Click(object sender, EventArgs e)
        {
            hi.Url = "https://api.netease.im/sms/sendcode.action";
             var minDate=DateTime.Parse("1970-01-01");  
             var totalSeconds=(DateTime.Now - minDate).TotalSeconds;
             string appKey = "1d44dbc95eef958f4bd47e0747e8df1c";
             string appSecret = "76c5f266b4f7";
             string nonce = "12345";
             string curTime = Math.Round(totalSeconds,0).ToString();
             string checkSum = getCheckSum(appSecret, nonce, curTime).ToLower();
            //saics
             hi.PostData = "mobile=18638808245";
            //date=&end_date=&title=&content=&key=%E5%85%AC%E5%8F%B8&database=saic&search_field=all&search_type=yes&page=2
            hi.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";
            hi.HeaderSet("Content-Type", "application/x-www-form-urlencoded");
            hi.HeaderSet("AppKey", appKey);
            hi.HeaderSet("CurTime", curTime);
            hi.HeaderSet("CheckSum", checkSum);
            hi.HeaderSet("Nonce", nonce);
            hi.HeaderSet("charset", "utf-8");
            var ho = LibCurlNet.HttpManager.Instance.ProcessRequest(hi);
            if (ho.IsOK)
            {
                this.richTextBox1.Text = ho.TxtData;
            }
        }

        private void button46_Click(object sender, EventArgs e)
        {
            //验证
            hi.Url = "https://api.netease.im/sms/verifycode.action";
            var minDate = DateTime.Parse("1970-01-01");
            var totalSeconds = (DateTime.Now - minDate).TotalSeconds;
            string appKey = "1d44dbc95eef958f4bd47e0747e8df1c";
            string appSecret = "76c5f266b4f7";
            string nonce = "12345";
            string curTime = Math.Round(totalSeconds, 0).ToString();
            string checkSum = getCheckSum(appSecret, nonce, curTime).ToLower();
            string code = "2189";
            //saics
            hi.PostData = "mobile=18638808245&code=" + code;
            //date=&end_date=&title=&content=&key=%E5%85%AC%E5%8F%B8&database=saic&search_field=all&search_type=yes&page=2
            hi.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";
            hi.HeaderSet("Content-Type", "application/x-www-form-urlencoded");
            hi.HeaderSet("AppKey", appKey);
            hi.HeaderSet("CurTime", curTime);
            hi.HeaderSet("CheckSum", checkSum);
            hi.HeaderSet("Nonce", nonce);
            hi.HeaderSet("charset", "utf-8");
            var ho = LibCurlNet.HttpManager.Instance.ProcessRequest(hi);
            if (ho.IsOK)
            {
                this.richTextBox1.Text = ho.TxtData;
            }
        }

        private void button47_Click(object sender, EventArgs e)
        {
            var helper = new LeetCodeHelper();
            var num = new int[] {3, 2, 2 ,3};
            var result = helper.RemoveElement(num,3);
        }

        #region leetCode 
        /// <summary>
        /// Given nums = [2, 7, 11, 15], target = 9, Because nums[0] + nums[1] = 2 + 7 = 9, return [0, 1].
        /// </summary>
        /// <param name="nums"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public int[] TwoSum(int[] nums, int target)
        {
            List<int> result =new List<int>() ;
           
            for (var i = 0; i <= nums.Length - 1 ; i++)
            {
                var curValue = nums[i];
                
                var nextValue = target - curValue;

                for (var j = i + 1; j <= nums.Length - 1 ; j++)
                {
                    if (nums[j] == nextValue)
                    {
                        result.Add(i);
                        result.Add(j);
                    }
                }
            
            }

            return result.ToArray();
        }
        /// <summary>
        /// It has at least 6 characters and at most 20 characters.
        /// must contain at least one lowercase letter, at least one uppercase letter, and at least one digit.
        /// It must NOT contain three repeating characters in a row ("...aaa..." is weak, but "...aa...a..." is strong, assuming other conditions are met).
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public int StrongPasswordChecker(string s)
        {
            var needChange = 0;
            if (s.Length < 6)
            {
                needChange= 6 - s.Length;
                return needChange;
            }
            if (s.Length > 20) { needChange= Math.Abs(20-s.Length);
                if (needChange > 2)
                {
                    return needChange;
                }
            }
            if (s.ToLower() == s || s.ToUpper() == s)
            {
                if (s.ToLower() == s)//allLower
                {
                    needChange = needChange + 1;
                    
                };
                if (s.ToUpper() == s)//allUpler
                {
                    needChange = needChange + 1;
                   
                }
                for (var c = '0'; c <= '9'; c++)
                {
                    if (s.IndexOf(string.Format("{0}{1}{2}", c, c, c)) != -1)
                    {
                        needChange = needChange + 1;
                        break;
                    }
                }
                if (needChange > 0) return needChange;
            }
            var finalNeedChange = 0;
            for (var c = 'A'; c <= 'z'; c++)//有几个重复改几个
            {
                if (s.IndexOf(string.Format("{0}{1}{2}", c, c, c)) != -1)
                {
                    finalNeedChange = finalNeedChange + 1;
                   
                }
            }

            return Math.Max(needChange,finalNeedChange);
        }
       
        #endregion

        private void button48_Click(object sender, EventArgs e)
        {
            var connStr = "mongodb://sa:dba@59.61.72.35/MZCityLibrary";
            DataOperation dataOp = new DataOperation(new MongoOperation(connStr));
            MongoOperation _mongoDBOp = new MongoOperation(connStr);
            var tableName = "ProjectHouseStatic";//设备账号注册
            var updateBson = new BsonDocument().Add("cityName", "深圳");

            dataOp.Update(tableName,Query.Exists("cityName",false), updateBson);
        }


        //List<string> urlFilter = new List<string>();
        //private bool IsKeyWordUrlSplited_APP(DataReceivedEventArgs args, int firstRecordCount, JToken htmlDoc)
        //{
        //    if (firstRecordCount <= 1000) return false;
        //    //registcapiNew 注册资本//startdateNew 成立日期//statuscodeNew 企业状态 industrycodeNew 行业门类
        //    var registCapiBeginParam = GetUrlParam(args.Url, "registCapiBegin");
        //    var registCapiEndParam = GetUrlParam(args.Url, "registCapiEnd");
        //    var statusCodeParam = GetUrlParam(args.Url, "statusCode");
        //    var startDateBeginParam = GetUrlParam(args.Url, "startDateBegin");
        //    var startDateEndParam = GetUrlParam(args.Url, "startDateEnd");
        //    var industryCode = GetUrlParam(args.Url, "industryCode");
        //    var subIndustryCode = GetUrlParam(args.Url, "subIndustryCode");
        //    try
        //    {
        //        #region 行业门类大类
        //        var industryDataValueList = GetFilterDataValue_App(htmlDoc, "industrycode");
        //        if (industryDataValueList.Count() > 0 && string.IsNullOrEmpty(industryCode))
        //        {
        //            foreach (var dataValue in industryDataValueList)//2016
        //            {
        //                var curUrl = args.Url.Replace("&industryCode=", string.Format("&industryCode={0}", dataValue));
        //                if (!urlFilter.Contains(curUrl))//防止重复爬取
        //                {
        //                    UrlQueue.Instance.EnQueue(new UrlInfo(curUrl));//添加条件过滤
        //                    urlFilter.Add(curUrl);
        //                }
        //            }
        //            return true;

        //        }
        //        #endregion
        //        #region 成立日期分支筛选
        //        var dateDataValueList = GetFilterDataValue_App(htmlDoc, "startdateyear");
        //        if (dateDataValueList.Count() > 0 && string.IsNullOrEmpty(startDateBeginParam) && string.IsNullOrEmpty(startDateEndParam))
        //        {
        //            foreach (var dataValue in dateDataValueList)//2016
        //            {
        //                var startDateBegin = string.Format("{0}0101", dataValue);
        //                var startDateEnd = string.Format("{0}1231", dataValue);
        //                if (dataValue == "0")
        //                {
        //                    startDateBegin = "0";
        //                    startDateEnd = "0";
        //                }

        //                var curUrl = args.Url.Replace("&startDateBegin=", string.Format("&startDateBegin={0}", startDateBegin));
        //                curUrl = curUrl.Replace("&startDateEnd=", string.Format("&startDateEnd={0}", startDateEnd));

        //                if (!urlFilter.Contains(curUrl))//防止重复爬取
        //                {
        //                    UrlQueue.Instance.EnQueue(new UrlInfo(curUrl));//添加条件过滤
        //                    urlFilter.Add(curUrl);
        //                }
        //            }
        //            return true;
        //        }
        //        #endregion
        //        #region 行业门类大类
        //        var subIndustryDataValueList = GetFilterDataValue_App(htmlDoc, "subindustrycode");
        //        if (subIndustryDataValueList.Count() > 0 && string.IsNullOrEmpty(subIndustryCode))
        //        {
        //            foreach (var dataValue in subIndustryDataValueList)//2016
        //            {
        //                var curUrl = args.Url.Replace("&subIndustryCode=", string.Format("&subIndustryCode={0}", dataValue));
        //                if (!urlFilter.Contains(curUrl))//防止重复爬取
        //                {
        //                    UrlQueue.Instance.EnQueue(new UrlInfo(curUrl));//添加条件过滤
        //                    urlFilter.Add(curUrl);
        //                }
        //            }
        //            return true;

        //        }
        //        #endregion



        //        #region 企业状态筛选
        //        var statusDataValueList = GetFilterDataValue_App(htmlDoc, "statuscode");
        //        if (statusDataValueList.Count() > 0 && string.IsNullOrEmpty(statusCodeParam))
        //        {
        //            foreach (var statusCode in statusDataValueList)//2016
        //            {
        //                var curUrl = args.Url.Replace("&statusCode=", string.Format("&statusCode={0}", statusCode));
        //                if (!urlFilter.Contains(curUrl))//防止重复爬取
        //                {
        //                    UrlQueue.Instance.EnQueue(new UrlInfo(curUrl));//添加条件过滤
        //                    urlFilter.Add(curUrl);
        //                }
        //            }
        //            return true;
        //        }
        //        #endregion
        //        #region 注册资本分支筛选
        //        var dataValueList = GetFilterDataValue_App(htmlDoc, "registcapi");
        //        if (dataValueList.Count() > 0 && string.IsNullOrEmpty(registCapiBeginParam) && string.IsNullOrEmpty(registCapiEndParam))
        //        {
        //            foreach (var dataValue in dataValueList)
        //            {
        //                var dataValueArray = dataValue.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
        //                if (dataValueArray.Length >= 2)
        //                {
        //                    var registCapiBegin = dataValueArray[0];
        //                    var registCapiEnd = dataValueArray[1];
        //                    if (registCapiEnd == "0")
        //                    {
        //                        registCapiEnd = string.Empty;
        //                    }
        //                    var curUrl = args.Url;
        //                    curUrl = curUrl.Replace("&registCapiBegin=", string.Format("&registCapiBegin={0}", registCapiBegin));
        //                    curUrl = curUrl.Replace("&registCapiEnd=", string.Format("&registCapiEnd={0}", registCapiEnd));
        //                    if (!urlFilter.Contains(curUrl))//防止重复爬取
        //                    {
        //                        UrlQueue.Instance.EnQueue(new UrlInfo(curUrl));//添加条件过滤
        //                        urlFilter.Add(curUrl);
        //                    }
        //                }
        //            }
        //            return true;
        //        }
        //        #endregion
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    return false;
        //}
        //private List<string> GetFilterDataValue_App(JToken groupItems, string name)
        //{

        //    List<string> result = new List<string>();
        //    if (name == "registcapi")//后续此处可继续细化
        //    {
        //        result.Add("1-499");
        //        result.Add("500-1000");
        //        result.Add("1000-5000");
        //        result.Add("5000-0");
        //    }
        //    else
        //    {
        //        var hitGourp = groupItems.Where(c => c["Key"].ToString() == name).FirstOrDefault();
        //        if (hitGourp != null)
        //        {
        //            foreach (var item in hitGourp["Items"])
        //            {
        //                if (item["Count"].ToString() != "0")
        //                {
        //                    result.Add(item["Value"].ToString());
        //                }
        //            }

        //        }
        //    }
        //    return result;
        //}

        //public class JToken:IEnumerable<JToken>
        //{
        //    public JToken(string _key,string _value)
        //    {
        //        key = _key; value = _value;
        //    }
        //    public List<JToken> ObjList = new List<JToken>();
        //    public  JToken this[string _key] { 
        //      get{
        //          var hitObj=ObjList.Where(c=>c.key==_key).FirstOrDefault();
        //          if(hitObj!=null)
        //          {
        //          return hitObj;
        //          }else{
        //          return null;
        //          }
        //      }
        //    }
        //    public override string ToString()
        //    {
        //        return key;

        //    }
        //    public string key;
        //    public string value;
        //}
    }
}
