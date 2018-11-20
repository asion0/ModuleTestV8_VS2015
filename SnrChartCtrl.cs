using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace ModuleTestV8
{
    public class SnrChartCtrl : System.Windows.Forms.PictureBox
    {
        public SnrChartCtrl()
        {

        }
        protected override void OnPaint(PaintEventArgs pe)
        {
            // Calling the base class OnPaint
            base.OnPaint(pe);
            //// Create two semi-transparent colors
            //Color c1 = Color.FromArgb
            //    (m_color1Transparent, m_color1);
            //Color c2 = Color.FromArgb
            //    (m_color2Transparent, m_color2);
            //Brush b = new System.Drawing.Drawing2D.LinearGradientBrush
            //    (ClientRectangle, c1, c2, 10);
            //pe.Graphics.FillRectangle(b, ClientRectangle);
            //b.Dispose();
        }
    }
}
