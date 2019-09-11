using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Registration;

namespace Yuzu.Plugins
{
    public class PluginManager
    {
        internal static string PluginPath => "Plugins";

        public List<string> FailedFiles { get; private set; } = new List<string>();

        private PluginManager()
        {
        }

        public static PluginManager GetInstance()
        {
            var builder = new RegistrationBuilder();
            builder.ForTypesDerivedFrom<IPlugin>().ExportInterfaces();
            builder.ForType<PluginManager>().Export<PluginManager>();

            var self = new AssemblyCatalog(typeof(PluginManager).Assembly, builder);
            var catalog = new AggregateCatalog(self);
            var failed = new List<string>();

            if (Directory.Exists(PluginPath))
            {
                foreach (string path in Directory.EnumerateFiles(PluginPath, "*.dll", SearchOption.AllDirectories))
                {
                    try
                    {
                        var asm = Assembly.LoadFile(Path.GetFullPath(path));
                        catalog.Catalogs.Add(new AssemblyCatalog(asm, builder));
                    }
                    catch (Exception ex) when (ex is NotSupportedException || ex is BadImageFormatException)
                    {
                        failed.Add(path);
                    }
                }
            }

            var container = new CompositionContainer(catalog);
            var manager = container.GetExportedValue<PluginManager>();
            manager.FailedFiles = failed;
            return manager;
        }
    }
}
