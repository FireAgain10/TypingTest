using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TypingTest
{
    internal static class Program
    {
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


    }
}
