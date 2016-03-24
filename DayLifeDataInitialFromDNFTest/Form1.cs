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
                        string value =  Microsoft.VisualBasic.Strings.StrConv(this.DBStringToNormal(dbStr), Microsoft.VisualBasic.VbStrConv.SimplifiedChinese, 0).Replace("%%", "%");
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
                        itemDesc =  Microsoft.VisualBasic.Strings.StrConv(itemDesc, Microsoft.VisualBasic.VbStrConv.SimplifiedChinese, 0).Replace("%%", "%");
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
            ADToDB odb = new ADToDB();
            ADDepartment department2 = odb.GetADTreeHQC_Modify(entity, list2, curDep);
            new SerializerXml<ADDepartment>(department2).BuildXml("tree.xml");
            OrganizationAD nad = new OrganizationAD(dataOp);
            nad.OrganizationSave(entity, department2);
            //nad.UserInsertByDepType(odb.UserListFilter(department2), 1, 0, 0, 1);
            MessageBox.Show("oK");

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


 


 


    }
}
