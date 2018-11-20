using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Resources;
using System.Reflection;
using System.Threading;
using System.Globalization;

namespace ModuleTestV8
{
    static class Program
    {
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        /// 
        public static ResourceManager rm = new ResourceManager("ModuleTestV8.LanguagePack", Assembly.GetExecutingAssembly());

        [STAThread]
        static void Main()
        {
            //Thread.CurrentThread.CurrentUICulture = new CultureInfo("en"); 
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ModuleTestForm());
        }

    }
}
