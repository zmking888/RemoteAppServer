namespace RemoteAppServer
{
    partial class Form1
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
            this.btnInstallHook = new System.Windows.Forms.Button();
            this.btnUninstallHook = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnInstallHook
            // 
            this.btnInstallHook.Location = new System.Drawing.Point(255, 118);
            this.btnInstallHook.Name = "btnInstallHook";
            this.btnInstallHook.Size = new System.Drawing.Size(124, 23);
            this.btnInstallHook.TabIndex = 0;
            this.btnInstallHook.Text = "使用客户端输入法";
            this.btnInstallHook.UseVisualStyleBackColor = true;
            this.btnInstallHook.Click += new System.EventHandler(this.btnInstallHook_Click);
            // 
            // btnUninstallHook
            // 
            this.btnUninstallHook.Location = new System.Drawing.Point(398, 118);
            this.btnUninstallHook.Name = "btnUninstallHook";
            this.btnUninstallHook.Size = new System.Drawing.Size(124, 23);
            this.btnUninstallHook.TabIndex = 1;
            this.btnUninstallHook.Text = "使用服务端输入法";
            this.btnUninstallHook.UseVisualStyleBackColor = true;
            this.btnUninstallHook.Click += new System.EventHandler(this.btnUninstallHook_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(25, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "label1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(558, 153);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnUninstallHook);
            this.Controls.Add(this.btnInstallHook);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Activated += new System.EventHandler(this.Form1_Activated);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnInstallHook;
        private System.Windows.Forms.Button btnUninstallHook;
        private System.Windows.Forms.Label label1;
    }
}

