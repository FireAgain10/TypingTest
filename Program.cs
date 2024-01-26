using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TypingTest
{
    internal static class Program
    {
        public static bool charByCharPrint = true;
        public static bool enableGlobalKeyListener = false;
        public static bool highlightMistake = true;
        public static double leftRightMargin = 0.15, topMargin = 0.6;
        public static int fontWidth = 20;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {  
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Form1 transparentTextForm = new Form1();
            transparentTextForm.FormBorderStyle = FormBorderStyle.None;
            //transparentTextForm.BackColor = System.Drawing.Color.FromArgb(1, 1, 1, 1); // Transparent background
            //transparentTextForm.TransparencyKey = System.Drawing.Color.FromArgb(0, 0, 0, 0); // Set transparency key


            Application.Run(transparentTextForm);
        }

        public static void printCheckpoint(String str,bool printFlag)
        {
           if  (printFlag) {
                Console.WriteLine(str);
            }
        }


    }
}
