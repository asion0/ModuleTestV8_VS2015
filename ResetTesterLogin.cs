using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace ModuleTestV8
{
    public partial class resetTesterLogin : Form
    {
        public static int BootBaudRate = 1;
        public static int TestPeriod = 120;
        public static int CheckInterval = 1100;
        public static DateTime loginTime = DateTime.Now;
        public static String currentPath;

        public resetTesterLogin()
        {
            InitializeComponent();
        }

        private void resetTesterLogin_Load(object sender, EventArgs e)
        {
            if (Environment.GetEnvironmentVariable("sfxname") == null)
            {
                resetTesterLogin.currentPath = Environment.CurrentDirectory;
            }
            else
            {   //For WinRar sfx package.
                resetTesterLogin.currentPath = Path.GetDirectoryName(Environment.GetEnvironmentVariable("sfxname"));
            } 
            
            Global.InjectionBaudRate(baudSelect);
            baudSelect.SelectedIndex = 1;
            if (Global.functionType == Global.FunctionType.iCacheTester)
            {
                checkInterval.Visible = false;
                checkInterval_t.Visible = false;
            }
            //testResetPeriod.Text = "120";
        }

        private void ok_Click(object sender, EventArgs e)
        {
            BootBaudRate = baudSelect.SelectedIndex;
            TestPeriod = Convert.ToInt32(testResetPeriod.Text);
            CheckInterval = Convert.ToInt32(checkInterval.Text);
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void testResetPeriod_TextChanged(object sender, EventArgs e)
        {
            ok.Enabled = (Global.GetTextBoxPositiveInt(testResetPeriod) > 0);
        }

        private void checkInterval_TextChanged(object sender, EventArgs e)
        {
            ok.Enabled = (Global.GetTextBoxPositiveInt(checkInterval) > 0);
        }
    }
}
