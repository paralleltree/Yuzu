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
        static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Application.ExecutablePath));

#if !DEBUG
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => DumpException((Exception)e.ExceptionObject, true);
#endif

            AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
            {
                if (e.RequestingAssembly == null) return null;
                string dir = Path.GetDirectoryName(e.RequestingAssembly.Location);
                string path = Path.Combine(dir, new AssemblyName(e.Name).Name + ".dll");
                return File.Exists(path) ? Assembly.LoadFile(path) : null;
            };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(args.Length == 0 ? new UI.MainForm() : new UI.MainForm(args[0]));
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
