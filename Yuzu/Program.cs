using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Yuzu
{
    static class Program
    {
        internal static readonly string ApplicationName = Assembly.GetEntryAssembly().GetName().Name;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

#if !DEBUG
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => DumpException((Exception)e.ExceptionObject, true);
#endif

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UI.MainForm());
        }

        internal static void DumpExceptionTo(Exception ex, string path)
        {
            File.WriteAllText(path, Newtonsoft.Json.JsonConvert.SerializeObject(ex));
        }

        static void DumpException(Exception ex, bool forceClose)
        {
            DumpExceptionTo(ex, "exception.json");
            if (!forceClose) return;
            MessageBox.Show("エラーが発生しました。\nアプリケーションを終了します。", ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(1);
        }
    }
}
