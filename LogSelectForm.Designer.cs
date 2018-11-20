namespace ModuleTestV8
{
    partial class LogSelectForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.logList = new System.Windows.Forms.CheckedListBox();
            this.convertBtn = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // logList
            // 
            this.logList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.logList.CheckOnClick = true;
            this.logList.FormattingEnabled = true;
            this.logList.Location = new System.Drawing.Point(13, 13);
            this.logList.Name = "logList";
            this.logList.Size = new System.Drawing.Size(531, 480);
            this.logList.TabIndex = 0;
            // 
            // convertBtn
            // 
            this.convertBtn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.convertBtn.Location = new System.Drawing.Point(12, 529);
            this.convertBtn.Name = "convertBtn";
            this.convertBtn.Size = new System.Drawing.Size(531, 26);
            this.convertBtn.TabIndex = 1;
            this.convertBtn.Text = "Convert";
            this.convertBtn.UseVisualStyleBackColor = true;
            this.convertBtn.Click += new System.EventHandler(this.convertBtn_Click);
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(13, 499);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(530, 24);
            this.progressBar.TabIndex = 2;
            // 
            // LogSelectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(556, 567);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.convertBtn);
            this.Controls.Add(this.logList);
            this.Name = "LogSelectForm";
            this.Text = "LogSelectForm";
            this.Load += new System.EventHandler(this.LogSelectForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckedListBox logList;
        private System.Windows.Forms.Button convertBtn;
        private System.Windows.Forms.ProgressBar progressBar;

    }
}