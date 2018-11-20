using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ModuleTestV8
{
    public partial class WaitingGoldenForm : Form
    {
        public ModuleTestForm main = null;
        public WaitingGoldenForm()
        {
            InitializeComponent();
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            main.hiddenNotify.Text = "WaitingCancel";
            this.DialogResult = DialogResult.Cancel;
            this.Hide();
        }

        private void WaitingGoldenForm_Load(object sender, EventArgs e)
        {
            
        }
    }
}
