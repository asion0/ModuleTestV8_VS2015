using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ModuleTestV8
{
    public partial class MyMessageBox : Form
    {
        public MyMessageBox()
        {
            InitializeComponent();
        }

        private void MyMessageBox_Load(object sender, EventArgs e)
        {
            msg.Text = message;
            this.Text = title;
        }
        //MessageBox.Show("Roteat to 90.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        public static int Show(string message, string title)
        {
            MyMessageBox mbox = new MyMessageBox();
            mbox.SetMessage(message, title);
            mbox.ShowDialog();
            
            return 0;
        }

        private string title = "";
        private string message = "";
        public void SetMessage(string msg, string tle)
        {
            message = msg;
            title = tle;
        }

        private void ok_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
