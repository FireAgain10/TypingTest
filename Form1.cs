using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TypingTest
{
    public partial class Form1 : Form
    {
        TransparentPanel panel;
        private bool isDragging = false;
        private Point offset;

        private GlobalKeyboardHook kbHook;
        public Form1()
        {
            InitializeComponent();
            Func<char, bool> kblistenerFunc = this.kbHookEvent;
            if (Program.enableGlobalKeyListener) kbHook = new GlobalKeyboardHook(kblistenerFunc);
            // _proc = HookCallback;
            addComponents();
        }
        public void addComponents()
        {
            Screen primaryScreen = Screen.PrimaryScreen;
            Program.leftRightMargin = (Program.leftRightMargin > 0.2) ? 0.2 : Program.leftRightMargin;
            Program.topMargin = (Program.topMargin > 0.8) ? 0.8 : Program.topMargin;

            var width = (1 - (2 * Program.leftRightMargin));
            this.Width = (int) (primaryScreen.Bounds.Width*width);

            this.Location = new Point((int)(primaryScreen.Bounds.Width * Program.leftRightMargin), (int)(primaryScreen.Bounds.Height * Program.topMargin));
            Func<int, bool> fn = this.setMainFormHeight;
            panel = new TransparentPanel(fn, this.Width);

            panel.Dock = DockStyle.Fill;
            this.Controls.Add(panel);

            panel.MouseDown += Form1_MouseDown;
            panel.MouseMove += Form1_MouseMove;
            panel.MouseUp += Form1_MouseUp;
        }
        private bool setMainFormHeight(int height)
        {
            int titleBarHeight = SystemInformation.CaptionHeight;
            this.Height = height + titleBarHeight;
            return true;
        }

        public bool kbHookEvent(char ch)
        {
            panel.OnKeyPressEvent(ch);
            return true;
        }
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            char ch = e.KeyChar;
            if (!Program.enableGlobalKeyListener) panel.OnKeyPressEvent(ch);
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            kbHook.destroy();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }


        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                offset = new Point(e.X, e.Y);
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point newLocation = this.PointToScreen(new Point(e.X, e.Y));
                newLocation.Offset(-offset.X, -offset.Y);
                this.Location = newLocation;
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        }
    }

}
