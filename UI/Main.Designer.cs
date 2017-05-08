namespace UI
{
    partial class Main
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
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.btnDocExt = new System.Windows.Forms.Button();
            this.btnResumeParse = new System.Windows.Forms.Button();
            this.txtTxt = new System.Windows.Forms.TextBox();
            this.txtResult = new System.Windows.Forms.TextBox();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnParse = new System.Windows.Forms.Button();
            this.btnScan = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.lblEnqueueStatus = new System.Windows.Forms.Label();
            this.ckAsync = new System.Windows.Forms.CheckBox();
            this.lbConsole = new System.Windows.Forms.ListBox();
            this.numTheadCount = new System.Windows.Forms.NumericUpDown();
            this.btnExit = new System.Windows.Forms.Button();
            this.txtQueueLength = new System.Windows.Forms.TextBox();
            this.txtFileCount = new System.Windows.Forms.TextBox();
            this.txtEnqueueCount = new System.Windows.Forms.TextBox();
            this.txtHandleCount = new System.Windows.Forms.TextBox();
            this.txtCpuInfo = new System.Windows.Forms.TextBox();
            this.txtServiceStatus = new System.Windows.Forms.TextBox();
            this.txtTotalTime = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numTheadCount)).BeginInit();
            this.SuspendLayout();
            // 
            // btnDocExt
            // 
            this.btnDocExt.Enabled = false;
            this.btnDocExt.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnDocExt.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnDocExt.Location = new System.Drawing.Point(6, 31);
            this.btnDocExt.Name = "btnDocExt";
            this.btnDocExt.Size = new System.Drawing.Size(100, 40);
            this.btnDocExt.TabIndex = 0;
            this.btnDocExt.Text = "文档抽取";
            this.btnDocExt.UseVisualStyleBackColor = true;
            this.btnDocExt.Click += new System.EventHandler(this.btnDocExt_Click);
            // 
            // btnResumeParse
            // 
            this.btnResumeParse.Enabled = false;
            this.btnResumeParse.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnResumeParse.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnResumeParse.Location = new System.Drawing.Point(7, 77);
            this.btnResumeParse.Name = "btnResumeParse";
            this.btnResumeParse.Size = new System.Drawing.Size(100, 40);
            this.btnResumeParse.TabIndex = 1;
            this.btnResumeParse.Text = "简历解析";
            this.btnResumeParse.UseVisualStyleBackColor = true;
            this.btnResumeParse.Click += new System.EventHandler(this.btnResumeParse_Click);
            // 
            // txtTxt
            // 
            this.txtTxt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTxt.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtTxt.Location = new System.Drawing.Point(113, 31);
            this.txtTxt.Multiline = true;
            this.txtTxt.Name = "txtTxt";
            this.txtTxt.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtTxt.Size = new System.Drawing.Size(613, 256);
            this.txtTxt.TabIndex = 2;
            // 
            // txtResult
            // 
            this.txtResult.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtResult.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtResult.Location = new System.Drawing.Point(113, 326);
            this.txtResult.Multiline = true;
            this.txtResult.Name = "txtResult";
            this.txtResult.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtResult.Size = new System.Drawing.Size(613, 251);
            this.txtResult.TabIndex = 3;
            // 
            // btnClear
            // 
            this.btnClear.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.btnClear.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnClear.Location = new System.Drawing.Point(12, 582);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(87, 40);
            this.btnClear.TabIndex = 4;
            this.btnClear.Text = "清空";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnParse
            // 
            this.btnParse.Enabled = false;
            this.btnParse.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnParse.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnParse.Location = new System.Drawing.Point(352, 289);
            this.btnParse.Name = "btnParse";
            this.btnParse.Size = new System.Drawing.Size(87, 35);
            this.btnParse.TabIndex = 5;
            this.btnParse.Text = "解析";
            this.btnParse.UseVisualStyleBackColor = true;
            this.btnParse.Click += new System.EventHandler(this.btnParse_Click);
            // 
            // btnScan
            // 
            this.btnScan.Enabled = false;
            this.btnScan.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnScan.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnScan.Location = new System.Drawing.Point(6, 185);
            this.btnScan.Name = "btnScan";
            this.btnScan.Size = new System.Drawing.Size(100, 40);
            this.btnScan.TabIndex = 7;
            this.btnScan.Text = "磁盘扫描";
            this.btnScan.UseVisualStyleBackColor = true;
            this.btnScan.Click += new System.EventHandler(this.btnScan_Click);
            // 
            // btnStart
            // 
            this.btnStart.Enabled = false;
            this.btnStart.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnStart.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStart.Location = new System.Drawing.Point(7, 229);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(42, 57);
            this.btnStart.TabIndex = 9;
            this.btnStart.Text = "开始";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnStop.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStop.Location = new System.Drawing.Point(65, 229);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(41, 57);
            this.btnStop.TabIndex = 10;
            this.btnStop.Text = "暂停";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // lblEnqueueStatus
            // 
            this.lblEnqueueStatus.AutoSize = true;
            this.lblEnqueueStatus.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblEnqueueStatus.ForeColor = System.Drawing.Color.Green;
            this.lblEnqueueStatus.Location = new System.Drawing.Point(11, 9);
            this.lblEnqueueStatus.Name = "lblEnqueueStatus";
            this.lblEnqueueStatus.Size = new System.Drawing.Size(53, 12);
            this.lblEnqueueStatus.TabIndex = 12;
            this.lblEnqueueStatus.Text = "入队信息";
            this.lblEnqueueStatus.Click += new System.EventHandler(this.lblEnqueueStatus_Click);
            // 
            // ckAsync
            // 
            this.ckAsync.AutoSize = true;
            this.ckAsync.Checked = true;
            this.ckAsync.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckAsync.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ckAsync.Location = new System.Drawing.Point(24, 133);
            this.ckAsync.Name = "ckAsync";
            this.ckAsync.Size = new System.Drawing.Size(56, 24);
            this.ckAsync.TabIndex = 13;
            this.ckAsync.Text = "异步";
            this.ckAsync.UseVisualStyleBackColor = true;
            // 
            // lbConsole
            // 
            this.lbConsole.BackColor = System.Drawing.Color.Black;
            this.lbConsole.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbConsole.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbConsole.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.lbConsole.FormattingEnabled = true;
            this.lbConsole.ItemHeight = 12;
            this.lbConsole.Location = new System.Drawing.Point(113, 582);
            this.lbConsole.Name = "lbConsole";
            this.lbConsole.Size = new System.Drawing.Size(601, 86);
            this.lbConsole.TabIndex = 14;
            // 
            // numTheadCount
            // 
            this.numTheadCount.Location = new System.Drawing.Point(24, 158);
            this.numTheadCount.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numTheadCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numTheadCount.Name = "numTheadCount";
            this.numTheadCount.Size = new System.Drawing.Size(56, 23);
            this.numTheadCount.TabIndex = 15;
            this.numTheadCount.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // btnExit
            // 
            this.btnExit.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnExit.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnExit.Location = new System.Drawing.Point(12, 625);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(87, 40);
            this.btnExit.TabIndex = 19;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // txtQueueLength
            // 
            this.txtQueueLength.BackColor = System.Drawing.SystemColors.InfoText;
            this.txtQueueLength.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtQueueLength.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.txtQueueLength.Location = new System.Drawing.Point(7, 327);
            this.txtQueueLength.Name = "txtQueueLength";
            this.txtQueueLength.ReadOnly = true;
            this.txtQueueLength.Size = new System.Drawing.Size(100, 31);
            this.txtQueueLength.TabIndex = 20;
            this.txtQueueLength.Text = "0";
            this.txtQueueLength.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtFileCount
            // 
            this.txtFileCount.BackColor = System.Drawing.SystemColors.InfoText;
            this.txtFileCount.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtFileCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.txtFileCount.Location = new System.Drawing.Point(7, 363);
            this.txtFileCount.Name = "txtFileCount";
            this.txtFileCount.ReadOnly = true;
            this.txtFileCount.Size = new System.Drawing.Size(100, 31);
            this.txtFileCount.TabIndex = 21;
            this.txtFileCount.Text = "0";
            this.txtFileCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtEnqueueCount
            // 
            this.txtEnqueueCount.BackColor = System.Drawing.SystemColors.InfoText;
            this.txtEnqueueCount.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtEnqueueCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.txtEnqueueCount.Location = new System.Drawing.Point(7, 399);
            this.txtEnqueueCount.Name = "txtEnqueueCount";
            this.txtEnqueueCount.ReadOnly = true;
            this.txtEnqueueCount.Size = new System.Drawing.Size(100, 31);
            this.txtEnqueueCount.TabIndex = 22;
            this.txtEnqueueCount.Text = "0";
            this.txtEnqueueCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtHandleCount
            // 
            this.txtHandleCount.BackColor = System.Drawing.SystemColors.InfoText;
            this.txtHandleCount.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtHandleCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.txtHandleCount.Location = new System.Drawing.Point(7, 435);
            this.txtHandleCount.Name = "txtHandleCount";
            this.txtHandleCount.ReadOnly = true;
            this.txtHandleCount.Size = new System.Drawing.Size(100, 31);
            this.txtHandleCount.TabIndex = 23;
            this.txtHandleCount.Text = "0";
            this.txtHandleCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtCpuInfo
            // 
            this.txtCpuInfo.BackColor = System.Drawing.SystemColors.InfoText;
            this.txtCpuInfo.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtCpuInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.txtCpuInfo.Location = new System.Drawing.Point(7, 471);
            this.txtCpuInfo.Name = "txtCpuInfo";
            this.txtCpuInfo.ReadOnly = true;
            this.txtCpuInfo.Size = new System.Drawing.Size(100, 31);
            this.txtCpuInfo.TabIndex = 24;
            this.txtCpuInfo.Text = "0";
            this.txtCpuInfo.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtServiceStatus
            // 
            this.txtServiceStatus.BackColor = System.Drawing.SystemColors.InfoText;
            this.txtServiceStatus.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtServiceStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.txtServiceStatus.Location = new System.Drawing.Point(7, 507);
            this.txtServiceStatus.Name = "txtServiceStatus";
            this.txtServiceStatus.ReadOnly = true;
            this.txtServiceStatus.Size = new System.Drawing.Size(100, 31);
            this.txtServiceStatus.TabIndex = 25;
            this.txtServiceStatus.Text = "服务状态";
            this.txtServiceStatus.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtTotalTime
            // 
            this.txtTotalTime.BackColor = System.Drawing.SystemColors.InfoText;
            this.txtTotalTime.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtTotalTime.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.txtTotalTime.Location = new System.Drawing.Point(7, 543);
            this.txtTotalTime.Name = "txtTotalTime";
            this.txtTotalTime.ReadOnly = true;
            this.txtTotalTime.Size = new System.Drawing.Size(100, 31);
            this.txtTotalTime.TabIndex = 26;
            this.txtTotalTime.Text = "0";
            this.txtTotalTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(729, 674);
            this.Controls.Add(this.txtTotalTime);
            this.Controls.Add(this.txtServiceStatus);
            this.Controls.Add(this.txtCpuInfo);
            this.Controls.Add(this.txtHandleCount);
            this.Controls.Add(this.txtEnqueueCount);
            this.Controls.Add(this.txtFileCount);
            this.Controls.Add(this.txtQueueLength);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.numTheadCount);
            this.Controls.Add(this.lbConsole);
            this.Controls.Add(this.ckAsync);
            this.Controls.Add(this.lblEnqueueStatus);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.btnScan);
            this.Controls.Add(this.btnParse);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.txtResult);
            this.Controls.Add(this.txtTxt);
            this.Controls.Add(this.btnResumeParse);
            this.Controls.Add(this.btnDocExt);
            this.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "XParser";
            this.Load += new System.EventHandler(this.Main_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numTheadCount)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnDocExt;
        private System.Windows.Forms.Button btnResumeParse;
        private System.Windows.Forms.TextBox txtTxt;
        private System.Windows.Forms.TextBox txtResult;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnParse;
        private System.Windows.Forms.Button btnScan;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label lblEnqueueStatus;
        private System.Windows.Forms.CheckBox ckAsync;
        private System.Windows.Forms.ListBox lbConsole;
        private System.Windows.Forms.NumericUpDown numTheadCount;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.TextBox txtQueueLength;
        private System.Windows.Forms.TextBox txtFileCount;
        private System.Windows.Forms.TextBox txtEnqueueCount;
        private System.Windows.Forms.TextBox txtHandleCount;
        private System.Windows.Forms.TextBox txtCpuInfo;
        private System.Windows.Forms.TextBox txtServiceStatus;
        private System.Windows.Forms.TextBox txtTotalTime;
    }
}

