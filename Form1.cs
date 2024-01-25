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
        private Func<int, bool> fn;
        //public List<int> keyCodeList = new List<int>();
        // GlobalKeyboardHook keyboardHook = new GlobalKeyboardHook();
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        private static LowLevelKeyboardProc _proc;
        private static IntPtr _hookID = IntPtr.Zero;
        const int VK_A = 0x41;
        const int VK_LSHIFT = 0xA0; // Left Shift
        const int VK_RSHIFT = 0xA1; // Right Shift
        const int VK_CAPITAL = 0x14; // Caps Lock

        [DllImport("user32.dll")]
        static extern short GetKeyState(int nVirtKey);
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public Form1()
        {
            InitializeComponent();
            _proc = HookCallback;
            addComponents();
        }
        //protected override void Dispose(bool disposing)
        //{
        //    UnhookWindowsHookEx(_hookID);
        //    base.Dispose(disposing);
        //}

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (ProcessModule curModule = Process.GetCurrentProcess().MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private  IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Console.WriteLine(vkCode);
                 kbHook(vkCode);
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
        public void addComponents()
        {
            Func<int, bool> fn = this.setMainFormHeight;
            panel = new TransparentPanel(fn);

            panel.Dock = DockStyle.Fill;
            this.Controls.Add(panel);
        }
        private bool setMainFormHeight(int height)
        {
            this.Height = height;
            return true;
        }
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            panel.OnKeyPressEvent(e.KeyChar);
        }

        public void kbHook(int keycode)
        {
            Keys ch = ((Keys)keycode);
            panel.OnKeyPressEvent(((char)ch));
        }

        private bool isShiftPressed()
        {
            bool isCapsLockOn = (GetKeyState(VK_CAPITAL) & 0x0001) != 0;

            bool lShiftPressed = (GetAsyncKeyState(VK_LSHIFT) & 0x8001) != 0;
            bool rShiftPressed = (GetAsyncKeyState(VK_RSHIFT) & 0x8001) != 0;
            if  (lShiftPressed || rShiftPressed)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void labelEdit()
        {
            //label1.Text = "This is the start of the typing project";
            //label1.ForeColor = System.Drawing.Color.Black;
            //   label1.Opacity = 100;
            //   label1.T
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
              UnhookWindowsHookEx(_hookID);
            //keyboardHook.destroy();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
                 _hookID = SetHook(_proc); //Sets Global Keyboard Hook
            //keyboardHook.init();
        }
    }

}
