using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ModuleTestV8
{
    public partial class Password : Form
    {
        public Password()
        {
            InitializeComponent();
        }

        private void ok_Click(object sender, EventArgs e)
        {
            if (0 == pwd.Text.CompareTo("skytraq28043557"))
            {
                DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                ErrorMessage.Show(ErrorMessage.Errors.PasswordError);
            }
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void Password_Load(object sender, EventArgs e)
        {
            AcceptButton = ok;
            CancelButton = cancel;
            cheats = 0;
        }

        private static int[] cheatCode = { 0x26, 0x26, 0x28, 0x28, 
                0x25, 0x27, 0x25, 0x27, 0x42, 0x41 };
        private int cheats = 0;
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            const int WM_KEYDOWN = 0x100;
            if ((msg.Msg == WM_KEYDOWN))
            {
                if (msg.WParam.ToInt32() == cheatCode[cheats])
                {
                    if (++cheats == cheatCode.Length)
                    {   //Complete Cheat.
                        DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
                else
                {
                    cheats = 0;
                }
            }
            return false;
        }
    }
}
