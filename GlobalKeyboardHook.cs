using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TypingTest
{
    using System;
    using System.Diagnostics;
    using System.Reflection.Emit;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using static System.Runtime.CompilerServices.RuntimeHelpers;

    public class GlobalKeyboardHook
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        const int VK_LSHIFT = 0xA0; // Left Shift
        const int VK_RSHIFT = 0xA1; // Right Shift
        private const int VK_SHIFT = 0x10;
        const int VK_CAPITAL = 0x14; // Caps Lock

        [DllImport("user32.dll")]
        static extern short GetKeyState(int nVirtKey);
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);
        public List<int> keyCodeList = new List<int>();

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static LowLevelKeyboardProc _proc;
        private static IntPtr _hookID = IntPtr.Zero;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int ToUnicode(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPArray)] char[] pwszBuff, int cchBuff, uint wFlags);

        Func<char, bool> sendCharPressed;
        public GlobalKeyboardHook(Func<char, bool> callback)
        {
            sendCharPressed = callback;
            init();
        }

        public void init()
        {
            _proc = HookCallback;
            _hookID = SetHook(_proc);
        }

        public void destroy()
        {
            UnhookWindowsHookEx(_hookID);
            Console.WriteLine("Keyboard Hook Released");
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (ProcessModule module = Process.GetCurrentProcess().MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(module.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                bool capsLockOn = (GetKeyState(VK_CAPITAL) & 0x0001) != 0;
                bool shiftPressed = (GetKeyState(VK_SHIFT) & 0x8000) != 0;
                finalChar(vkCode, capsLockOn, shiftPressed);
                //  sendCharPressed(ch);
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private void finalChar(int keyCode, bool capsLock, bool shiftPressed)
        {
            char ch = GetCharFromKey(keyCode, capsLock, shiftPressed);
            if (isValidChar(ch) || ((short)Keys.Back == ((short)ch)))
            {
                //Console.WriteLine($"shiftPressed 3: {shiftPressed} : {ch}");
                sendCharPressed(ch);
            }
        }

        private static char GetCharFromKey(int virtualKeyCode, bool capsLockOn, bool shiftPressed)
        {
            byte[] keyState = new byte[256];
            GetKeyboardState(keyState);

            if (capsLockOn)
            {
                keyState[VK_CAPITAL] = 1;
            }

            char[] buffer = new char[2];
            int result = ToUnicode((uint)virtualKeyCode, 0, keyState, buffer, buffer.Length, 0);

            if (result == 1)
            {
                char ch = buffer[0];

                if (shiftPressed)
                {
                    if (char.IsUpper(ch))
                    {
                       char.ToLower(ch);
                    }
                    else if (char.IsLower(ch))
                    {
                        char.ToUpper(ch);
                    }
                }
                return ch;
            }
            else { return '\0'; }

        }

        private bool isValidChar(char ch)
        {

            if (char.IsLetterOrDigit(ch) || char.IsSymbol(ch) || char.IsPunctuation(ch) || char.IsSeparator(ch) || char.IsWhiteSpace(ch))
            {
                return true;
            }
            return false;
        }


    }

}
