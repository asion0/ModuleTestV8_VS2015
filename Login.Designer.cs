namespace ModuleTestV8
{
    partial class Login
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Login));
            this.label1 = new System.Windows.Forms.Label();
            this.testerNo = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.rework = new System.Windows.Forms.RadioButton();
            this.firstTest = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.fixtureNo = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.workNo = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.profilePath = new System.Windows.Forms.TextBox();
            this.profileSelect = new System.Windows.Forms.Button();
            this.ok = new System.Windows.Forms.Button();
            this.cancel = new System.Windows.Forms.Button();
            this.debugMode = new System.Windows.Forms.Button();
            this.testBtn = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "測試人員編號";
            // 
            // testerNo
            // 
            this.testerNo.Location = new System.Drawing.Point(97, 21);
            this.testerNo.Name = "testerNo";
            this.testerNo.Size = new System.Drawing.Size(439, 22);
            this.testerNo.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.rework);
            this.panel1.Controls.Add(this.firstTest);
            this.panel1.Location = new System.Drawing.Point(15, 60);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(521, 20);
            this.panel1.TabIndex = 1;
            // 
            // rework
            // 
            this.rework.AutoSize = true;
            this.rework.Location = new System.Drawing.Point(256, 4);
            this.rework.Name = "rework";
            this.rework.Size = new System.Drawing.Size(83, 16);
            this.rework.TabIndex = 1;
            this.rework.TabStop = true;
            this.rework.Text = "重測、重工";
            this.rework.UseVisualStyleBackColor = true;
            this.rework.CheckedChanged += new System.EventHandler(this.rework_CheckedChanged);
            // 
            // firstTest
            // 
            this.firstTest.AutoSize = true;
            this.firstTest.Location = new System.Drawing.Point(4, 3);
            this.firstTest.Name = "firstTest";
            this.firstTest.Size = new System.Drawing.Size(83, 16);
            this.firstTest.TabIndex = 0;
            this.firstTest.TabStop = true;
            this.firstTest.Text = "第一次測試";
            this.firstTest.UseVisualStyleBackColor = true;
            this.firstTest.CheckedChanged += new System.EventHandler(this.firstTest_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 105);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "測試治具編號";
            // 
            // fixtureNo
            // 
            this.fixtureNo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.fixtureNo.FormattingEnabled = true;
            this.fixtureNo.Location = new System.Drawing.Point(97, 100);
            this.fixtureNo.Name = "fixtureNo";
            this.fixtureNo.Size = new System.Drawing.Size(439, 20);
            this.fixtureNo.TabIndex = 2;
            this.fixtureNo.SelectedIndexChanged += new System.EventHandler(this.fixtureNo_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 147);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "工單號碼";
            // 
            // workNo
            // 
            this.workNo.Location = new System.Drawing.Point(97, 140);
            this.workNo.Name = "workNo";
            this.workNo.Size = new System.Drawing.Size(439, 22);
            this.workNo.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 186);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 7;
            this.label4.Text = "測試設定檔";
            // 
            // profilePath
            // 
            this.profilePath.Location = new System.Drawing.Point(97, 183);
            this.profilePath.Name = "profilePath";
            this.profilePath.Size = new System.Drawing.Size(403, 22);
            this.profilePath.TabIndex = 4;
            // 
            // profileSelect
            // 
            this.profileSelect.Location = new System.Drawing.Point(507, 181);
            this.profileSelect.Name = "profileSelect";
            this.profileSelect.Size = new System.Drawing.Size(29, 23);
            this.profileSelect.TabIndex = 4;
            this.profileSelect.Text = "...";
            this.profileSelect.UseVisualStyleBackColor = true;
            this.profileSelect.Click += new System.EventHandler(this.profileSelect_Click);
            // 
            // ok
            // 
            this.ok.Location = new System.Drawing.Point(382, 293);
            this.ok.Name = "ok";
            this.ok.Size = new System.Drawing.Size(75, 45);
            this.ok.TabIndex = 5;
            this.ok.Text = "OK";
            this.ok.UseVisualStyleBackColor = true;
            this.ok.Click += new System.EventHandler(this.ok_Click);
            // 
            // cancel
            // 
            this.cancel.Location = new System.Drawing.Point(464, 293);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(75, 45);
            this.cancel.TabIndex = 6;
            this.cancel.Text = "Cancel";
            this.cancel.UseVisualStyleBackColor = true;
            this.cancel.Click += new System.EventHandler(this.cancel_Click);
            // 
            // debugMode
            // 
            this.debugMode.Location = new System.Drawing.Point(260, 315);
            this.debugMode.Name = "debugMode";
            this.debugMode.Size = new System.Drawing.Size(75, 23);
            this.debugMode.TabIndex = 7;
            this.debugMode.Text = "Debug Mode";
            this.debugMode.UseVisualStyleBackColor = true;
            this.debugMode.Click += new System.EventHandler(this.debugMode_Click);
            // 
            // testBtn
            // 
            this.testBtn.Location = new System.Drawing.Point(19, 315);
            this.testBtn.Name = "testBtn";
            this.testBtn.Size = new System.Drawing.Size(75, 23);
            this.testBtn.TabIndex = 8;
            this.testBtn.Text = "test";
            this.testBtn.UseVisualStyleBackColor = true;
            this.testBtn.Visible = false;
            this.testBtn.Click += new System.EventHandler(this.test_Click);
            // 
            // Login
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(553, 350);
            this.Controls.Add(this.testBtn);
            this.Controls.Add(this.debugMode);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.ok);
            this.Controls.Add(this.profileSelect);
            this.Controls.Add(this.profilePath);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.workNo);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.fixtureNo);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.testerNo);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Login";
            this.Text = "Login";
            this.Load += new System.EventHandler(this.Login_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox testerNo;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton rework;
        private System.Windows.Forms.RadioButton firstTest;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox fixtureNo;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox workNo;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox profilePath;
        private System.Windows.Forms.Button profileSelect;
        private System.Windows.Forms.Button ok;
        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.Button debugMode;
        private System.Windows.Forms.Button testBtn;
    }
}