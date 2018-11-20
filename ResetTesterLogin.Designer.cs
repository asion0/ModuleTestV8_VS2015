namespace ModuleTestV8
{
    partial class resetTesterLogin
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
            this.baudSelect = new System.Windows.Forms.ComboBox();
            this.testResetPeriod = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.ok = new System.Windows.Forms.Button();
            this.checkInterval = new System.Windows.Forms.TextBox();
            this.checkInterval_t = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // baudSelect
            // 
            this.baudSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.baudSelect.FormattingEnabled = true;
            this.baudSelect.Location = new System.Drawing.Point(104, 42);
            this.baudSelect.Name = "baudSelect";
            this.baudSelect.Size = new System.Drawing.Size(121, 20);
            this.baudSelect.TabIndex = 0;
            // 
            // testResetPeriod
            // 
            this.testResetPeriod.Location = new System.Drawing.Point(132, 94);
            this.testResetPeriod.MaxLength = 8;
            this.testResetPeriod.Name = "testResetPeriod";
            this.testResetPeriod.Size = new System.Drawing.Size(58, 22);
            this.testResetPeriod.TabIndex = 1;
            this.testResetPeriod.Text = "300";
            this.testResetPeriod.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.testResetPeriod.TextChanged += new System.EventHandler(this.testResetPeriod_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 47);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "Boot Baud Rate :";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(35, 97);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "Test Period :";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(196, 97);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "second(s)";
            // 
            // ok
            // 
            this.ok.Location = new System.Drawing.Point(104, 169);
            this.ok.Name = "ok";
            this.ok.Size = new System.Drawing.Size(75, 23);
            this.ok.TabIndex = 5;
            this.ok.Text = "OK";
            this.ok.UseVisualStyleBackColor = true;
            this.ok.Click += new System.EventHandler(this.ok_Click);
            // 
            // checkInterval
            // 
            this.checkInterval.Location = new System.Drawing.Point(132, 125);
            this.checkInterval.MaxLength = 8;
            this.checkInterval.Name = "checkInterval";
            this.checkInterval.Size = new System.Drawing.Size(58, 22);
            this.checkInterval.TabIndex = 1;
            this.checkInterval.Text = "2500";
            this.checkInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.checkInterval.TextChanged += new System.EventHandler(this.checkInterval_TextChanged);
            // 
            // checkInterval_t
            // 
            this.checkInterval_t.AutoSize = true;
            this.checkInterval_t.Location = new System.Drawing.Point(12, 128);
            this.checkInterval_t.Name = "checkInterval_t";
            this.checkInterval_t.Size = new System.Drawing.Size(116, 12);
            this.checkInterval_t.TabIndex = 3;
            this.checkInterval_t.Text = "Check NMEA Interval :";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(196, 128);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(18, 12);
            this.label5.TabIndex = 4;
            this.label5.Text = "ms";
            // 
            // resetTesterLogin
            // 
            this.AcceptButton = this.ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(285, 204);
            this.Controls.Add(this.ok);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.checkInterval_t);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.checkInterval);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.testResetPeriod);
            this.Controls.Add(this.baudSelect);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "resetTesterLogin";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Reset Tester Setting";
            this.Load += new System.EventHandler(this.resetTesterLogin_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox baudSelect;
        private System.Windows.Forms.TextBox testResetPeriod;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button ok;
        private System.Windows.Forms.TextBox checkInterval;
        private System.Windows.Forms.Label checkInterval_t;
        private System.Windows.Forms.Label label5;
    }
}