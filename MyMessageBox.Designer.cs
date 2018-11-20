namespace ModuleTestV8
{
    partial class MyMessageBox
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
            this.ok = new System.Windows.Forms.Button();
            this.msg = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // ok
            // 
            this.ok.Font = new System.Drawing.Font("微軟正黑體", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.ok.Location = new System.Drawing.Point(314, 152);
            this.ok.Name = "ok";
            this.ok.Size = new System.Drawing.Size(168, 58);
            this.ok.TabIndex = 0;
            this.ok.Text = "OK";
            this.ok.UseVisualStyleBackColor = true;
            this.ok.Click += new System.EventHandler(this.ok_Click);
            // 
            // msg
            // 
            this.msg.Font = new System.Drawing.Font("Verdana", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.msg.Location = new System.Drawing.Point(12, 9);
            this.msg.Name = "msg";
            this.msg.Size = new System.Drawing.Size(770, 140);
            this.msg.TabIndex = 1;
            this.msg.Text = "msg";
            this.msg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MyMessageBox
            // 
            this.AcceptButton = this.ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(794, 222);
            this.Controls.Add(this.msg);
            this.Controls.Add(this.ok);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MyMessageBox";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Warning";
            this.Load += new System.EventHandler(this.MyMessageBox_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button ok;
        private System.Windows.Forms.Label msg;
    }
}