using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Yuzu.Media;

namespace Yuzu.UI.Forms
{
    public partial class SoundSourceSelector : UserControl
    {
        public IEnumerable<string> SupportedExtensions => new string[] { ".wav", ".mp3", ".ogg" };

        public SoundSource SoundSource
        {
            get
            {
                if (string.IsNullOrEmpty(fileBox.Text)) return null;
                return new SoundSource(fileBox.Text, (double)offsetBox.Value);
            }
            set
            {
                fileBox.Text = value?.FilePath ?? "";
                offsetBox.Value = (decimal)(value?.Latency ?? 0);
            }
        }

        public SoundSourceSelector()
        {
            InitializeComponent();
            AllowDrop = true;

            browseButton.Click += (s, e) =>
            {
                var wildcards = SupportedExtensions.Select(p => "*" + p);
                var dialog = new OpenFileDialog()
                {
                    Title = "音源選択",
                    Filter = "音声ファイル" + string.Format("({0})|{1}", string.Join(", ", wildcards), string.Join(";", wildcards))
                };
                if (dialog.ShowDialog(this) == DialogResult.OK) fileBox.Text = dialog.FileName;
            };
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);
            e.Effect = DragDropEffects.None;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            var items = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (items.Length != 1) return;

            string path = items[0];
            if (SupportedExtensions.Any(p => Path.GetExtension(path) == p))
                e.Effect = DragDropEffects.Copy;
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);
            fileBox.Text = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
            offsetBox.Value = 0;
        }
    }
}
