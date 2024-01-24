using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TypingTest
{
    public partial class Form1 : Form
    {
        TransparentPanel panel;
        private Func<int, bool> fn;
        public Form1()
        {
            InitializeComponent();
            //  this.BackColor = System.Drawing.Color.Transparent;
            //   this.TransparencyKey = System.Drawing.Color.Transparent;
            addComponents();
        }

        public void addComponents()
        {
            Func<int, bool> fn= this.setMainFormHeight;
             panel = new TransparentPanel(fn);

            panel.Dock = DockStyle.Fill;
            this.Controls.Add(panel);

            //Label label = new Label();
            //label.Text = "Hello, this is some text!";
            //label.Dock = DockStyle.Fill;
            //label.Font = new Font("Arial", 16, FontStyle.Regular);
            //label.ForeColor = Color.White;
            //label.TextAlign = ContentAlignment.MiddleCenter;

            //panel.Controls.Add(label);
        }

        private bool setMainFormHeight(int height)
        {
            this.Height = height;
            return true;
        }
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            panel.OnKeyPressEvent(e);
        }


        private void labelEdit()
        {
            //label1.Text = "This is the start of the typing project";
            //label1.ForeColor = System.Drawing.Color.Black;
         //   label1.Opacity = 100;
         //   label1.T
        }

    }

}
