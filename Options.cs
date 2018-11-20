using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ModuleTestV8
{
    public partial class Options : Form
    {
        public Options()
        {
            InitializeComponent();
        }

        private void ok_Click(object sender, EventArgs e)
        {
            ModuleTestV8.Properties.Settings o = ModuleTestV8.Properties.Settings.Default;

            o.a1GpSnrOffset = Convert.ToDouble(a1GpSnrOffset.Text);
            o.a2GpSnrOffset = Convert.ToDouble(a2GpSnrOffset.Text);
            o.a3GpSnrOffset = Convert.ToDouble(a3GpSnrOffset.Text);
            o.a4GpSnrOffset = Convert.ToDouble(a4GpSnrOffset.Text);
            o.b1GpSnrOffset = Convert.ToDouble(b1GpSnrOffset.Text);
            o.b2GpSnrOffset = Convert.ToDouble(b2GpSnrOffset.Text);
            o.b3GpSnrOffset = Convert.ToDouble(b3GpSnrOffset.Text);
            o.b4GpSnrOffset = Convert.ToDouble(b4GpSnrOffset.Text);

            o.a1GlSnrOffset = Convert.ToDouble(a1GlSnrOffset.Text);
            o.a2GlSnrOffset = Convert.ToDouble(a2GlSnrOffset.Text);
            o.a3GlSnrOffset = Convert.ToDouble(a3GlSnrOffset.Text);
            o.a4GlSnrOffset = Convert.ToDouble(a4GlSnrOffset.Text);
            o.b1GlSnrOffset = Convert.ToDouble(b1GlSnrOffset.Text);
            o.b2GlSnrOffset = Convert.ToDouble(b2GlSnrOffset.Text);
            o.b3GlSnrOffset = Convert.ToDouble(b3GlSnrOffset.Text);
            o.b4GlSnrOffset = Convert.ToDouble(b4GlSnrOffset.Text);

            o.a1BdSnrOffset = Convert.ToDouble(a1BdSnrOffset.Text);
            o.a2BdSnrOffset = Convert.ToDouble(a2BdSnrOffset.Text);
            o.a3BdSnrOffset = Convert.ToDouble(a3BdSnrOffset.Text);
            o.a4BdSnrOffset = Convert.ToDouble(a4BdSnrOffset.Text);
            o.b1BdSnrOffset = Convert.ToDouble(b1BdSnrOffset.Text);
            o.b2BdSnrOffset = Convert.ToDouble(b2BdSnrOffset.Text);
            o.b3BdSnrOffset = Convert.ToDouble(b3BdSnrOffset.Text);
            o.b4BdSnrOffset = Convert.ToDouble(b4BdSnrOffset.Text);

            o.Save();
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void Options_Load(object sender, EventArgs e)
        {
            ModuleTestV8.Properties.Settings o = ModuleTestV8.Properties.Settings.Default;

            a1GpSnrOffset.Text = o.a1GpSnrOffset.ToString();
            a2GpSnrOffset.Text = o.a2GpSnrOffset.ToString();
            a3GpSnrOffset.Text = o.a3GpSnrOffset.ToString();
            a4GpSnrOffset.Text = o.a4GpSnrOffset.ToString();
            b1GpSnrOffset.Text = o.b1GpSnrOffset.ToString();
            b2GpSnrOffset.Text = o.b2GpSnrOffset.ToString();
            b3GpSnrOffset.Text = o.b3GpSnrOffset.ToString();
            b4GpSnrOffset.Text = o.b4GpSnrOffset.ToString();

            a1GlSnrOffset.Text = o.a1GlSnrOffset.ToString();
            a2GlSnrOffset.Text = o.a2GlSnrOffset.ToString();
            a3GlSnrOffset.Text = o.a3GlSnrOffset.ToString();
            a4GlSnrOffset.Text = o.a4GlSnrOffset.ToString();
            b1GlSnrOffset.Text = o.b1GlSnrOffset.ToString();
            b2GlSnrOffset.Text = o.b2GlSnrOffset.ToString();
            b3GlSnrOffset.Text = o.b3GlSnrOffset.ToString();
            b4GlSnrOffset.Text = o.b4GlSnrOffset.ToString();

            a1BdSnrOffset.Text = o.a1BdSnrOffset.ToString();
            a2BdSnrOffset.Text = o.a2BdSnrOffset.ToString();
            a3BdSnrOffset.Text = o.a3BdSnrOffset.ToString();
            a4BdSnrOffset.Text = o.a4BdSnrOffset.ToString();
            b1BdSnrOffset.Text = o.b1BdSnrOffset.ToString();
            b2BdSnrOffset.Text = o.b2BdSnrOffset.ToString();
            b3BdSnrOffset.Text = o.b3BdSnrOffset.ToString();
            b4BdSnrOffset.Text = o.b4BdSnrOffset.ToString();
        }

        private void SnrOffset_TextChanged(object sender, EventArgs e)
        {
            TextBox t = sender as TextBox;
            double value = 0;
            try
            {
                value = Convert.ToDouble(t.Text);
                t.ForeColor = Color.Black;
                ok.Enabled = true;
            }
            catch
            {
                t.ForeColor = Color.Red;
                ok.Enabled = false;
            }
        }

        private void reset_Click(object sender, EventArgs e)
        {
            a1GpSnrOffset.Text = "0";
            a2GpSnrOffset.Text = "0";
            a3GpSnrOffset.Text = "0";
            a4GpSnrOffset.Text = "0";
            b1GpSnrOffset.Text = "0";
            b2GpSnrOffset.Text = "0";
            b3GpSnrOffset.Text = "0";
            b4GpSnrOffset.Text = "0";

            a1GlSnrOffset.Text = "0";
            a2GlSnrOffset.Text = "0";
            a3GlSnrOffset.Text = "0";
            a4GlSnrOffset.Text = "0";
            b1GlSnrOffset.Text = "0";
            b2GlSnrOffset.Text = "0";
            b3GlSnrOffset.Text = "0";
            b4GlSnrOffset.Text = "0";

            a1BdSnrOffset.Text = "0";
            a2BdSnrOffset.Text = "0";
            a3BdSnrOffset.Text = "0";
            a4BdSnrOffset.Text = "0";
            b1BdSnrOffset.Text = "0";
            b2BdSnrOffset.Text = "0";
            b3BdSnrOffset.Text = "0";
            b4BdSnrOffset.Text = "0";
        }
    }
}
