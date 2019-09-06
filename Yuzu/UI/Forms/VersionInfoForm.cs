using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Yuzu.Properties;

namespace Yuzu.UI.Forms
{
    public partial class VersionInfoForm : Form
    {
        public VersionInfoForm()
        {
            InitializeComponent();
            buttonClose.Click += (s, e) => Close();
            pictureBoxIcon.Image = Bitmap.FromHicon(Resources.MainIcon.Handle);

            var asm = Assembly.GetEntryAssembly();
            labelTitle.Text = string.Format("{0} - {1}", asm.GetCustomAttribute<AssemblyTitleAttribute>().Title, asm.GetCustomAttribute<AssemblyDescriptionAttribute>().Description);
            labelVersion.Text = string.Format("Version {0}", asm.GetName().Version.ToString());
            labelProduct.Text = asm.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
        }
    }
}
