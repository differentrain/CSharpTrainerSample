namespace CETT
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
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.CheckBoxPassBy = new System.Windows.Forms.CheckBox();
            this.TimerListener = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // CheckBoxPassBy
            // 
            this.CheckBoxPassBy.AutoSize = true;
            this.CheckBoxPassBy.Enabled = false;
            this.CheckBoxPassBy.Location = new System.Drawing.Point(12, 10);
            this.CheckBoxPassBy.Name = "CheckBoxPassBy";
            this.CheckBoxPassBy.Size = new System.Drawing.Size(174, 16);
            this.CheckBoxPassBy.TabIndex = 0;
            this.CheckBoxPassBy.Text = "Passby CE tutorial step 2";
            this.CheckBoxPassBy.UseVisualStyleBackColor = true;
            this.CheckBoxPassBy.CheckedChanged += new System.EventHandler(this.CheckBoxPassBy_CheckedChanged);
            // 
            // TimerListener
            // 
            this.TimerListener.Enabled = true;
            this.TimerListener.Tick += new System.EventHandler(this.TimerListener_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(198, 38);
            this.Controls.Add(this.CheckBoxPassBy);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CETT";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox CheckBoxPassBy;
        private System.Windows.Forms.Timer TimerListener;
    }
}

