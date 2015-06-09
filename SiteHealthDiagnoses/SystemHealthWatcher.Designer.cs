namespace SiteHealthDiagnoses
{
    partial class SystemHealthWatcher
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.functionListCtrl = new System.Windows.Forms.CheckedListBox();
            this.start = new System.Windows.Forms.Button();
            this.refesh = new System.Windows.Forms.Button();
            this.conStr = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SaveConfigBtn = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.pushUrlText = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.CustomerCodeCmb = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // functionListCtrl
            // 
            this.functionListCtrl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.functionListCtrl.FormattingEnabled = true;
            this.functionListCtrl.Location = new System.Drawing.Point(3, 92);
            this.functionListCtrl.Name = "functionListCtrl";
            this.functionListCtrl.Size = new System.Drawing.Size(483, 212);
            this.functionListCtrl.TabIndex = 0;
            // 
            // start
            // 
            this.start.Location = new System.Drawing.Point(406, 322);
            this.start.Name = "start";
            this.start.Size = new System.Drawing.Size(75, 23);
            this.start.TabIndex = 1;
            this.start.Text = "开始运行";
            this.start.UseVisualStyleBackColor = true;
            this.start.Click += new System.EventHandler(this.start_Click);
            // 
            // refesh
            // 
            this.refesh.Location = new System.Drawing.Point(3, 322);
            this.refesh.Name = "refesh";
            this.refesh.Size = new System.Drawing.Size(121, 23);
            this.refesh.TabIndex = 2;
            this.refesh.Text = "初始化并读取功能";
            this.refesh.UseVisualStyleBackColor = true;
            this.refesh.Click += new System.EventHandler(this.refesh_Click);
            // 
            // conStr
            // 
            this.conStr.Location = new System.Drawing.Point(87, 6);
            this.conStr.Name = "conStr";
            this.conStr.Size = new System.Drawing.Size(399, 21);
            this.conStr.TabIndex = 3;
            this.conStr.Text = "mongodb://sa:dba@192.168.1.230/WorkPlanManage";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "连接字符串：";
            // 
            // SaveConfigBtn
            // 
            this.SaveConfigBtn.Location = new System.Drawing.Point(189, 323);
            this.SaveConfigBtn.Name = "SaveConfigBtn";
            this.SaveConfigBtn.Size = new System.Drawing.Size(75, 23);
            this.SaveConfigBtn.TabIndex = 5;
            this.SaveConfigBtn.Text = "保存配置";
            this.SaveConfigBtn.UseVisualStyleBackColor = true;
            this.SaveConfigBtn.Click += new System.EventHandler(this.SaveConfigBtn_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 307);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(0, 12);
            this.label2.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 36);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "推送地址：";
            // 
            // pushUrlText
            // 
            this.pushUrlText.Location = new System.Drawing.Point(87, 33);
            this.pushUrlText.Name = "pushUrlText";
            this.pushUrlText.Size = new System.Drawing.Size(399, 21);
            this.pushUrlText.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 61);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 4;
            this.label4.Text = "客户代码：";
            // 
            // CustomerCodeCmb
            // 
            this.CustomerCodeCmb.FormattingEnabled = true;
            this.CustomerCodeCmb.Items.AddRange(new object[] {
            "F8A3250F-A433-42be-9F68-803BBF01ZHHY",
            "6F9619FF-8B86-D011-B42D-00C04FC964SN",
            "71E8DBA3-5DC6-4597-9DCD-F3CC1F04FCXH",
            "73345DB5-DFE5-41F8-B37E-7D83335AZHTZ",
            "84C7D7E3-26C2-479F-B67F-F240E506CEQX",
            "84C7D7E3-26C2-479F-B67F-F240E506QXSD",
            "802812B4-B670-48C2-9E20-F9954CA65CXC",
            "4DD74057-DDF4-4533-AFE8-51AC263B05LF",
            "4BF8120C-DB2C-495D-8BC2-FD9189E8NJHY",
            "958AEDDF-04F0-4702-B5F6-FC300262F96D",
            "6B47BD15-0400-1622-0250-39E3DB0411JH",
            "5D8A608E-85A6-45C3-A3FE-E3B24623DWPM",
            "DE548D75-FC95-40CB-B6AB-A0E9E8FF78ZY"});
            this.CustomerCodeCmb.Location = new System.Drawing.Point(87, 62);
            this.CustomerCodeCmb.Name = "CustomerCodeCmb";
            this.CustomerCodeCmb.Size = new System.Drawing.Size(399, 20);
            this.CustomerCodeCmb.TabIndex = 7;
            // 
            // SystemHealthWatcher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(493, 358);
            this.Controls.Add(this.CustomerCodeCmb);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.SaveConfigBtn);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pushUrlText);
            this.Controls.Add(this.conStr);
            this.Controls.Add(this.refesh);
            this.Controls.Add(this.start);
            this.Controls.Add(this.functionListCtrl);
            this.Name = "SystemHealthWatcher";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.SystemWatcher_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox functionListCtrl;
        private System.Windows.Forms.Button start;
        private System.Windows.Forms.Button refesh;
        private System.Windows.Forms.TextBox conStr;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button SaveConfigBtn;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox pushUrlText;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox CustomerCodeCmb;

    }
}

