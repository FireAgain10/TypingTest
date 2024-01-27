using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypingTest
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using static System.Net.Mime.MediaTypeNames;

    public class TransparentTextBox : TextBox
    {
        private string text = "Hey , some Text";
        public TransparentTextBox()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Opaque, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            BackColor = Color.Transparent;
        }
        public override string Text
        {
            get { return text; }
            set
            {
                if (text != value)
                {
                    text = value;
                    Invalidate();
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Draw the background
            using (Brush backBrush = new SolidBrush(BackColor))
            {
                e.Graphics.FillRectangle(backBrush, ClientRectangle);
            }

            // Draw the text
          //  TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, Color.Black, TextFormatFlags.VerticalCenter);
        }
    }

}
